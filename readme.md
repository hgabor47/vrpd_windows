[![VRPDlogo](http://babylonms.com/babylonms/en/projects/vrpdesktop/media/vrpdicon.png "VR Prelimutens Desktop")
](http://babylonms.com/babylonms/en/projects/vrpdesktop/index.html "on WEB")
HGPLSoft VRPrelimutensDesktop for Windows
=========================================

**Part of the VRPrelimutensDesktop GITHUB project.** https://github.com/hgabor47/VRPrelimutensDesktop

With VR Prelimutens Desktop begins a new era. You can place your windows in a limitless and free space. You can use any OS’es, even more than one at the same time. Arrange different computers’ windows in a single 3D place. 
You can use same place simultaneously with others, see their avatars and what they are doing at the moment. 
Primarily, this application provides help with your work. Develop together while you are working on different files - you only need turn your head to see what the others are doing. If this is not enough we still have plans, knowledge and fantasy 
[WEBSite](http://babylonms.com/babylonms/en/projects/vrpdesktop/index.html "on WEB")

## If you would like to use only (try) download binaries and enjoy
http://babylonms.com/babylonms/en/projects/vrpdesktop/index.html


## Prerequisites

### for usage
* Android table or phone with Android 6.0 or above.
* VR Headset from low price. I use VRBOX2 and Samsung Gear VR (perfect)

### for develop
* Visual Studio 2017

## Deployment

*Please consider to diretory hierarchy (seen below) and this list will be working!

0. You need get a BABYLONMS Core for c# from here: https://github.com/hgabor47/babylonms.git

1. First you need to run VRCEShared batch for right and updated VRCESHared.cs hardlinks!
2. Need to start(!) VS2017 (Not click .sln only)
3. Open VRPrelimutensDesktop solution.
4. F7 Build all :)
5. Setup.bat for right exe files' copy.
6. Setup or create a private network. (example with Android devices USB Net Sharing) and will get IP Address.
	use this address.
7. Start:
7.a - With GUI (VRPrelimutensDesktopGUI) (recommended)		
7.b - With GO_FOR_TESTING.bat
8. RUN VRPD_Android app on your Android device https://github.com/hgabor47/vrpd_android


Hierarchy 
=========
	any dir ---BabylonMS                             https://github.com/hgabor47/babylonms.git
	        |
	        ---VRPrelimutensDesktop                  https://github.com/hgabor47/VRPrelimutensDesktop.git
	           |
	           ---vrpd_android  ()			 https://github.com/hgabor47/vrpd_android
	           |
	           ---vrpd_windows ()
[VRPD](https://github.com/hgabor47/VRPrelimutensDesktop)
[VRPDAndroid](https://github.com/hgabor47/vrpd_android)

Installer
=========
I use InnoSetup for create install package. 
http://www.jrsoftware.org


## Authors

* **Gabor Horvath** - *Initial work* - [BabylonMessageSwap](https://github.com/hgabor47/babylonms)
                                       [VRPrelimutensDesktop](https://github.com/hgabor47/VRPrelimutensDesktop)

