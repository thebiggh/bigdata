FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY twitter_stream.csproj twitter_stream.csproj
RUN dotnet restore twitter_stream.csproj
COPY Program.cs Program.cs
RUN dotnet build twitter_stream.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish twitter_stream.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "twitter_stream.dll"]