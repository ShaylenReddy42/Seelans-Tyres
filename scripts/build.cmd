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

XCOPY /S /Q Frontends\SeelansTyres.Frontends.Mvc\publish\ publish\SeelansTyres.Frontends.Mvc\
ECHO.

XCOPY /S /Q Gateways\SeelansTyres.Gateways.MvcBff\publish\ publish\SeelansTyres.Gateways.MvcBff\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.AddressService\publish\ publish\SeelansTyres.Services.AddressService\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.IdentityService\publish\ publish\SeelansTyres.Services.IdentityService\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.OrderService\publish\ publish\SeelansTyres.Services.OrderService\
ECHO.

XCOPY /S /Q Services\SeelansTyres.Services.TyresService\publish\ publish\SeelansTyres.Services.TyresService\
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Frontends.Mvc.cmd publish\SeelansTyres.Frontends.Mvc\run.cmd
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Gateways.MvcBff.cmd publish\SeelansTyres.Gateways.MvcBff\run.cmd
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Services.AddressService.cmd publish\SeelansTyres.Services.AddressService\run.cmd
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Services.IdentityService.cmd publish\SeelansTyres.Services.IdentityService\run.cmd
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Services.OrderService.cmd publish\SeelansTyres.Services.OrderService\run.cmd
ECHO.

COPY /V /Y scripts\run\SeelansTyres.Services.TyresService.cmd publish\SeelansTyres.Services.TyresService\run.cmd
ECHO.

PAUSE