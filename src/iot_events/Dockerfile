FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY iot_events.csproj iot_events.csproj
RUN dotnet restore iot_events.csproj
COPY Program.cs Program.cs
RUN dotnet build iot_events.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish iot_events.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "iot_events.dll"]