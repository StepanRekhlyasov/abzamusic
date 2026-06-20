FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./

FROM base AS build-stage
RUN dotnet publish -c Release -o out

FROM base AS dev-stage
EXPOSE 8080
CMD ["dotnet", "watch", "run", "--non-interactive", "--no-launch-profile"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production-stage
WORKDIR /app
COPY --from=build-stage /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]
