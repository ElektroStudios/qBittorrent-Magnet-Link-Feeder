<!-- Common Project Tags:
command-line 
command-line-interface 
console-applications 
dotnet 
netframework 
netframework48 
tool 
tools 
vbnet 
visualstudio 
windows 
windows-app 
qBittorrent 
torrent-cli 
torrents 
torrenting 
torrent-automation 
magnet-uri 
magnet-link 
magnet-links 
magnetlink 
magnetlinks 
torrentlink 
torrent-link 
command-line-tool 
command-line-tools 
qbittorent 
qbittorent-cli 
torrenting-windows-desktop 
torrent-file-management 
qbittorrent-automation 
 -->

# qBittorrent Magnet Link Feeder

### Command-line utility that automates importing magnet link lists directly into the qBittorrent UI

> [!NOTE]
> Tested only with qBittorrent version 5.0.x.

------------------

## 👋 Introduction

**qBittorrent Magnet Link Feeder** is a Command-Line Interface (CLI) automation tool designed to streamline the bulk import of magnet links into qBittorrent. 

The application accepts a target directory path as a command-line argument, scanning its root for `.txt` files containing magnet links. It then automates the process of importing these links into the qBittorrent user interface and automatically handles the confirmation dialogs. To keep your workspace clean, it organizes all active downloads into structured folders named after the source text file and the torrent index, completely eliminating naming conflicts.

> 💡 **The Perfect Companion Tool**  
> This application serves as the ideal companion for [BT4G-Torrent-Magnet-Scraper](https://github.com/ElektroStudios/BT4G-Torrent-Magnet-Scraper). While the scraper automates the  harvesting of magnet links into text files, **qBittorrent Magnet Link Feeder** takes over the heavy lifting of importing and organizing those generated magnet lists directly into your qBittorrent client seamlessly.

## 👌 Features

- Automatically scans, detects, and processes `.txt` files located in the root of the specified directory path.

- Automatically structures your downloads into dedicated folders using the source `.txt` filename combined with a torrent index to prevent overwriting files.

- Employs precise `SendKeys` simulation to interact with qBittorrent dialogs without requiring manual intervention.

- Safely blocks user keyboard and mouse input during active automation sequences to guarantee flawless execution and prevent accidental misclicks.

- Designed with safety in mind; you can instantly break the input block and interrupt execution at any time by pressing `CTRL+ALT+DEL`.

## ⚠️ Limitations

> [!NOTE]
> Tested only with qBittorrent version 5.0.x.

The UI automation relies on explicit window title strings to detect and automatically dismiss duplicate torrent dialogs. Currently, **only English and Spanish** qBitTorrent user interfaces are supported. If your qBittorrent client is configured in any other language and a duplicate dialog window appear, the automation will fail to recognize the window and it will result in other issues.

To add support for other languages, the only requirement is to add the localized window title to `TORRENT_ALREADY_PRESENT_WINDOW_TITLES` collection in the source code, then compile the project with your changes using [Visual Studio](https://visualstudio.microsoft.com/downloads/).

### 🌐 Adding Multilingual Support

If you are a developer and want to add support for other languages, the process is straightforward:

1. Add the localized window title to the `TORRENT_ALREADY_PRESENT_WINDOW_TITLES` collection in the source code.
2. Recompile the project with your changes using [Visual Studio](https://visualstudio.microsoft.com/downloads/).

## 🖼️ Screenshots

![screenshot](/Images/screenshot1.png)

## 🎦 Videos

[qBittorrent-Magnet-Link-Feeder DEMO VIDEO](https://github.com/user-attachments/assets/b6c617a3-2d6a-4bfa-9306-e98e106ef38a)

## 📝 Requirements

- Microsoft Windows OS.
- [qBitTorrent](https://www.qbittorrent.org/download)

- Having the following checkbox checked in qBitTorrent: 

  ![screenshot](/Images/requisite1.png)

- Having at least one plain text file (.txt) containing magnet URIs inside (to be automatically imported to qBitTorrent).

## 🤖 Getting Started

Download the latest release by clicking [here](https://github.com/ElektroStudios/qBittorrent-Magnet-Link-Feeder/releases/latest) and start using it.

## 🔄 Change Log

Explore the complete list of changes, bug fixes, and improvements across different releases by clicking [here](/Docs/CHANGELOG.md).

## 🏆 Credits

This work relies on the following resources: 

 - [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

## ⚠️ Disclaimer:

This Work (the repository and the content provided in) is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the Work or the use or other dealings in the Work.

This Work has no affiliation, approval or endorsement by the author(s) of the third-party libraries used by this Work.

## 💪 Contributing

Your contribution is highly appreciated!. If you have any ideas, suggestions, or encounter issues, feel free to open an issue by clicking [here](https://github.com/ElektroStudios/qBittorrent-Magnet-Link-Feeder/issues/new/choose). 

Your input helps make this Work better for everyone. Thank you for your support! 🚀

## 💰 Beyond Contribution 

This work is distributed for educational purposes and without any profit motive. However, if you find value in my efforts and wish to support and motivate my ongoing work, you may consider contributing financially through the following options:

<br></br>
<p align="center"><img src="/Images/github_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Becoming my sponsor on Github:</h3>
<p align="center">You can show me your support by clicking <a href="https://github.com/sponsors/ElektroStudios/">here</a>, <br align="center">contributing any amount you prefer, and unlocking rewards!</br></p>
<br></br>

<p align="center"><img src="/Images/paypal_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Making a Paypal Donation:</h3>
<p align="center">You can donate to me any amount you like via Paypal by clicking <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY">here</a>.</p>
<br></br>

<p align="center"><img src="/Images/envato_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Purchasing software of mine at Envato's Codecanyon marketplace:</h3>
<p align="center">If you are a .NET developer, you may want to explore '<b>DevCase Class Library for .NET</b>', <br align="center">a huge set of APIs that I have on sale. Check out the product by clicking <a href="https://codecanyon.net/item/elektrokit-class-library-for-net/19260282">here</a></br><br align="center"><i>It also contains all piece of reusable code that you can find across the source code of my open source works.</i></p>
<br></br>

<h2 align="center"><u>Your support means the world to me! Thank you for considering it!</u> 👍</h2>
