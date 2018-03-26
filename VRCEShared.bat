@echo off
echo The VRCEShared and BabylonMS files will be refreshed (mklink) from VRMainContentExporter.
echo Want to this process? (nothing problem if yes)
echo Press any key or CTLR+C
pause
rem setup Symbolic links 

set pib=%~dp0"\bms_imagebuffer"
set pic=%~dp0"\bms_inputcontroller"
set psce=%~dp0"\bms_screencontentexporter"
set pwl=%~dp0"\bms_windowslist"

echo "I."
set vrce=VRCEShared.cs
set p0=%~dp0"\VRMainContentExporter"
set p2=%p0%\%vrce%

echo "II. VRCEShared"
del %pib%\%vrce%
del %pic%\%vrce%
del %psce%\%vrce%
del %pwl%\%vrce%
mklink /H %pib%\%vrce% %p2% 
mklink /H %pic%\%vrce% %p2% 
mklink /H %psce%\%vrce% %p2% 
mklink /H %pwl%\%vrce% %p2% 

rem because VS inlink used
rem echo "III."
rem set vrce=babylonms.cs
rem set p0=%~dp0"..\..\babylonms\main\c#"
rem set p2=p0\%vrce%

rem echo "IV. Babylonms"
rem del %pib%\%vrce%
rem del %pic%\%vrce%
rem del %psce%\%vrce%
rem del %pwl%\%vrce%
rem mklink %pib%\%vrce% %p2% 
rem mklink %pic%\%vrce% %p2% 
rem mklink %psce%\%vrce% %p2% 
rem mklink %pwl%\%vrce% %p2% 
