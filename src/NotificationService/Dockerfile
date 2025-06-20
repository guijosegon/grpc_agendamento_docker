FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/NotificationService/NotificationService.csproj .
COPY src/NotificationService/protos ./protos
RUN dotnet restore NotificationService.csproj
COPY src/NotificationService/. .
RUN dotnet publish -c Release -o /app NotificationService.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "NotificationService.dll"]