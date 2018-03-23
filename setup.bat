echo off
rem setup Symbolic links 
set ib=imagebuffer.exe
set ic=inputcontroller.exe
set sce=screencontentexporter.exe
set wl=windowslist.exe
set mwc=mainwebcontent.exe
set wcb=webcontentbrowser.exe
set vrce=VRCEShared.cs

set pib=%~dp0"\BabylonModules\ImageBuffer"
set pic=%~dp0"\BabylonModules\InputController"
set psce=%~dp0"\BabylonModules\ScreenContentExporter\ScreenContentExporter\ScreenContentExporter"
set pwl=%~dp0"\BabylonModules\WindowsList\WindowsList\WindowsList"

set bd=\bin\Debug

echo "I."
set p0=%~dp0"\BabylonMSMains\MainVRContentExporter\VRMainContentExporter\VRMainContentExporter"
set p1=%p0%%bd%
del %p1%\%ib%
del %p1%\%ic%
del %p1%\%sce%
del %p1%\%wl%
mklink %p1%\%ib% %pib%%bd%\%ib%
mklink %p1%\%ic% %pic%%bd%\%ic%
mklink %p1%\%sce% %psce%%bd%\%sce%
mklink %p1%\%wl% %pwl%%bd%\%wl%
set p2=p0\%vrce%

echo "II."
set p0=%~dp0"\BabylonModules\BabylonMSScreenshotClient"
set p1=%p0%%bd%
del %p1%\%sce%
mklink %p1%\%sce% %~dp0"\BabylonModules\ScreenContentExporter\ScreenContentExporter\ScreenContentExporter"%bd%\%sce%

echo "III."
set p0=%~dp0"\BabylonModules\Screenshot"
set p1=%p0%%bd%
del %p1%\%sce%
mklink %p1%\%sce% %~dp0"\BabylonModules\ScreenContentExporter\ScreenContentExporter\ScreenContentExporter"%bd%\%sce%

echo "IV."
set p0=%~dp0"\BabylonModules\WebContent"
set p1=%p0%%bd%
echo %p1%\%mwc%
del %p1%\%mwc%
del %p1%\%wcb%
mklink %p1%\%mwc% %~dp0"\BabylonMSMains\MainWebContent\MainWebContent"%bd%\%mwc%
mklink %p1%\%wcb% %~dp0"\BabylonModules\WebContentBrowser\WebContentBrowser"%bd%\%wcb%

echo "V."
set p0=%~dp0"\BabylonMSSolution\Test_ScreenContentExporter"
set p1=%p0%%bd%
del %p1%\%sce%
mklink %p1%\%sce% %~dp0"\BabylonModules\ScreenContentExporter\ScreenContentExporter\ScreenContentExporter"%bd%\%sce%

echo "VI. VRCEShared"
del %pib%\%vrce%
del %pic%\%vrce%
del %psce%\%vrce%
del %pwl%\%vrce%
mklink %pib%\%vrce% %p2% 
mklink %pic%\%vrce% %p2% 
mklink %psce%\%vrce% %p2% 
mklink %pwl%\%vrce% %p2% 
