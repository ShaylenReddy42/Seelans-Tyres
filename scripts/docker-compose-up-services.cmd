@ECHO off

CD "%~dp0"

ECHO Starting the services in the background
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-services.yml up -d

ECHO.
ECHO Run "docker-compose-down-services.cmd" to remove containers
ECHO.

PAUSE
