# ==================== Python (musiclang_predict needs 3.11; Ubuntu 24.04 ships 3.12) ====================
FROM python:3.11-slim-bookworm AS python-deps
WORKDIR /build
RUN apt-get update \
    && apt-get install -y --no-install-recommends build-essential \
    && rm -rf /var/lib/apt/lists/*
COPY scripts/requirements.txt .
RUN python -m venv --copies /build/venv \
    && /build/venv/bin/pip install --no-cache-dir --upgrade pip setuptools wheel \
    && /build/venv/bin/pip install --no-cache-dir "numpy>=1.23.5,<2" \
    && /build/venv/bin/pip install --no-cache-dir torch --index-url https://download.pytorch.org/whl/cpu \
    && /build/venv/bin/pip install --no-cache-dir -r requirements.txt

# ==================== Backend ====================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-base
WORKDIR /app
COPY backend/*.csproj ./
RUN dotnet restore
COPY backend/ ./

FROM backend-base AS backend-python
RUN apt-get update \
    && apt-get install -y --no-install-recommends fluidsynth fluid-soundfont-gm \
    && rm -rf /var/lib/apt/lists/*
COPY --from=python-deps /usr/local/lib/python3.11 /usr/local/lib/python3.11
COPY --from=python-deps /usr/local/lib/libpython3.11.so* /usr/local/lib/
COPY --from=python-deps /build/venv /app/venv
COPY scripts/ /app/scripts/
ENV LD_LIBRARY_PATH=/usr/local/lib
ENV MusicGeneration__PythonPath=/app/venv/bin/python3

FROM backend-python AS backend-build
RUN dotnet publish -c Release -o out

FROM backend-python AS backend-dev-stage
EXPOSE 8080
CMD ["dotnet", "watch", "run", "--non-interactive", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS backend-production-stage
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends fluidsynth fluid-soundfont-gm \
    && rm -rf /var/lib/apt/lists/*
COPY --from=python-deps /usr/local/lib/python3.11 /usr/local/lib/python3.11
COPY --from=python-deps /usr/local/lib/libpython3.11.so* /usr/local/lib/
COPY --from=python-deps /build/venv /app/venv
COPY scripts/ /app/scripts/
COPY --from=backend-build /app/out .
ENV LD_LIBRARY_PATH=/usr/local/lib
ENV MusicGeneration__PythonPath=/app/venv/bin/python3
EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]

# ==================== Frontend ====================
FROM node:24-alpine AS frontend-base
WORKDIR /app
COPY frontend/package*.json ./
RUN npm install --ignore-scripts
COPY frontend/ ./
RUN npm run postinstall

FROM frontend-base AS frontend-build
RUN npm run build

FROM frontend-base AS frontend-dev-stage
EXPOSE 80
CMD ["npm", "run", "dev", "--", "-H", "0.0.0.0", "-p", "80"]

FROM nginx:stable-alpine AS frontend-production-stage
COPY --from=frontend-build /app/dist/spa /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

# ==================== Production (Render) ====================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production-stage
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends nginx fluidsynth fluid-soundfont-gm \
    && rm -f /etc/nginx/sites-enabled/default \
    && rm -rf /var/lib/apt/lists/*
COPY --from=python-deps /usr/local/lib/python3.11 /usr/local/lib/python3.11
COPY --from=python-deps /usr/local/lib/libpython3.11.so* /usr/local/lib/
COPY --from=python-deps /build/venv /app/venv
COPY scripts/ /app/scripts/
COPY --from=backend-build /app/out .
ENV LD_LIBRARY_PATH=/usr/local/lib
ENV MusicGeneration__PythonPath=/app/venv/bin/python3
COPY --from=frontend-build /app/dist/spa /usr/share/nginx/html
COPY frontend/nginx.single.conf /etc/nginx/conf.d/default.conf
COPY entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
