@ECHO off

CD "%~dp0"

CD ..

MORE RequiredEnvironmentVariables.txt

ECHO.
PAUSE

CD "%~dp0"

CALL migrate-databases.cmd

CD "%~dp0"

CALL run-cmake.cmd

CD "%~dp0"

CALL publish-all.cmd

CD "%~dp0"

ECHO.

CALL copy-publish-files.cmd

PAUSE
