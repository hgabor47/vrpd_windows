@echo off
echo Commit across all modules!
if [%1]==[] (
	SET /P message=Please enter your common commit message for windows vrpd:
) else (
	set message=%1
)

git add .
git commit -m "%message%"
git push -u origin master

set p0=".\bms_imagebuffer"
echo ----------%p0%
git -C %p0% add .
git -C %p0% commit -m "%message%"
git -C %p0% push -u origin master

set p0=".\bms_inputcontroller"
echo ----------%p0%
git -C %p0% add .
git -C %p0% commit -m "%message%"
git -C %p0% push -u origin master

set p0=".\bms_windowslist"
echo ----------%p0%
git -C %p0% add .
git -C %p0% commit -m "%message%"
git -C %p0% push -u origin master

set p0=".\bms_screencontentexporter"
echo ----------%p0%
git -C %p0% add .
git -C %p0% commit -m "%message%"
git -C %p0% push -u origin master

echo Finished
