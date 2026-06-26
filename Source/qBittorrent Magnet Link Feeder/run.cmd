@ECHO OFF

SET "DIRECTORY=%~f1"

IF DEFINED DIRECTORY (
    "%~dp0qBittorrent_Magnet_Link_Feeder.exe" "%DIRECTORY%"
) ELSE (
    "%~dp0qBittorrent_Magnet_Link_Feeder.exe"
)

PAUSE