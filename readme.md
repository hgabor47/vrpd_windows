

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
7.a - With GO_FOR_TESTING.bat
7.b - With GUI (VRPrelimutensDesktopGUI) (recommended)		


Hierarchy
=========
	any dir ---BabylonMS                             https://github.com/hgabor47/babylonms.git
	        |
	        ---VRPrelimutensDesktop                  https://github.com/hgabor47/VRPrelimutensDesktop.git
	           |
	           ---vrpd_android  (if you would like)
	           |
	           ---vrpd_windows (need)


Installer
=========
I use InnoSetup for create install package. 
http://www.jrsoftware.org
