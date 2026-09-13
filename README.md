**简体中文** | [English](README-EN.md) | [繁體中文](README-ZH_TW.md)

# PCL-BCE（Plain Craft Launcher Bedrock Community Edition）

![Platform](https://img.shields.io/badge/平台-Windows%20x64-lightgrey)
![Release](https://img.shields.io/github/v/release/CookiesJune/PCL-Bedrock-Community-Edition)
![License](https://img.shields.io/badge/License-Apache--2.0-orange)

> 基于 [PCL-CE](https://github.com/PCL-Community/PCL-CE) 二次开发的 Minecraft 启动器。

主打基岩版支持的 PCL-CE 改版，内置异地联机组网，开箱即用。

---

## 功能特性

- 全面支持基岩版：GDK / UWP 包下载、解压与启动，自动识别包类型并选用对应启动方式
- 基岩版版本管理：正式版 / 预览版 / Beta 大版本折叠分类，GDK / UWP 标签区分，随附预览版专属图标
- 基岩版实例管理：自动识别 bedrock_versions 目录，实例设置内管理行为包、资源包、世界、截图与皮肤包
- 基岩版资源下载：CurseForge 源，Add-On / 地图 / 材质包分类筛选，.mcaddon / .mcpack 下载后自动安装到当前版本
- 异地联机：Java 版使用 EasyTier，基岩版使用 GravityCone，开箱即用
- 继承 PCL-CE 全部 Java 版能力：启动、下载、模组与整合包管理等
- 内置离线账户（防强制正版账号），首次启动自动创建
- 个性化：墨绿主题与多款配色，支持浅色 / 深色模式
- 全屏模式、运行日志、结束游戏进程、非正常退出提示等实用功能

## 下载

前往 [Releases 页面](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/releases) 或[官网](https://pcl-bce.netlify.app/)获取最新版本。

| 文件 | 说明 |
| --- | --- |
| `PCL-BCE_x64_v1.2.2.zip` | 压缩包，解压即用（约 18 MB） |
| `PCL-BCE_x64_v1.2.2.exe` | 单文件版（约 50 MB） |

> 杀毒软件可能误报：本程序是未签名的第三方社区工具，请加入信任区后再运行。下载前建议核对文件哈希，防止文件被篡改。

## 更新日志

### v1.2.2
- 基岩版资源下载接入 CurseForge：支持 Add-On / 地图 / 材质包分类筛选与版本选择
- 修复资源列表混入 Java 版资源、错误版本号显示与版本折叠错乱的问题
- .mcaddon / .mcpack 下载后自动解压安装到当前版本的行为包 / 资源包目录
- 基岩版版本选择改为基岩版版本格式，修复版本显示为“未知”与“未选择 BE 版本”误报
- 修复点击 Java 资源页面导致启动器崩溃的问题
- 修复游戏启动卡在 40% 时取消按钮失灵的问题
- 全局资源页面可显示已安装的资源（如 VDX: Java/Desktop UI）
- 移除基岩版实例设置中的“从文件安装”
- 优化账号与皮肤数据保存

### v1.2.0
- 修复了很多bug（包括但不限于解压问题、部分设备运行问题、UWP下载报错问题），然后又吃了一包软糖。

### v1.1.8
- 全面完善基岩版体验：实例设置、资源管理、启动与日志优化。
- 新增皮肤包管理，行为包 / 资源包 / 世界 / 截图目录化管理。
- 启动与退出日志完善，支持结束游戏进程按钮。

### v1.1.1
- 清理内置构建路径信息（Sentry 元数据相对化）。

## 使用说明

1. 下载 zip 后解压，或直接运行 exe。
2. 使用方法与 PCL-CE 基本一致，首次启动时按提示选择 Minecraft 文件夹即可。

**系统要求**：Windows 10/11 x64。

## 免责声明

- 本项目仅供个人学习交流，与 Minecraft（Mojang / Microsoft）官方无关。
- 本启动器基于开源项目 [PCL-CE](https://github.com/PCL-Community/PCL-CE) 修改分发，遵循上游 Apache-2.0 与 Plain Craft Launcher 自定义许可，保留原版权声明。
- 部分功能参考 [BedrockBoot](https://github.com/Round-Studio/BedrockBoot)。

## 反馈

遇到问题请到 [Issues](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/issues) 反馈，并附上：

- 系统版本、启动器版本
- 复现步骤
- 日志文件（`PCL\Log` 目录下）

---

*Copyright © 2026 小teto实验室 · 基于 Apache-2.0 协议发布*
