@ECHO off

CD "%~dp0"

CALL prebuild.cmd

CD "%~dp0"

CALL publish-all.cmd

CD "%~dp0"

CD ..

ECHO.
ECHO Cleanup
ECHO.
RD build /S /Q
DEL efbundle.exe

ECHO Copying files to a root publish Directory
ECHO.

CD "%~dp0"

CALL copy-publish-files.cmd

PAUSE
