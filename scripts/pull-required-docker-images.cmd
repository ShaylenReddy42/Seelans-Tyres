@ECHO off

docker pull elasticsearch:7.17.20
@ECHO.

docker pull kibana:7.17.20
@ECHO.

docker pull nginx:1-bullseye
@ECHO.

docker pull rabbitmq:3-management
@ECHO.

docker pull redis:latest
@ECHO.

docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
@ECHO.

docker pull mcr.microsoft.com/dotnet/sdk:6.0
@ECHO.

docker pull mcr.microsoft.com/dotnet/aspnet:8.0-alpine
@ECHO.

docker pull mcr.microsoft.com/dotnet/sdk:8.0
@ECHO.

docker pull mcr.microsoft.com/mssql/server:2022-latest
@ECHO.
