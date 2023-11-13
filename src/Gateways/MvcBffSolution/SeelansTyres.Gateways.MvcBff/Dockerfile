#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN update-ca-certificates
EXPOSE 8080
ENV InContainer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/ShaylenReddy42.UnpublishedUpdatesManagement.csproj", "Libraries/UnpublishedUpdatesManagementSolution/ShaylenReddy42.UnpublishedUpdatesManagement/"]
COPY ["Gateways/MvcBffSolution/SeelansTyres.Gateways.MvcBff/SeelansTyres.Gateways.MvcBff.csproj", "Gateways/MvcBffSolution/SeelansTyres.Gateways.MvcBff/"]
RUN dotnet restore "Gateways/MvcBffSolution/SeelansTyres.Gateways.MvcBff/SeelansTyres.Gateways.MvcBff.csproj"
COPY . .
WORKDIR "/src/Gateways/MvcBffSolution/SeelansTyres.Gateways.MvcBff"
RUN dotnet build "SeelansTyres.Gateways.MvcBff.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Gateways.MvcBff.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
RUN addgroup -S nonroot && adduser -S nonroot -G nonroot
USER nonroot
WORKDIR /app
COPY --from=publish /src/Gateways/MvcBffSolution/SeelansTyres.Gateways.MvcBff/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Gateways.MvcBff.dll"]