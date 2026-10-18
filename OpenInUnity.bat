@echo off
setlocal
set "HM1_ROOT=%~dp0"
set "HM1_UNITY=C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe"
if defined UNITY_EDITOR set "HM1_UNITY=%UNITY_EDITOR%"
if not exist "%HM1_UNITY%" (
  echo Install Unity 2022.3.62f1 in Unity Hub, then Add this HM1 folder.
  echo Or set UNITY_EDITOR to the full path of Unity.exe.
  pause
  exit /b 1
)
start "Dune Dash" "%HM1_UNITY%" -projectPath "%HM1_ROOT%"
