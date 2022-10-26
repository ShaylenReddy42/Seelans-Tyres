@ECHO off

CD "%~dp0"

CD ..

ECHO.
ECHO Run CMake
ECHO.
cmake -G "Visual Studio 17" -S . -B build
ECHO.
ECHO.

CD scripts

ECHO All the logs will be streamed to this command prompt.
ECHO This process could that very long the first few times because some images have to be pulled
ECHO Once the containers are started up, ignore all the errors, the containers will restart
ECHO Wait for the message "Kibana is now available (was degraded)" (it'll stop at that point) which means the site and Kibana can be accessed
ECHO The site is on http://localhost:5001 and Kibana is on http://localhost:5601
ECHO Press Ctrl+C to stop
ECHO A message to terminate the console will come up, press N and Enter
ECHO Then any key to remove all the containers
ECHO.
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-all.yml up

ECHO.
ECHO Continuing will run "docker compose down" to remove all the containers
ECHO.

PAUSE

docker compose -f ../orchestration/docker-compose/docker-compose-all.yml down

ECHO.
ECHO The containers have been removed
ECHO.

PAUSE
