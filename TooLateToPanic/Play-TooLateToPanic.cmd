@echo off
setlocal
set "PANIC_HOME=%~dp0Builds\Home-V4.5-20261017"
if not exist "%PANIC_HOME%\TooLateToPanic.exe" (
  echo Home V4.5 build bulunamadi.
  pause
  exit /b 1
)
start "Too Late to Panic Home V4.5" /D "%PANIC_HOME%" "%PANIC_HOME%\TooLateToPanic.exe"




