@ECHO off

CD "%~dp0"

ECHO Starting the logging sinks in the background
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-loggingsinks.yml up -d

ECHO.

CALL build.cmd
