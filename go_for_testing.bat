@echo off
SET /P ip=Please enter the android device IP address:
rem call VRMainContentExporter.exe e7bdb39f-c2c1-447b-b528-4b9a40757e90
cd VRMainContentExporter\bin\debug\
call VRMainContentExporter.exe -ib 127.0.0.1 -andro %ip% -id c9146853-5b63-4e72-bd03-8234f53edbbf -lc 8
rem call VRMainContentExporter.exe -ib 127.0.0.1 -andro 127.0.0.1 -id c9146853-5b63-4e72-bd03-8234f53edbbf