ECHO @ECHO OFF > "bin\%~1.bat"
ECHO "%%~dp0tools\%~1.exe" %%* >> "bin\%~1.bat"
