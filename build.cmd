@ECHO off

CD "%~dp0"

MORE RequiredEnvironmentVariables.txt

ECHO.
PAUSE

ECHO.
ECHO Restore dotnet tools
ECHO.
dotnet tool restore

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.WebApi
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project Services/SeelansTyres.WebApi/SeelansTyres.WebApi.csproj --startup-project Services/SeelansTyres.WebApi/SeelansTyres.WebApi.csproj -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresContext%"

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

CD Frontend/SeelansTyres.Mvc

ECHO.
ECHO SeelansTyres.Mvc
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the Frontend Project
ECHO.
dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.WebApi

ECHO.
ECHO SeelansTyres.WebApi
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the WebApi 
ECHO.
dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.OrderService

ECHO.
ECHO SeelansTyres.Services.OrderService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the Order Microservice 
ECHO.
dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.TyresService

ECHO.
ECHO SeelansTyres.Services.TyresService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the Order Microservice 
ECHO.
dotnet publish -c Release -r win-x64 --no-self-contained

CD ../..

ECHO.
ECHO Cleanup
ECHO.
RD build /S /Q
DEL efbundle.exe

ECHO Copying files to a root publish Directory
ECHO.

IF EXIST publish (
	RD publish /S /Q
	MD publish
)

XCOPY /S /Q Frontend\SeelansTyres.Mvc\publish\ publish\SeelansTyres.Mvc\
ECHO.

XCOPY /S /Q Services\SeelansTyres.WebApi\publish\ publish\SeelansTyres.WebApi\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.OrderService\publish\ publish\SeelansTyres.Services.OrderService\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.TyresService\publish\ publish\SeelansTyres.Services.TyresService\
ECHO.

PAUSE