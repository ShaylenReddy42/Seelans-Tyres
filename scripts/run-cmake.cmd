@ECHO off

CD "%~dp0"

CD ..

ECHO Run CMake
ECHO.

cmake -S . -B build

RD build /S /Q
