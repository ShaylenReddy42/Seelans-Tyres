@ECHO off

CD "%~dp0"

CD ..

CALL run-cmake.cmd

CD "%~dp0"

CD ..\..\orchestration\docker-compose

ECHO.
ECHO Running docker-compose build to build all the images
ECHO.

docker compose -f docker-compose-all.yml build

ECHO.
ECHO Next, run "02-deploy.cmd" to deploy to kubernetes
ECHO.

PAUSE
