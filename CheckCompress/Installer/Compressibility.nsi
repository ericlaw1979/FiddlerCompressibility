Name "Compressibility"
OutFile "FiddlerCompressibilityAddon.exe"

Icon "addon.ico"
SetCompressor /solid lzma
RequestExecutionLevel "admin"
XPStyle on

!DEFINE /file VER_ADDON Addon.ver
!define /date NOW "%b-%d-%y"

BrandingText "[${NOW}] v${VER_ADDON}" 
VIProductVersion "${VER_ADDON}"
VIAddVersionKey "FileVersion" "${VER_ADDON}"
VIAddVersionKey "ProductName" "Fiddler Compressibility Addon"
VIAddVersionKey "Comments" "https://www.fiddler2.com/"
VIAddVersionKey "LegalCopyright" "©2016 Eric Lawrence"
VIAddVersionKey "CompanyName" "Bayden Systems"
VIAddVersionKey "FileDescription" "Installer for Fiddler Compressibility Addon"

InstallDir "$PROGRAMFILES\Fiddler2\Scripts"
InstallDirRegKey HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\Fiddler2" "LMScriptPath"
;DirShow hide

Section "Main" ; (default section)
SetOutPath "$INSTDIR"

SetOverwrite on
File "..\Bin\Release\Compressibility.dll"
SetOverwrite ifnewer

SetOutPath "$INSTDIR\..\Tools\"
File "C:\program Files (x86)\fiddler2\tools\brotli.exe"
File "C:\program Files (x86)\fiddler2\tools\zopfli.exe"
File "C:\program Files (x86)\fiddler2\tools\cwebp.exe"


WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Command" "$INSTDIR\..\Tools\cwebp.exe"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Options" "<stderr>"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\To&WebP Lossless" "Parameters" '-lossless -m 6 "{in}" -o "{out:webp}"'

WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Command" "$INSTDIR\..\Tools\cwebp.exe"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Options" "<stderr>"
WriteRegStr HKCU "Software\Microsoft\Fiddler2\ImagesMenuExt\ToWebP &Lossy" "Parameters" '-m 6 "{in}" -o "{out:webp}"'


MessageBox MB_OK "Addon Installed Successfully$\n$\nRestart Fiddler and click View > Tabs to show the 'Compressibility' tab."

SectionEnd ; end of default section