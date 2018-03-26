@echo off
echo Setup runnable EXE files link for right works (mklink to working directories)
echo The VRCEShared files will be refreshed (mklink) from VRMainContentExporter.
echo Want to this process? (nothing problem if yes)
echo Press any key or CTLR+C
pause
rem setup Symbolic links 
set ib=imagebuffer.exe
set ic=inputcontroller.exe
set sce=screencontentexporter.exe
set wl=windowslist.exe

set pib=%~dp0"\bms_imagebuffer"
set pic=%~dp0"\bms_inputcontroller"
set psce=%~dp0"\bms_screencontentexporter"
set pwl=%~dp0"\bms_windowslist"

set bd=\bin\Debug

echo "I."
set p0=%~dp0"\VRMainContentExporter"
set p1=%p0%%bd%
del %p1%\%ib%
del %p1%\%ic%
del %p1%\%sce%
del %p1%\%wl%
mklink %p1%\%ib% %pib%%bd%\%ib%
mklink %p1%\%ic% %pic%%bd%\%ic%
mklink %p1%\%sce% %psce%%bd%\%sce%
mklink %p1%\%wl% %pwl%%bd%\%wl%
