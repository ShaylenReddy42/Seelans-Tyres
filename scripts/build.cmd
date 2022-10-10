@ECHO off

CD "%~dp0"

CALL prebuild.cmd

CD Frontends/SeelansTyres.Frontends.Mvc

ECHO.
ECHO SeelansTyres.Frontends.Mvc
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Gateways/SeelansTyres.Gateways.MvcBff

ECHO.
ECHO SeelansTyres.Gateways.MvcBff
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.AddressService

ECHO.
ECHO SeelansTyres.Services.AddressService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.IdentityService

ECHO.
ECHO SeelansTyres.Services.IdentityService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.OrderService

ECHO.
ECHO SeelansTyres.Services.OrderService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

dotnet publish -c Release -r win-x64 --no-self-contained

CD ../../Services/SeelansTyres.Services.TyresService

ECHO.
ECHO SeelansTyres.Services.TyresService
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

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

CD "%~dp0"

CALL copy-publish-files.cmd

PAUSE
