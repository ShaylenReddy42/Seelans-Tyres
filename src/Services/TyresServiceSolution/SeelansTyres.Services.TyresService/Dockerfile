#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN apk add --upgrade --no-cache ca-certificates && update-ca-certificates
RUN update-ca-certificates
EXPOSE 8080
ENV InContainer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/ShaylenReddy42.UnpublishedUpdatesManagement.csproj", "Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/SeelansTyres.Models.TyresModels.csproj", "Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj", "Services/TyresServiceSolution/SeelansTyres.Services.TyresService/"]
RUN dotnet restore "Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj"
COPY . .
WORKDIR "/src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService"
RUN dotnet build "SeelansTyres.Services.TyresService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.TyresService.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.TyresService.dll"]