FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ScheduleService/ScheduleService.csproj .
COPY src/ScheduleService/protos ./protos
RUN dotnet restore ScheduleService.csproj
COPY src/ScheduleService/. .
RUN dotnet publish -c Release -o /app ScheduleService.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ScheduleService.dll"]