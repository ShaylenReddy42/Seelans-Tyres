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
COPY ["Services/AddressServiceSolution/SeelansTyres.Models.AddressModels/SeelansTyres.Models.AddressModels.csproj", "Services/AddressServiceSolution/SeelansTyres.Models.AddressModels/"]
COPY ["Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/SeelansTyres.Models.IdentityModels.csproj", "Services/IdentityServiceSolution/SeelansTyres.Models.IdentityModels/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Models.OrderModels/SeelansTyres.Models.OrderModels.csproj", "Services/OrderServiceSolution/SeelansTyres.Models.OrderModels/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/SeelansTyres.Models.TyresModels.csproj", "Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/"]
COPY ["Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/SeelansTyres.Frontends.Mvc.csproj", "Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/"]
RUN dotnet restore "Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/SeelansTyres.Frontends.Mvc.csproj"
COPY . .
WORKDIR "/src/Frontends/MvcSolution/SeelansTyres.Frontends.Mvc"
RUN dotnet build "SeelansTyres.Frontends.Mvc.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Frontends.Mvc.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Frontends.Mvc.dll"]