#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN update-ca-certificates
EXPOSE 8080
ENV InContainer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/ShaylenReddy42.UnpublishedUpdatesManagement.csproj", "Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/"]
COPY ["Services/AddressServiceSolution/SeelansTyres.Data.AddressData/SeelansTyres.Data.AddressData.csproj", "Services/AddressServiceSolution/SeelansTyres.Data.AddressData/"]
COPY ["Services/AddressServiceSolution/SeelansTyres.Models.AddressModels/SeelansTyres.Models.AddressModels.csproj", "Services/AddressServiceSolution/SeelansTyres.Models.AddressModels/"]
COPY ["Services/AddressServiceSolution/SeelansTyres.Services.AddressService/SeelansTyres.Services.AddressService.csproj", "Services/AddressServiceSolution/SeelansTyres.Services.AddressService/"]
RUN dotnet restore "Services/AddressServiceSolution/SeelansTyres.Services.AddressService/SeelansTyres.Services.AddressService.csproj"
COPY . .
WORKDIR "/src/Services/AddressServiceSolution/SeelansTyres.Services.AddressService"
RUN dotnet build "SeelansTyres.Services.AddressService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.AddressService.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Services/AddressServiceSolution/SeelansTyres.Services.AddressService/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.AddressService.dll"]