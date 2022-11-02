#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/SeelansTyres.Libraries.Shared.csproj", "Libraries/SharedLibrarySolution/SeelansTyres.Libraries.Shared/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Data.OrderData/SeelansTyres.Data.OrderData.csproj", "Services/OrderServiceSolution/SeelansTyres.Data.OrderData/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Models.OrderModels/SeelansTyres.Models.OrderModels.csproj", "Services/OrderServiceSolution/SeelansTyres.Models.OrderModels/"]
COPY ["Services/OrderServiceSolution/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj", "Services/OrderServiceSolution/SeelansTyres.Services.OrderService/"]
RUN dotnet restore "Services/OrderServiceSolution/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj"
COPY . .
WORKDIR "/src/Services/OrderServiceSolution/SeelansTyres.Services.OrderService"
RUN dotnet build "SeelansTyres.Services.OrderService.csproj" -c Release -r linux-x64 --no-self-contained -o /app/build

FROM build AS publish
RUN dotnet publish "SeelansTyres.Services.OrderService.csproj" -c Release -r linux-x64 --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /src/Services/OrderServiceSolution/SeelansTyres.Services.OrderService/publish .
ENTRYPOINT ["dotnet", "SeelansTyres.Services.OrderService.dll"]