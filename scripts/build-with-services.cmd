@ECHO off

CD "%~dp0"

ECHO Starting the services in the background
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-services.yml up -d

ECHO.

CALL build.cmd
