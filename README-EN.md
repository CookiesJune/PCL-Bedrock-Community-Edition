[简体中文](README.md) | **English** | [繁體中文](README-ZH_TW.md)

# PCL-BCE (Plain Craft Launcher Bedrock Community Edition)

![Platform](https://img.shields.io/badge/Platform-Windows%20x64-lightgrey)
![Release](https://img.shields.io/github/v/release/CookiesJune/PCL-Bedrock-Community-Edition)
![License](https://img.shields.io/badge/License-Apache--2.0-orange)

> A Minecraft launcher forked from [PCL-CE](https://github.com/PCL-Community/PCL-CE).

A PCL-CE fork focused on Bedrock support, with built-in remote LAN play, ready to use out of the box.

---

## Features

- Full Bedrock support: GDK / UWP package download, extraction and launch, with automatic package-type detection and the matching launch method
- Bedrock version management: stable / preview / beta grouped by major version, GDK / UWP tags, and a dedicated preview icon
- Bedrock instance management: automatic recognition of the `bedrock_versions` folder; manage behavior packs, resource packs, worlds, screenshots and skin packs from instance settings
- Bedrock resource download: CurseForge source with Add-On / maps / texture pack categories; .mcaddon / .mcpack are installed automatically into the selected version after download
- Remote LAN play: EasyTier for Java Edition, GravityCone for Bedrock, ready out of the box
- Inherits all Java Edition capabilities from PCL-CE: launching, downloading, mod and modpack management and more
- Built-in offline account (anti-forced-online), created automatically on first launch
- Personalization: ink-green theme and multiple accent colors, with light / dark mode
- Fullscreen mode, run logs, game process kill button, abnormal exit notifications and more

## Download

Get the latest version from the [Releases page](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/releases) or the [official site](https://pcl-bce.netlify.app/).

| File | Description |
| --- | --- |
| `PCL-BCE_x64_v1.2.2.zip` | Archive, extract and run (approx. 18 MB) |
| `PCL-BCE_x64_v1.2.2.exe` | Standalone executable (approx. 50 MB) |

> Antivirus software may flag this program: it is an unsigned third-party community tool. Please add it to the trusted zone before running. Verify the file hash before downloading to prevent tampering.

## Changelog

### v1.2.2
- Bedrock resource downloads now use CurseForge: Add-On / maps / texture pack categories and version selection
- Fixed resource lists mixing in Java Edition content, wrong version numbers and broken version accordion grouping
- .mcaddon / .mcpack are now extracted and installed automatically into the selected version's behavior pack / resource pack folders
- Bedrock version selection now uses Bedrock version format; fixed versions showing as "Unknown" and false "no BE version selected" prompts
- Fixed a crash when opening the Java Edition resource page
- Fixed the cancel button being unresponsive when the game launch is stuck at 40%
- Installed resources (e.g. VDX: Java/Desktop UI) now appear on the global resource page
- Removed "Install from file" from Bedrock instance settings
- Improved account and skin data persistence

### v1.2.0
- A lot of bugs were fixed (including but not limited to decompression problems, some equipment operation problems, and UWP download error reporting problems), and then once again ate a bag of soft candy.

### v1.1.8
- Greatly improved the Bedrock experience: instance settings, resource management, launch and log optimization.
- Added skin pack management; behavior packs / resource packs / worlds / screenshots managed in dedicated folders.
- Improved launch and exit logs; added a process-kill button for the game.

### v1.1.1
- Clean up embedded build path info (Sentry metadata relativized).

## Usage

1. Extract the zip, or run the exe directly.
2. Usage is mostly the same as PCL-CE; on first launch, follow the prompts to select your Minecraft folder.

**System requirements**: Windows 10/11 x64.

## Disclaimer

- This project is for personal learning and communication only, and is not affiliated with the official Minecraft (Mojang / Microsoft).
- This launcher is modified and distributed based on the open-source project [PCL-CE](https://github.com/PCL-Community/PCL-CE), following the upstream Apache-2.0 and Plain Craft Launcher custom license, with the original copyright notice retained.
- Some features reference [BedrockBoot](https://github.com/Round-Studio/BedrockBoot).

## Feedback

Report issues on the [Issues page](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/issues), and include:

- OS and launcher version
- Steps to reproduce
- Log files (under `PCL\Log`)

---

*Copyright © 2026 小teto实验室 · Released under the Apache-2.0 License*
