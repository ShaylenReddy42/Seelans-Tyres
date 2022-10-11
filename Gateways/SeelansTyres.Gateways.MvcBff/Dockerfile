#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Libraries/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SeelansTyres.Libraries.Shared/"]
COPY ["Gateways/SeelansTyres.Gateways.MvcBff/SeelansTyres.Gateways.MvcBff.csproj", "Gateways/SeelansTyres.Gateways.MvcBff/"]
RUN dotnet restore "Gateways/SeelansTyres.Gateways.MvcBff/SeelansTyres.Gateways.MvcBff.csproj"
COPY . .
WORKDIR "/src/Gateways/SeelansTyres.Gateways.MvcBff"
RUN dotnet build "SeelansTyres.Gateways.MvcBff.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Gateways.MvcBff.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Gateways/SeelansTyres.Gateways.MvcBff/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Gateways.MvcBff.dll"]