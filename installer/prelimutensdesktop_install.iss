  ; -- Example1.iss --
; Demonstrates copying 3 files and creating an icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=VR Prelimutens Desktop
AppVersion=0.1
DefaultDirName={pf}\VRPrelimutensDesktop
;DefaultDirName=c:\CRDesktop
DefaultGroupName=VRPrelimutensDesktop
UninstallDisplayIcon={app}\Unins000.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=VRPDInstall
Uninstallable=yes

[Dirs]
;Name: "{sd}\Users\Public\dmselotet\web"

[Files]
Source: "..\VRPrelimutensDesktopGUI\bin\Debug\*.exe"; DestDir: "{app}"
Source: "..\VRPrelimutensDesktopGUI\bin\Debug\*.dll"; DestDir: "{app}"
Source: ".\vrpd.ico"; DestDir: "{app}"
Source: ".\nocursor.cur"; DestDir: "{app}"

[Icons]
Name: "{group}\VRPrelimutensDesktop"; Filename: "{app}\VRPrelimutensDesktopGUI.exe";  WorkingDir: {app}; Parameters: ""; IconFilename: "{app}\vrpd.ico"
Name: "{commondesktop}\VRPrelimutensDesktop"; Filename: "{app}\VRPrelimutensDesktopGUI.exe";  WorkingDir: {app}; Parameters: ""; IconFilename: "{app}\vrpd.ico"
Name: "{group}\Uninstall"; Filename: "{app}\Unins000.exe"
