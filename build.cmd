@ECHO off

CD "%~dp0"

MORE RequiredEnvironmentVariables.txt

ECHO.
PAUSE

CD SeelansTyres.WebApi

ECHO.
ECHO Create EF Core Bundle
ECHO.
dotnet tool run dotnet-ef migrations bundle --force -o ../efbundle.exe

CD ..

ECHO.
ECHO Execute EF Core Bundle against configured database connection
ECHO.
efbundle.exe --connection "%ConnectionStrings__SeelansTyresContext%"

ECHO.
ECHO Run CMake
ECHO.
cmake -G "Visual Studio 17" -S . -B build

CD SeelansTyres.Mvc

ECHO.
ECHO SeelansTyres.Mvc
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the Frontend Project
ECHO.
dotnet publish -c Release -r win-x64 --self-contained

CD ../SeelansTyres.WebApi

ECHO.
ECHO SeelansTyres.WebApi
ECHO.

IF EXIST publish (
	RD publish /S /Q
)

ECHO Publish the WebApi 
ECHO.
dotnet publish -c Release -r win-x64 --self-contained

CD ..

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

XCOPY /S /Q SeelansTyres.Mvc\publish\ publish\SeelansTyres.Mvc\
ECHO.

XCOPY /S /Q SeelansTyres.WebApi\publish\ publish\SeelansTyres.WebApi\
ECHO. 

PAUSE