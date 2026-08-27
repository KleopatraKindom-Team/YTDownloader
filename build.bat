@echo off
echo ===================================================
echo   Building YouTube Downloader
echo ===================================================
echo.

dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not installed!
    pause
    exit /b
)

echo [1/3] Downloading yt-dlp.exe and ffmpeg.exe...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ProgressPreference = 'SilentlyContinue'; if (-not (Test-Path 'yt-dlp.exe')) { Invoke-WebRequest -Uri 'https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe' -OutFile 'yt-dlp.exe' }"

powershell -NoProfile -ExecutionPolicy Bypass -Command "$ProgressPreference = 'SilentlyContinue'; if (-not (Test-Path 'ffmpeg.exe')) { Invoke-WebRequest -Uri 'https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip' -OutFile 'ffmpeg.zip'; Expand-Archive -Path 'ffmpeg.zip' -DestinationPath 'temp_ffmpeg' -Force; Move-Item -Path 'temp_ffmpeg\*\bin\ffmpeg.exe' -Destination 'ffmpeg.exe' -Force; Remove-Item 'ffmpeg.zip' -Force; Remove-Item 'temp_ffmpeg' -Recurse -Force }"

echo [2/3] Restoring packages...
dotnet restore src/YtDownloader.csproj

echo [3/3] Compiling project...
dotnet publish src/YtDownloader.csproj -c Release -r win-x64 --self-contained false -o ./Build

if %errorlevel% equ 0 (
    copy /Y yt-dlp.exe Build\ > nul
    copy /Y ffmpeg.exe Build\ > nul
    if exist app.ico copy /Y app.ico Build\ > nul
    echo.
    echo ===================================================
    echo  SUCCESS! App built in ./Build folder.
    echo  Run: Build\YtDownloader.exe
    echo ===================================================
) else (
    echo.
    echo [ERROR] Build failed.
)

pause
