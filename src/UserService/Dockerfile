FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/UserService/UserService.csproj .
COPY src/UserService/protos ./protos
RUN dotnet restore UserService.csproj
COPY src/UserService/. .
RUN dotnet publish -c Release -o /app UserService.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "UserService.dll"]