@echo off
setlocal
set "HM1_GAME=%~dp0Builds\Windows\DuneDash.exe"
if exist "%HM1_GAME%" (
  start "Dune Dash" "%HM1_GAME%"
) else (
  call "%~dp0OpenInUnity.bat"
)
