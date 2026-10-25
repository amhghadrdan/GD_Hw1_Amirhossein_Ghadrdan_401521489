; One-file launcher: extract to a unique per-run temporary directory,
; wait for the game, then remove its temporary files. No installation.
Unicode true
RequestExecutionLevel user
SilentInstall silent
AutoCloseWindow true
CRCCheck force
SetCompressor /SOLID lzma
SetCompressorDictSize 16
SetDatablockOptimize on
SetDateSave on

!include "LogicLib.nsh"
!include "FileFunc.nsh"
!include "x64.nsh"

Name "Dune Dash - Salt Coast"
OutFile "${PORTABLE_OUTPUT}"
Icon "${PORTABLE_ICON}"
VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=1033 "ProductName" "Dune Dash - Salt Coast"
VIAddVersionKey /LANG=1033 "FileDescription" "Dune Dash single-file portable game"
VIAddVersionKey /LANG=1033 "FileVersion" "1.0.0"
VIAddVersionKey /LANG=1033 "LegalCopyright" "See the project source notices on GitHub"

Function .onInit
  ${IfNot} ${RunningX64}
    MessageBox MB_ICONSTOP|MB_OK "Dune Dash requires 64-bit Windows."
    SetErrorLevel 1
    Abort
  ${EndIf}
  InitPluginsDir
FunctionEnd

Section "Run game"
  SetOutPath "$PLUGINSDIR\game"
  File /r "${PLAYER_DIR}/*"
  ${GetParameters} $0
  ClearErrors
  ExecWait '"$PLUGINSDIR\game\DuneDash.exe" $0' $1
  IfErrors start_failed
  ; Leave the payload directory before deleting it on Windows.
  SetOutPath "$TEMP"
  RMDir /r "$PLUGINSDIR\game"
  SetErrorLevel $1
  Goto finished

start_failed:
  MessageBox MB_ICONSTOP|MB_OK "Dune Dash could not be started. Please check that you are using 64-bit Windows and have free temporary disk space."
  SetOutPath "$TEMP"
  RMDir /r "$PLUGINSDIR\game"
  SetErrorLevel 1
finished:
SectionEnd
