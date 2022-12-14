#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY self-signed-certificates/ /usr/local/share/ca-certificates/
RUN update-ca-certificates
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/SeelansTyres.Models.TyresModels.csproj", "Services/TyresServiceSolution/SeelansTyres.Models.TyresModels/"]
COPY ["Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj", "Services/TyresServiceSolution/SeelansTyres.Services.TyresService/"]
RUN dotnet restore "Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj"
COPY . .
WORKDIR "/src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService"
RUN dotnet build "SeelansTyres.Services.TyresService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.TyresService.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.TyresService.dll"]