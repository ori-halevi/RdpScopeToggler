@echo off
:: if you want your script to run minimized change the following value to true, otherwise change it to false
set run_minimized=false

if %run_minimized%==true (
    set "minimized_option=-WindowStyle Minimized"
)
:: Running a command that will return an error if not run with administrator privileges
NET SESSION >nul 2>&1
if %errorLevel% == 2 (
    :: Run the script again by PowerShell with administrator privileges
    powershell "Start-Process -FilePath \"%~f0\" -Verb RunAs %minimized_option%"
    echo The script is not running as administrator. Restarting with administrator privileges...
	exit
)
echo Running the main part of the script...
:: You add your code here



start "" "%~dp0RdpScopeToggler.sln"