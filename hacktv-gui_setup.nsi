Unicode false # Required for nsUnzip plugin to work
!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "WinVer.nsh"
!include "Integration.nsh"

Name "hacktv-gui"
InstallDir "$APPDATA\hacktv-gui"
OutFile "hacktv-gui_setup.exe"
RequestExecutionLevel user

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "license.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
var StartMenuFolder
!define MUI_INNERTEXT_STARTMENU_CHECKBOX "Do not create shortcuts (Portable installation)"
!insertmacro MUI_PAGE_STARTMENU $(^Name) $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES
#!define MUI_FINISHPAGE_NOAUTOCLOSE
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Version Information

  VIProductVersion  "1.0.0.0"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "hacktv-gui"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "GUI wrapper for hacktv"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright (C) 2025 Stephen McGarry (https://github.com/steeviebops)"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "hacktv-gui installer"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "1.0.0.0"

;--------------------------------

Section "!Required files" MAIN
    SectionIn RO
    SetOutPath $INSTDIR
    File "launcher.exe"

    CreateDirectory "$INSTDIR\bin"
    SetOutPath "$INSTDIR\bin"

    # hacktv-gui.jar
    DetailPrint "Downloading https://github.com/steeviebops/hacktv-gui/releases/latest/download/hacktv-gui.jar..."
    inetc::get /caption "hacktv-gui" "https://github.com/steeviebops/hacktv-gui/releases/latest/download/hacktv-gui.jar" "$INSTDIR\bin\hacktv-gui.jar" /end
    Pop $0 # return value = exit code, "OK" means OK

    # windows-kill
    DetailPrint "Downloading https://github.com/ElyDotDev/windows-kill/releases/download/1.1.4/windows-kill_1.1.4_release.zip..."
    inetc::get /caption "windows-kill application" "https://github.com/ElyDotDev/windows-kill/releases/download/1.1.4/windows-kill_1.1.4_release.zip" "$TEMP\windows-kill_1.1.4_release.zip" /end
    Pop $0
    DetailPrint "Extract: windows-kill.exe"
    nsUnzip::Extract /j "$TEMP\windows-kill_1.1.4_release.zip" "windows-kill_x64_1.1.4_lib_release\windows-kill.exe" /end
    Pop $1
    Delete "$TEMP\windows-kill_1.1.4_release.zip"

    # hacktv (fsphil)
    DetailPrint "Downloading https://download.bops.ie/hacktv/fsphil.zip..."
    inetc::get /caption "hacktv" "https://download.bops.ie/hacktv/fsphil.zip" "$TEMP\fsphil.zip"
    Pop $2
    DetailPrint "Extract: hacktv.exe"
    nsUnzip::Extract "$TEMP\fsphil.zip" /end
    Delete "$TEMP\fsphil.zip"
    Delete "readme.txt"

    # Create Start menu shortcuts if enabled
    !insertmacro MUI_STARTMENU_WRITE_BEGIN $(^Name)
        CreateDirectory "$SMPrograms\$StartMenuFolder"
        CreateShortcut /NoWorkingDir "$SMPrograms\$StartMenuFolder\$(^Name).lnk" "$InstDir\launcher.exe"
        CreateShortcut /NoWorkingDir "$SMPrograms\$StartMenuFolder\$(^Name) (Console mode).lnk" "$InstDir\launcher.exe" "/console"
        ${If} $(^Name) != $StartMenuFolder
            # Write the name of the selected Start Menu folder to the registry so we can remove it during uninstall
            WriteRegStr HKCU "Software\$(^Name)\Setup" "CustomStartDir" $StartMenuFolder
        ${EndIf}
        # Uninstaller data
        WriteUninstaller "$INSTDIR\uninstall.exe"
        !define UNINSTALL_PATH "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
        WriteRegStr HKCU "${UNINSTALL_PATH}" "DisplayName" $(^Name)
        WriteRegStr HKCU "${UNINSTALL_PATH}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
        WriteRegStr HKCU "${UNINSTALL_PATH}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
        WriteRegStr HKCU "${UNINSTALL_PATH}" "DisplayIcon" "$\"$INSTDIR\launcher.exe$\""
        WriteRegStr HKCU "${UNINSTALL_PATH}" "Publisher" "Stephen McGarry"
        WriteRegStr HKCU "${UNINSTALL_PATH}" "UrlUpdateInfo" "https://github.com/steeviebops/hacktv-gui"
        # File associations
        !define ASSOC_EXT ".htv"
        !define ASSOC_PROGID "hacktv-gui"
        !define ASSOC_VERB "open"
        !define ASSOC_APPEXE "launcher.exe"
        !define ASSOC_DESC "hacktv-gui configuration file"
        # Register file type
        WriteRegStr HKCU "Software\Classes\${ASSOC_PROGID}\DefaultIcon" "" "$InstDir\${ASSOC_APPEXE},0"
        WriteRegStr HKCU "Software\Classes\${ASSOC_PROGID}" "" "${ASSOC_DESC}"
        WriteRegStr HKCU "Software\Classes\${ASSOC_PROGID}\shell\${ASSOC_VERB}\command" "" '"$InstDir\${ASSOC_APPEXE}" "%1"'
        WriteRegStr HKCU "Software\Classes\${ASSOC_EXT}" "" "${ASSOC_PROGID}"

        ${NotifyShell_AssocChanged}

    !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

Section "Java Runtime Environment" JRE
    SetOutPath $INSTDIR
    ${If} ${FileExists} `$INSTDIR\jre\*.*`
        RMDir /r $INSTDIR\jre
    ${EndIf}
    DetailPrint "Downloading https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.26+4/OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip..."
    inetc::get /caption "Eclipse Temurin OpenJDK JRE 11.0.26+4" "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.26+4/OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip" "$TEMP\OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip" /end
    Pop $3
    DetailPrint "Extracting: OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip..."
    nsUnzip::Extract "$TEMP\OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip" /end
    Pop $4
    Rename "jdk-11.0.26+4-jre" "jre"
    Delete "$TEMP\OpenJDK11U-jre_x64_windows_hotspot_11.0.26_4.zip"
SectionEnd

Section "FlatLaf" FLATLAF
    SetOutPath "$INSTDIR\bin"
    DetailPrint "Downloading https://repo1.maven.org/maven2/com/formdev/flatlaf/3.0/flatlaf-3.0.jar..."
    inetc::get /caption "FlatLaf" "https://repo1.maven.org/maven2/com/formdev/flatlaf/3.0/flatlaf-3.0.jar" "$INSTDIR\bin\flatlaf-3.0.jar" /end
    Pop $5
SectionEnd

Section /o "yt-dlp" YT_DLP
    SetOutPath "$INSTDIR\bin"
    DetailPrint "Downloading https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
    inetc::get /caption "yt-dlp" "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe" "$INSTDIR\bin\yt-dlp.exe" /end
    Pop $6
SectionEnd

Section ""
    # Calculate on-disk size, from https://nsis-dev.github.io/NSIS-Forums/html/t-267188.html
    ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
    IntFmt $0 "0x%08X" $0
    WriteRegDWORD HKCU "${UNINSTALL_PATH}" "EstimatedSize" "$0"
SectionEnd

Section Uninstall
    MessageBox MB_YESNO|MB_ICONQUESTION|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" /SD IDYES IDYES true IDNO false
    true:
        # Remove launcher
        ${If} ${FileExists} `$INSTDIR\launcher.exe`
            Delete "$INSTDIR\launcher.exe"
        ${EndIf}
        # Remove bin directory
        ${If} ${FileExists} `$INSTDIR\bin\*.*`
            RMDir /r $INSTDIR\bin
        ${EndIf}
        # Remove JRE
        ${If} ${FileExists} `$INSTDIR\jre\*.*`
            RMDir /r $INSTDIR\jre
        ${EndIf}
        # Remove Start menu shortcuts
        var /global startDir
        ReadRegStr $0 HKCU "Software\$(^Name)\Setup" "CustomStartDir"
        ${If} $0 == ""
            StrCpy $startDir $(^Name)
        ${Else}
            StrCpy $startDir $0
        ${EndIf}
        ${If} ${FileExists} `$SMPrograms\$startDir\*.*`
            RMDir /r "$SMPrograms\$startDir"
        ${EndIf}
        # Remove reg keys
        DeleteRegKey HKCU "Software\$(^Name)"
        DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
        # Unregister file type
        DeleteRegKey HKCU "Software\Classes\.htv"
        DeleteRegKey HKCU "Software\Classes\$(^Name)"
        ${NotifyShell_AssocChanged}
        # Remove uninstaller
        ${If} ${FileExists} `$INSTDIR\uninstall.exe`
            Delete "$INSTDIR\Uninstall.exe"
        ${EndIf}
        # Remove install dir
        ${If} ${FileExists} `$INSTDIR`
            RMDir $INSTDIR
        ${EndIf}
        Goto next
    false:
        Quit
    next:
SectionEnd

Function .onInit
    # Windows version check
    ${WinVerGetBuild} $R2
    ${IfNot} ${AtLeastWin10}
        ${AndIf} $R2 < 17763
            MessageBox MB_OK|MB_ICONSTOP "This application requires Windows 10 1809 or later."
            Quit
    ${EndIf}

    # Set estimated disk space requirements for each section
    SectionSetSize ${MAIN} 25600
    SectionSetSize ${JRE} 122880
    SectionSetSize ${FLATLAF} 765
    SectionSetSize ${YT_DLP} 20480
FunctionEnd

# Set section descriptions
LangString DESC_MAIN ${LANG_ENGLISH} "Installs hacktv-gui and supporting files. Also installs fsphil's build of hacktv; this can be changed later."
LangString DESC_JRE ${LANG_ENGLISH} "Installs a dedicated copy of the Java Runtime Environment (Eclipse Temurin OpenJDK 11). If you already have a version 11 or later JRE or JDK installed, you can deselect this option."
LangString DESC_FLATLAF ${LANG_ENGLISH} "Installs the FlatLaf library, for dark mode and modern UI skins."
LangString DESC_YT_DLP ${LANG_ENGLISH} "Installs yt-dlp, a YouTube downloader. Used for streaming videos from online video sites."
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${MAIN} $(DESC_MAIN)
    !insertmacro MUI_DESCRIPTION_TEXT ${JRE} $(DESC_JRE)
    !insertmacro MUI_DESCRIPTION_TEXT ${FLATLAF} $(DESC_FLATLAF)
    !insertmacro MUI_DESCRIPTION_TEXT ${YT_DLP} $(DESC_YT_DLP)
!insertmacro MUI_FUNCTION_DESCRIPTION_END