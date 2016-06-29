@title Compressibility Builder
@filever "..\Bin\Release\Compressibility.dll" > Addon.ver
@C:\src\NSIS\MakeNSIS.EXE /V2 Compressibility.nsi
@CHOICE /M "Would you like to sign?"
@if %ERRORLEVEL%==2 goto done
@signtool sign /d "Fiddler Compressibility Addon" /du "https://www.fiddler2.com/" /n "Eric Lawrence" /t http://timestamp.digicert.com /fd SHA1 FiddlerCompressibilityAddon.exe 
@signtool sign /as /d "Fiddler Compressibility Addon" /du "https://www.fiddler2.com/" /n "Eric Lawrence" /tr http://timestamp.digicert.com /td SHA256 /fd SHA256 FiddlerCompressibilityAddon.exe 
@if %ERRORLEVEL%==-1 goto sign
@:done
@title Command Prompt