@ECHO off

CD "%~dp0"

ECHO Starting the logging sinks in the background
ECHO.

docker compose -f ../docker-compose-loggingsinks.yml up -d

ECHO.
ECHO Run "docker-compose-down-loggingsinks.cmd" to remove containers
ECHO.

PAUSE
