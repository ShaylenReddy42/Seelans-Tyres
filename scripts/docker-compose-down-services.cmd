@ECHO off

CD "%~dp0"

ECHO Removing the containers
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-services.yml down

ECHO.
ECHO The containers have been removed
ECHO.

PAUSE
