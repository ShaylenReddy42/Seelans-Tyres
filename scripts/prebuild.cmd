@ECHO off

CD ..

MORE RequiredEnvironmentVariables.txt

ECHO.
PAUSE

ECHO.
ECHO Restore dotnet tools
ECHO.
dotnet tool restore

SET PROJECT=src/Services/AddressServiceSolution/SeelansTyres.Services.AddressService/SeelansTyres.Services.AddressService.csproj

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.AddressService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project "%PROJECT%" --startup-project "%PROJECT%" -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresAddressContext%"

SET PROJECT=src/Services/OrderServiceSolution/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.OrderService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project "%PROJECT%" --startup-project "%PROJECT%" -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresOrderContext%"

SET PROJECT=src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj
SET RabbitMQ__ConnectionProperties__ConnectionString=amqp://localhost:5673

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.TyresService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project "%PROJECT%" --startup-project "%PROJECT%" -o efbundle.exe

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%SeelansTyresTyresContext%"

ECHO.
ECHO Run CMake
ECHO.
cmake -G "Visual Studio 17" -S . -B build