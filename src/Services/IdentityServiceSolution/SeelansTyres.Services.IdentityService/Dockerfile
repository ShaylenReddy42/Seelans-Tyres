#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN update-ca-certificates
EXPOSE 8080
ENV InContainer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/ShaylenReddy42.UnpublishedUpdatesManagement.csproj", "Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/"]
COPY ["Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/SeelansTyres.Models.IdentityModels.csproj", "Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/"]
COPY ["Services/IdentityServiceSolution/SeelansTyres.Services.IdentityService/SeelansTyres.Services.IdentityService.csproj", "Services/IdentityServiceSolution/SeelansTyres.Services.IdentityService/"]
RUN dotnet restore "Services/IdentityServiceSolution/SeelansTyres.Services.IdentityService/SeelansTyres.Services.IdentityService.csproj"
COPY . .
WORKDIR "/src/Services/IdentityServiceSolution/SeelansTyres.Services.IdentityService"
RUN dotnet build "SeelansTyres.Services.IdentityService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.IdentityService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.IdentityService.dll"]