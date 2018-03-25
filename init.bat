@echo off 
echo Clone all necessery BabylonMS modules? 
echo If this is an original clone from vrpd_windows then you need to load content to the empty modules' directories. If the modules' directories are not empty then you will get errors. So please start once only.
echo If you would like to download contents (clone) then press a key or CTRL+C. 
pause
git clone https://gitlab.com/babylonms/bms_inputcontroller.git
git clone https://gitlab.com/babylonms/bms_imagebuffer.git
git clone https://gitlab.com/babylonms/bms_screencontentexporter.git
git clone https://gitlab.com/babylonms/bms_windowslist.git

