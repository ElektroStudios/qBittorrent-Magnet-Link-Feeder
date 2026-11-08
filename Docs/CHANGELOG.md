# qBittorrent Magnet Link Feeder Change Log 📋

## v1.1 🔄
- Fixed: The magnet links count used to wait for the torrent dialogs did not match the way qBittorrent parses the "Download from URLs" dialog. Lines with leading/trailing white-spaces were not counted, and duplicated links that only differed by white-spaces were counted multiple times, which could make the application wait for torrent dialogs that would never appear until the 5 minutes timeout was reached. Lines are now trimmed and duplicates discarded the same way qBittorrent does.
- Fixed: When the default download path was the root of a drive (e.g. `C:\`), the resulting download path contained a doubled directory separator (e.g. `C:\\List\1`).
- Fixed: A download path containing surrounding white-spaces or line breaks (e.g. copied from the clipboard with a trailing new line) was not properly normalized.
- Fixed: A download path containing `"`, `<`, `>` or `|` characters caused a fatal "Illegal characters in path" error instead of having these characters replaced with hyphens.
- Fixed: Files whose extension only starts with "txt" (e.g. `.txt2`, `.txtold`) were also processed as source .txt files, due to the legacy behavior of the Windows file search pattern `*.txt`.
- Fixed: Typo in the console message asking to press the 'Y' key to start processing the text files.
- Improved: The application now fails with a descriptive error message if the source text file name or the download path resolves to an empty name after normalization, instead of building an invalid download path.
- Added: Unit test project (MSTest) covering path normalization, download path building, magnet links counting, natural sort order and source .txt files discovery.

## v1.0 🔄
Initial Release.