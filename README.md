# hacktv-gui-installer
Installer and launcher for hacktv-gui on Windows.

This repo contains a Windows-based installer and launcher for the hacktv-gui application. This was created to simplify the installation process on Windows, after I received some feedback that the install process was too complicated (which, in fairness, it was).

The installer is available <a href="https://github.com/steeviebops/hacktv-gui-installer/releases/latest/download/hacktv-gui_setup.exe">here</a>, or on the Releases page. It requires an active internet connection.

## Portable installation
If you don't want the installer to create Start menu shortcuts, file associations and uninstall data, select the **Do not create shortcuts (Portable installation)** option when prompted to choose a Start menu folder. You should also change the install location to a folder of your choice.

## How it works
The installer includes a small launcher executable, which runs the Java Runtime Environment (JRE) with the correct parameters. It will also create Start menu shortcuts and associate the .htv file type with the launcher.

The following components are downloaded during installation:

**Required files**
- hacktv-gui (from its GitHub repo)
- windows-kill (from its GitHub repo). This is not strictly essential but improves performance on some systems.

**Java Runtime Environment**\
Eclipse Temurin OpenJDK JRE 17 (from its GitHub repo). If you already have a JDK or JRE installed (11 or later), you can deselect this option.

**hacktv**\
fsphil's build of hacktv (from my build server, updated within two hours of a new commit). If you want to change this to another fork, you can do so on the GUI Settings tab in hacktv-gui after the installation is complete.

**FlatLaf**\
Version 3.5.4 from its Maven repo. This is an optional component which adds modern UI skins and dark mode. Also includes the IntelliJ theme pack.

**yt-dlp**\
Latest version from its GitHub repo. This is an optional component which allows for streaming of YouTube content and other online video sites.
