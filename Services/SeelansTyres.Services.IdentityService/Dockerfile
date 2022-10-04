#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/SeelansTyres.Services.IdentityService/SeelansTyres.Services.IdentityService.csproj", "Services/SeelansTyres.Services.IdentityService/"]
RUN dotnet restore "Services/SeelansTyres.Services.IdentityService/SeelansTyres.Services.IdentityService.csproj"
COPY . .
WORKDIR "/src/Services/SeelansTyres.Services.IdentityService"
RUN dotnet build "SeelansTyres.Services.IdentityService.csproj" -c Release -r linux-x64 -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.IdentityService.csproj" -c Release -r linux-x64 /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Services/SeelansTyres.Services.IdentityService/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.IdentityService.dll"]