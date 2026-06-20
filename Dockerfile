# ==================== Backend ====================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-base
WORKDIR /app
COPY backend/*.csproj ./
RUN dotnet restore
COPY backend/ ./

FROM backend-base AS backend-build
RUN dotnet publish -c Release -o out

FROM backend-base AS backend-dev-stage
EXPOSE 8080
CMD ["dotnet", "watch", "run", "--non-interactive", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS backend-production-stage
WORKDIR /app
COPY --from=backend-build /app/out .
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
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "80"]

FROM nginx:stable-alpine AS frontend-production-stage
COPY --from=frontend-build /app/dist/spa /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
