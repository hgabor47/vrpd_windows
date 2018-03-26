@echo off
echo The VRCEShared files will be refreshed (mklink) from VRMainContentExporter.
echo Want to this process? (nothing problem if yes)
echo Press any key or CTLR+C
pause
rem setup Symbolic links 
set vrce=VRCEShared.cs

set pib=%~dp0"\bms_imagebuffer"
set pic=%~dp0"\bms_inputcontroller"
set psce=%~dp0"\bms_screencontentexporter"
set pwl=%~dp0"\bms_windowslist"

echo "I."
set p0=%~dp0"\VRMainContentExporter"
set p2=p0\%vrce%

echo "VI. VRCEShared"
del %pib%\%vrce%
del %pic%\%vrce%
del %psce%\%vrce%
del %pwl%\%vrce%
mklink %pib%\%vrce% %p2% 
mklink %pic%\%vrce% %p2% 
mklink %psce%\%vrce% %p2% 
mklink %pwl%\%vrce% %p2% 
