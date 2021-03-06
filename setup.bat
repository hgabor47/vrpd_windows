@echo off
echo Setup runnable EXE files link for right works (mklink to working directories)
echo Want to this process? (nothing problem if yes)
echo Press any key or CTLR+C
pause
rem setup Symbolic links 
set ib=imagebuffer.exe
set ic=inputcontroller.exe
set sce=screencontentexporter.exe
set wl=windowslist.exe
set ce=vrmaincontentexporter.exe
set dll=mousekeyboardactivitymonitor.dll
set cur=nocursor.cur
set ico=vrpd.ico

set pib=%~dp0"\bms_imagebuffer"
set pic=%~dp0"\bms_inputcontroller"
set psce=%~dp0"\bms_screencontentexporter"
set pwl=%~dp0"\bms_windowslist"
set pce=%~dp0"\VRMainContentExporter"
set gui=%~dp0"\VRPrelimutensDesktopGUI"

set bd=\bin\Debug

echo "I."
set p0=%pic%
set p1=%p0%%bd%
if exist %p1%\%cur% del %p1%\%cur%
mklink %p1%\%cur% %p0%\%cur% 


echo "II."
set p0=%pce%
set p1=%p0%%bd%
if exist %p1%\%ib% del %p1%\%ib%
if exist %p1%\%ic% del %p1%\%ic%
if exist %p1%\%sce% del %p1%\%sce%
if exist %p1%\%wl% del %p1%\%wl%
if exist %p1%\%dll% del %p1%\%dll%
if exist %p1%\%cur% del %p1%\%cur%
mklink %p1%\%ib% %pib%%bd%\%ib% 
mklink %p1%\%ic% %pic%%bd%\%ic% 
mklink %p1%\%sce% %psce%%bd%\%sce% 
mklink %p1%\%wl% %pwl%%bd%\%wl% 
mklink %p1%\%dll% %pic%%bd%\%dll%
mklink %p1%\%cur% %pic%\%cur% 

echo "III."
set p0=%gui%
set p1=%p0%%bd%
if exist %p1%\%ib% del %p1%\%ib%
if exist %p1%\%ic% del %p1%\%ic%
if exist %p1%\%sce% del %p1%\%sce%
if exist %p1%\%wl% del %p1%\%wl%
if exist %p1%\%ce% del %p1%\%ce%
if exist %p1%\%dll% del %p1%\%dll%
if exist %p1%\%ico% del %p1%\%ico%
if exist %p1%\%cur% del %p1%\%cur%
mklink %p1%\%ib% %pib%%bd%\%ib%
mklink %p1%\%ic% %pic%%bd%\%ic%
mklink %p1%\%sce% %psce%%bd%\%sce%
mklink %p1%\%wl% %pwl%%bd%\%wl%
mklink %p1%\%ce% %pce%%bd%\%ce%
mklink %p1%\%dll% %pic%%bd%\%dll%
mklink %p1%\%cur% %pic%\%cur%
mklink %p1%\%ico% %~dp0\%ico%

echo Finished
