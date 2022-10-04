@ECHO off

CD ..

MORE RequiredEnvironmentVariables.txt

ECHO.
PAUSE

ECHO.
ECHO Restore dotnet tools
ECHO.
dotnet tool restore

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.AddressService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project Services/SeelansTyres.Services.AddressService/SeelansTyres.Services.AddressService.csproj --startup-project Services/SeelansTyres.Services.AddressService/SeelansTyres.Services.AddressService.csproj -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresAddressContext%"

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.OrderService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project Services/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj --startup-project Services/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresOrderContext%"

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.TyresService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project Services/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj --startup-project Services/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresTyresContext%"

ECHO.
ECHO Run CMake
ECHO.
cmake -G "Visual Studio 17" -S . -B build