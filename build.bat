@echo off
cls

echo ==========================
echo YT Downloader Build
echo ==========================


if not exist app.ico (

echo ERROR: app.ico not found!
pause
exit /b

)



if not exist buildproj (

dotnet new winforms -n buildproj

)



copy Program.cs buildproj\Program.cs
copy app.ico buildproj\app.ico



cd buildproj



dotnet add package YoutubeExplode



powershell -Command "(Get-Content buildproj.csproj) -replace '</PropertyGroup>', '<ApplicationIcon>app.ico</ApplicationIcon></PropertyGroup>' | Set-Content buildproj.csproj"



echo Building...



dotnet publish ^
-c Release ^
-r win-x64 ^
--self-contained true ^
-p:PublishSingleFile=true ^
-o ..\Build



cd ..



copy Build\buildproj.exe YTDownloader.exe



echo.
echo ==========================
echo DONE!
echo Icon added!
echo ==========================


pause