[简体中文](README.md) | [English](README-EN.md) | **繁體中文**

# PCL-BCE（Plain Craft Launcher Bedrock Community Edition）

![Platform](https://img.shields.io/badge/平台-Windows%20x64-lightgrey)
![Release](https://img.shields.io/github/v/release/CookiesJune/PCL-Bedrock-Community-Edition)
![License](https://img.shields.io/badge/License-Apache--2.0-orange)

> 基於 [PCL-CE](https://github.com/PCL-Community/PCL-CE) 二次開發的 Minecraft 啟動器。

主打基岩版支援的 PCL-CE 改版，內建異地連線組網，開箱即用。

---

## 功能特色

- 全面支援基岩版：GDK / UWP 套件下載、解壓縮與啟動，自動辨識套件類型並選用對應啟動方式
- 基岩版版本管理：正式版 / 預覽版 / Beta 大版本摺疊分類，GDK / UWP 標籤區分，隨附預覽版專屬圖示
- 基岩版實例管理：自動辨識 bedrock_versions 資料夾，實例設定內管理行為包、資源包、世界、截圖與皮膚包
- 基岩版資源下載：CurseForge 來源，Add-On / 地圖 / 材質包分類篩選，.mcaddon / .mcpack 下載後自動安裝到目前版本
- 異地連線：Java 版使用 EasyTier，基岩版使用 GravityCone，開箱即用
- 繼承 PCL-CE 全部 Java 版能力：啟動、下載、模組與整合包管理等
- 內建離線帳戶（防強制正版帳號），首次啟動自動建立
- 個人化：墨綠主題與多款配色，支援淺色 / 深色模式
- 全螢幕模式、執行日誌、結束遊戲程序、非正常退出提示等實用功能

## 下載

前往 [Releases 頁面](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/releases) 或[官方網站](https://pcl-bce.netlify.app/)取得最新版本。

| 檔案 | 說明 |
| --- | --- |
| `PCL-BCE_x64_v1.2.2.zip` | 壓縮檔，解壓縮即可使用（約 18 MB） |
| `PCL-BCE_x64_v1.2.2.exe` | 單一檔案版（約 50 MB） |

> 防毒軟體可能誤判：本程式是未簽名的第三方社群工具，請加入信任區後再執行。下載前建議核對檔案雜湊，防止檔案被竄改。

## 更新紀錄

### v1.2.2
- 基岩版資源下載接入 CurseForge：支援 Add-On / 地圖 / 材質包分類篩選與版本選擇
- 修復資源列表混入 Java 版資源、錯誤版本號顯示與版本摺疊錯亂的問題
- .mcaddon / .mcpack 下載後自動解壓安裝到目前版本的行為包 / 資源包目錄
- 基岩版版本選擇改為基岩版版本格式，修復版本顯示為「未知」與「未選擇 BE 版本」誤報
- 修復點擊 Java 資源頁面導致啟動器崩潰的問題
- 修復遊戲啟動卡在 40% 時取消按鈕失靈的問題
- 全域資源頁面可顯示已安裝的資源（如 VDX: Java/Desktop UI）
- 移除基岩版實例設定中的「從檔案安裝」
- 最佳化帳號與皮膚資料儲存

### v1.2.0
- 修復了很多bug（包括但不限於解壓問題、部分設備運行問題、UWP下載報錯問題），然後又雙叒叕吃了一包軟糖。

### v1.1.8
- 全面完善基岩版體驗：實例設定、資源管理、啟動與日誌最佳化。
- 新增皮膚包管理，行為包 / 資源包 / 世界 / 截圖資料夾化管理。
- 啟動與退出日誌完善，支援結束遊戲程序按鈕。

### v1.1.1
- 清理內建建置路徑資訊（Sentry 中繼資料相對化）。

## 使用說明

1. 下載 zip 後解壓縮，或直接執行 exe。
2. 使用方法與 PCL-CE 大致相同，首次啟動時依照提示選擇 Minecraft 資料夾即可。

**系統需求**：Windows 10/11 x64。

## 免責聲明

- 本專案僅供個人學習交流，與 Minecraft（Mojang / Microsoft）官方無關。
- 本啟動器基於開放原始碼專案 [PCL-CE](https://github.com/PCL-Community/PCL-CE) 修改發布，遵循上游 Apache-2.0 與 Plain Craft Launcher 自訂授權，保留原版權聲明。
- 部分功能參考 [BedrockBoot](https://github.com/Round-Studio/BedrockBoot)。

## 意見回饋

遇到問題請到 [Issues](https://github.com/CookiesJune/PCL-Bedrock-Community-Edition/issues) 回饋，並附上：

- 系統版本、啟動器版本
- 重現步驟
- 日誌檔案（`PCL\Log` 資料夾中）

---

*Copyright © 2026 小teto實驗室 · 基於 Apache-2.0 協議發布*
