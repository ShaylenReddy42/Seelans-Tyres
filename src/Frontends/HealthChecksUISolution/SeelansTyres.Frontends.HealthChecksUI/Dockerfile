#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Frontends/HealthChecksUISolution/SeelansTyres.Frontends.HealthChecksUI/SeelansTyres.Frontends.HealthChecksUI.csproj", "Frontends/HealthChecksUISolution/SeelansTyres.Frontends.HealthChecksUI/"]
RUN dotnet restore "Frontends/HealthChecksUISolution/SeelansTyres.Frontends.HealthChecksUI/SeelansTyres.Frontends.HealthChecksUI.csproj"
COPY . .
WORKDIR "/src/Frontends/HealthChecksUISolution/SeelansTyres.Frontends.HealthChecksUI"
RUN dotnet build "SeelansTyres.Frontends.HealthChecksUI.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Frontends.HealthChecksUI.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Frontends/HealthChecksUISolution/SeelansTyres.Frontends.HealthChecksUI/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Frontends.HealthChecksUI.dll"]