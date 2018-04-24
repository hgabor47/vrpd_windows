@echo off
echo TAG this state of the app
SET /P tag=Please enter your TAG (v0.8):
SET /P message=Please enter your message's TAG:
git tag -a "%tag%" -m "%message%"
echo Finished
