#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN apk add --upgrade --no-cache ca-certificates && update-ca-certificates
RUN update-ca-certificates
EXPOSE 8080
ENV InContainer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/ShaylenReddy42.UnpublishedUpdatesManagement.csproj", "Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/"]
COPY ["Services/AddressServiceSolution/SeelansTyres.Data.AddressData/SeelansTyres.Data.AddressData.csproj", "Services/AddressServiceSolution/SeelansTyres.Data.AddressData/"]
COPY ["Services/AddressServiceSolution/SeelansTyres.Workers.AddressWorker/SeelansTyres.Workers.AddressWorker.csproj", "Services/AddressServiceSolution/SeelansTyres.Workers.AddressWorker/"]
RUN dotnet restore "Services/AddressServiceSolution/SeelansTyres.Workers.AddressWorker/SeelansTyres.Workers.AddressWorker.csproj"
COPY . .
WORKDIR "/src/Services/AddressServiceSolution/SeelansTyres.Workers.AddressWorker"
RUN dotnet build "SeelansTyres.Workers.AddressWorker.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Workers.AddressWorker.csproj" -c Release -r linux-x64 --no-self-contained -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Workers.AddressWorker.dll"]