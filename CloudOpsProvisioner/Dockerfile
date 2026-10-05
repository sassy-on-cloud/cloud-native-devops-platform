# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["CloudOpsProvisioner.csproj", "./"]

RUN dotnet restore "CloudOpsProvisioner.csproj"

COPY . .

RUN dotnet publish "CloudOpsProvisioner.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CloudOpsProvisioner.dll"]
