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

SET Database__ConnectionString=Server=localhost;Initial Catalog=SeelansTyresAddressDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%Database__ConnectionString%"

SET PROJECT=src/Services/OrderServiceSolution/SeelansTyres.Services.OrderService/SeelansTyres.Services.OrderService.csproj

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.OrderService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project "%PROJECT%" --startup-project "%PROJECT%" -o efbundle.exe

SET Database__ConnectionString=Server=localhost;Initial Catalog=SeelansTyresOrderDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%Database__ConnectionString%"

SET PROJECT=src/Services/TyresServiceSolution/SeelansTyres.Services.TyresService/SeelansTyres.Services.TyresService.csproj

ECHO.
ECHO Create EF Core Bundle for SeelansTyres.Services.TyresService
ECHO.
dotnet tool run dotnet-ef migrations bundle --force --project "%PROJECT%" --startup-project "%PROJECT%" --context TyresDbContext -o efbundle.exe

SET Database__ConnectionString=Server=localhost;Initial Catalog=SeelansTyresTyresDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true
SET RabbitMQ__ConnectionProperties__ConnectionString=amqp://localhost:5673

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%Database__ConnectionString%"

ECHO.
ECHO Run CMake
ECHO.
cmake -G "Visual Studio 17" -S . -B build