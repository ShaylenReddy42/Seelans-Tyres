@ECHO off

CD "%~dp0"

CALL run-cmake.cmd

CD "%~dp0"

ECHO.
ECHO.
ECHO All the logs will be streamed to this command prompt.
ECHO This process could take very long the first few times because some images have to be pulled
ECHO Wait for the message "Kibana is now available (was degraded)" (it'll stop at that point) which means the site and Kibana can be accessed
ECHO Press Ctrl+C to stop
ECHO A message to terminate the console will come up, press N and Enter
ECHO.
ECHO.
ECHO URLs
ECHO -----
ECHO Health Checks UI    -- https://health-local.seelanstyres.com:4000
ECHO MVC Frontend        -- https://www-local.seelanstyres.com:4000
ECHO Identity Service    -- https://id-local.seelanstyres.com:4000
ECHO.
ECHO Kibana              -- http://localhost:5601
ECHO RabbitMQ Management -- http://localhost:15672
ECHO.
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-all.yml up

ECHO.
ECHO "docker compose down" will be run to remove all the containers
ECHO.

docker compose -f ../orchestration/docker-compose/docker-compose-all.yml down

ECHO.
ECHO The containers have been removed
ECHO.

PAUSE
