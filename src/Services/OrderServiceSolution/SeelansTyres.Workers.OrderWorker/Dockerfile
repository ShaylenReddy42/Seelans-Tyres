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
COPY ["Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/SeelansTyres.Models.IdentityModels.csproj", "Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/SeelansTyres.Models.TyresModels.csproj", "Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Data.OrderData/SeelansTyres.Data.OrderData.csproj", "Services/OrderServiceSolution/SeelansTyres.Data.OrderData/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Workers.OrderWorker/SeelansTyres.Workers.OrderWorker.csproj", "Services/OrderServiceSolution/SeelansTyres.Workers.OrderWorker/"]
RUN dotnet restore "Services/OrderServiceSolution/SeelansTyres.Workers.OrderWorker/SeelansTyres.Workers.OrderWorker.csproj"
COPY . .
WORKDIR "/src/Services/OrderServiceSolution/SeelansTyres.Workers.OrderWorker"
RUN dotnet build "SeelansTyres.Workers.OrderWorker.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Workers.OrderWorker.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Services/OrderServiceSolution/SeelansTyres.Workers.OrderWorker/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Workers.OrderWorker.dll"]