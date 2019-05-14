Name "Compressibility"
OutFile "FiddlerCompressibilityAddon.exe"
Icon "addon.ico"

RequestExecutionLevel "user"
SetCompressor /solid lzma
XPStyle on

!DEFINE /file VER_ADDON Addon.ver
!define /date NOW "%b-%d-%y"

BrandingText "[${NOW}] v${VER_ADDON}" 
VIProductVersion "${VER_ADDON}"
VIAddVersionKey "FileVersion" "${VER_ADDON}"
VIAddVersionKey "ProductName" "Fiddler Compressibility Addon"
VIAddVersionKey "Comments" "https://textslashplain.com/2016/01/27/automatically-evaluating-compressibility/"
VIAddVersionKey "LegalCopyright" "©2019 Eric Lawrence"
VIAddVersionKey "CompanyName" "Eric Lawrence"
VIAddVersionKey "FileDescription" "Installer for Fiddler Compressibility Addon"

InstallDir "$DOCUMENTS\Fiddler2\scripts\"

Section "Main"
SetOutPath "$INSTDIR"

SetOverwrite on
File "..\Bin\Release\Compressibility.dll"
SetOverwrite ifnewer

SetOutPath "$INSTDIR\Tools\"
File "C:\program Files (x86)\fiddler2\tools\brotli.exe"
File "C:\program Files (x86)\fiddler2\tools\zopfli.exe"
File "C:\program Files (x86)\fiddler2\tools\cwebp.exe"

WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Command" "$INSTDIR\..\Tools\cwebp.exe"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Options" "<stderr>"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Parameters" '-lossless -m 6 "{in}" -o "{out:webp}"'

WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Command" "$INSTDIR\..\Tools\cwebp.exe"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Options" "<stderr>"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Parameters" '-m 6 "{in}" -o "{out:webp}"'

MessageBox MB_OK "Installed Successfully$\n$\nRestart Fiddler and click View > Tabs to show the 'Compressibility' tab."

SectionEnd ; end of default section