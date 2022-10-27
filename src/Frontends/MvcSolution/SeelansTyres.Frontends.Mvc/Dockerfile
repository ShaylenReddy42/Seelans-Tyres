#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/SeelansTyres.Frontends.Mvc.csproj", "Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/"]
RUN dotnet restore "Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/SeelansTyres.Frontends.Mvc.csproj"
COPY . .
WORKDIR "/src/Frontends/MvcSolution/SeelansTyres.Frontends.Mvc"
RUN dotnet build "SeelansTyres.Frontends.Mvc.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Frontends.Mvc.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Frontends/MvcSolution/SeelansTyres.Frontends.Mvc/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Frontends.Mvc.dll"]