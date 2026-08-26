# File Converter (Forked & Enhanced)

> **Notice / 說明**
> 
> 本專案 Fork 自 [Tichau/FileConverter](https://github.com/Tichau/FileConverter)。
> 
> 本版本進行了全方位核心架構升級：**「全 Presets 自訂執行檔路徑、通用自訂 CLI 轉換器架構、動態副檔名管理、完整繁體中文多語系支援、以及智慧自動覆蓋安裝程式」**。使用者可自由替換新版 FFmpeg / Ghostscript 或整合任意命令列轉檔工具，無需修改原始碼或重新編譯。
> 
> This project is forked from [Tichau/FileConverter](https://github.com/Tichau/FileConverter) with a **Custom Executable Path for All Presets, Generic Custom CLI Converter Architecture, Dynamic Input Formats Management, Full Traditional Chinese Localization, and Intelligent Auto-Upgrade Installer**.

---

## 🌟 新增功能與改進重點 (Key Features & Enhancements)

### 1. 所有 Presets 皆支援自訂執行檔路徑 (Custom Executable Path for All Presets)
- **自由升級底層轉檔工具**：
  - 不僅自訂（Custom）預設集，所有的**內建轉檔預設集**（如 MP4, MP3, FLAC, WAV, MKV, AVI, WEBM, GIF, PDF, PNG, JPG, WEBP 等）均支援自訂執行檔路徑！
  - 若您自行下載了新版 `ffmpeg.exe` 或新版 Ghostscript `gswin64c.exe`，可直接在 Preset 設定中指定，轉檔時自動調用新版執行檔，並完整保留內建強大的編碼參數產生、硬體加速與進度回報功能。
- **預設直接顯示內建路徑**：
  - 輸入框不再是空白，而是自動帶入並顯示軟體安裝目錄下的內建執行檔路徑（例如 `C:\Program Files\File Converter\ffmpeg.exe`），一目了然。
- **一鍵【重設】按鈕**：
  - 當指定了外部自訂路徑後，介面會即時顯示 **【重設】** 按鈕，隨時可一鍵快速還原回軟體內建預設路徑。
- **即時路徑狀態驗證**：
  - 自動檢驗檔案是否存在，即時顯示綠色 `✓ 已找到執行檔` 或提示狀態。

### 2. 通用自訂 CLI 轉換器架構 (Generic Custom CLI Converter Architecture)
- **自由整合任何命令行工具**：可直接透過圖形化介面配置任何外部 CLI 工具（如 Microsoft MarkItDown、Pandoc、自訂 Python / .NET 腳本、PowerShell、批次檔等）。
- **靈活參數樣板**：支援動態變數替換：
  - `{input}`：輸入檔案完整路徑
  - `{output}`：輸出檔案完整路徑
  - `{inputDir}` / `{outputDir}`：輸入/輸出目錄
  - `{inputFileName}` / `{outputFileName}`：不含副檔名的檔案名稱
- **自訂副檔名支援**：定義任意自訂輸出副檔名（如 `.md`、`.json`、`.txt`、`.pdf` 等）。
- **動態指令模擬預覽 (Command Preview)**：在等寬代碼框內即時模擬實際轉檔時執行的完整命令行指令，所見即所得。

### 3. 動態自訂輸入副檔名管理 (Dynamic Custom Input Formats Management)
- **免重編譯動態新增**：使用者可在 Settings 視窗的「輸入格式（Input Formats）」面板直接輸入任意副檔名（如 `.dat`、`.log`、`.ts`、`.heif` 等），即刻生效並為 Preset 勾選。
- **自訂分類與管理**：提供專屬的「自訂格式（Custom）」分類與一鍵刪除按鈕。
- **持久化儲存**：自訂副檔名自動儲存至 `Settings.user.xml`，重啟與升級時自動保留。

### 4. 完整繁體中文與多語系支援 (Full Localization & Multi-Language Support)
- **繁體中文（zh-TW）原生介面**：所有設定視窗、右鍵選單項目、提示訊息、安裝程式全面提供精準繁體中文化。
- **動態語言切換**：修正 .NET 附屬組件（Satellite Assemblies）載入機制，設定視窗切換語系即時生效。

### 5. 智慧 MSI 安裝程式 (Intelligent Auto-Upgrade Installer)
- **自動覆蓋與無縫升級**：
  - 無論電腦中目前安裝的是舊版、同版或任何 build，執行安裝程式時皆會**自動先關閉背景程式、自動移除現有版本、並乾淨安裝最新版本**，完全無需手動至控制台解除安裝。
- **自訂安裝功能選單 (Custom Setup)**：
  - 預設集擴充選項採用清晰分類勾選（音訊、視訊、圖片、文件、縮放、旋轉、光碟擷取等）。
  - 核心程式自動必選，支援自訂安裝目錄「瀏覽...」功能。

### 6. Presets 分類檔案庫與快速開啟目錄 (Categorized Presets Library)
- **純淨預設集**：全新安裝預設僅載入常用核心 Preset，右鍵選單清爽。
- **分類 Presets 資料夾**：安裝目錄內附 `Presets/` 資料夾，提供多種分類好的 XML 預設檔案（如 `Audio.xml`, `Video.xml`, `Image.xml`, `Document.xml`, `Scale.xml`, `Rotate.xml`, `Extract_Media.xml` 等）。
- **一鍵快速開啟目錄**：在 Settings 介面與右鍵選單中提供 **「預設集目錄」** 與 **「設定檔目錄」** 捷徑按鈕，方便隨時瀏覽與匯入。

---

## 💡 實用自訂 Preset 設定範例 (Custom Preset Examples)

您可以直接在 **File Converter Settings** 視窗中新建 Preset（將輸出格式設為 `Custom`），或是透過「Import（匯入）」載入以下 XML 設定檔：

### 範例 1：整合 Microsoft MarkItDown 轉為 Markdown (`.md`)
將 PDF、Word、PPT、Excel、網頁、電子書及圖片音訊 Metadata 一鍵轉為 Markdown 文件。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="To Markdown" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>pdf</InputTypes>
        <InputTypes>docx</InputTypes>
        <InputTypes>pptx</InputTypes>
        <InputTypes>xlsx</InputTypes>
        <InputTypes>csv</InputTypes>
        <InputTypes>html</InputTypes>
        <InputTypes>htm</InputTypes>
        <InputTypes>json</InputTypes>
        <InputTypes>xml</InputTypes>
        <InputTypes>eml</InputTypes>
        <InputTypes>msg</InputTypes>
        <InputTypes>epub</InputTypes>
        <InputTypes>ipynb</InputTypes>
        <InputTypes>png</InputTypes>
        <InputTypes>jpg</InputTypes>
        <InputTypes>mp3</InputTypes>
        <InputTypes>wav</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>C:\Tools\MarkItDown\markitdown.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>"{input}" -o "{output}"</CustomArgumentsTemplate>
        <CustomOutputExtension>md</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

---

### 範例 2：整合 Pandoc 進行文件格式互轉（Markdown ➔ PDF / Word）
使用 Pandoc 將 Markdown (`.md`) 轉為排版良好的 PDF 或 DOCX 文件。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="Markdown to PDF (Pandoc)" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>md</InputTypes>
        <InputTypes>markdown</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>C:\Program Files\Pandoc\pandoc.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>"{input}" -o "{output}" --pdf-engine=xelatex</CustomArgumentsTemplate>
        <CustomOutputExtension>pdf</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

---

### 範例 3：使用 PowerShell 進行自動化轉檔 (PowerShell Script & Commands)
透過 Windows 內建的 `powershell.exe`，直接執行單行指令或呼叫外部 `.ps1` 腳本完成轉檔任務。

#### (A) 單行指令：CSV 轉 JSON
```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="CSV to JSON (PowerShell)" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>csv</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>powershell.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>-NoProfile -ExecutionPolicy Bypass -Command "Import-Csv -Path '{input}' -Encoding UTF8 | ConvertTo-Json -Depth 5 | Out-File -FilePath '{output}' -Encoding UTF8"</CustomArgumentsTemplate>
        <CustomOutputExtension>json</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

#### (B) 單行指令：文字檔編碼轉為 UTF-8 (ANSI / Big5 ➔ UTF-8)
```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="Convert to UTF-8 (PowerShell)" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>txt</InputTypes>
        <InputTypes>csv</InputTypes>
        <InputTypes>log</InputTypes>
        <InputTypes>ini</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>powershell.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>-NoProfile -ExecutionPolicy Bypass -Command "Get-Content -Path '{input}' -Encoding Default | Set-Content -Path '{output}' -Encoding UTF8"</CustomArgumentsTemplate>
        <CustomOutputExtension>txt</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

#### (C) 呼叫外部 PowerShell 腳本 (`.ps1`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="Custom PowerShell Script" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>csv</InputTypes>
        <InputTypes>json</InputTypes>
        <InputTypes>xml</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>powershell.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>-NoProfile -ExecutionPolicy Bypass -File "C:\Scripts\convert.ps1" -InputPath "{input}" -OutputPath "{output}"</CustomArgumentsTemplate>
        <CustomOutputExtension>txt</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

---

### 範例 4：使用 Google cwebp 工具壓制高壓縮率 WebP 圖片
使用 Google 官方 `cwebp.exe` 對 JPG/PNG 進行高品質無損或指定品質壓縮。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="Compress to WebP (q85)" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>jpg</InputTypes>
        <InputTypes>jpeg</InputTypes>
        <InputTypes>png</InputTypes>
        <InputTypes>bmp</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>C:\Tools\libwebp\bin\cwebp.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>-q 85 "{input}" -o "{output}"</CustomArgumentsTemplate>
        <CustomOutputExtension>webp</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

---

### 範例 5：呼叫自訂 Python 腳本或 Batch 批次檔
透過執行檔路徑指向 `python.exe` 或 `cmd.exe`，直接執行您的自動化資料處理腳本。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Presets xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ConversionPreset Name="Custom Python Script" OutputType="Custom" IsDefaultSettings="false">
        <InputTypes>csv</InputTypes>
        <InputTypes>json</InputTypes>
        <InputTypes>txt</InputTypes>
        <InputPostConversionAction>None</InputPostConversionAction>
        <OutputFileNameTemplate>(p)(f)</OutputFileNameTemplate>
        <CustomExecutablePath>C:\Python311\python.exe</CustomExecutablePath>
        <CustomArgumentsTemplate>"C:\Scripts\process_data.py" --input "{input}" --output "{output}"</CustomArgumentsTemplate>
        <CustomOutputExtension>json</CustomOutputExtension>
    </ConversionPreset>
</Presets>
```

---

## 📁 如何匯入與管理 Presets (How to Import & Manage Presets)

1. 開啟 **File Converter Settings**。
2. 點擊左下方的 **`[📁 預設集目錄]`** 按鈕，檔案總管會直接開啟安裝目錄下的 `Presets/` 資料夾。
3. 點擊 Settings 視窗左下方的 **`Import`**（或在 Preset 清單點右鍵選擇「匯入預設集」）。
4. 選擇您需要的分類預設檔（如 `Scale.xml`, `Rotate.xml`, `Extract_Media.xml` 或您自訂的 XML 檔案），即可一鍵匯入！

---

## Description

**File Converter** is a very simple tool which allows you to convert and compress one or several file(s) using the context menu of windows explorer.

![File Converter Usage](Resources/FileConverterUsage.gif)

You can find more information about original File converter on the upstream [wiki](https://github.com/Tichau/FileConverter/wiki).

## 🛠️ 開發環境與建置 (Setup Development Environment)

### 系統需求 (Requirements)
* Visual Studio 2019 / 2022 (.NET Framework 4.8 / C#)
* [WiX Toolset v5](https://wixtoolset.org/) (透過 NuGet 套件管理員自動還原與安裝)
  * [FireGiant HeatWave Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=FireGiant.FireGiantHeatWaveDev17)

### 建置指令 (Command Line Build)
```powershell
MSBuild.exe FileConverter.sln /t:Restore,Build /p:Configuration=Release /p:Platform=x64
```
產出的 MSI 安裝檔路徑：`Installer\bin\x64\Release\FileConverter-setup.msi`

---

## 📦 內建組件與 Middleware (Middlewares & Components)

File Converter 整合與使用了下列優秀的開源元件：

* **FFmpeg** (v8.0.1)：視訊與音訊轉換核心 [ffmpeg.org](https://ffmpeg.org)
* **ImageMagick** (v14.10)：圖片編輯與處理核心 [imagemagick.net](http://imagemagick.net) / [GitHub](https://github.com/ImageMagick/ImageMagick)
* **Ghostscript** (10.02.1)：PDF 渲染與轉檔核心 [ghostscript.com](https://www.ghostscript.com)
* **SharpShell**：Windows 檔案總管右鍵選單擴充套件核心 [GitHub](https://github.com/dwmkerr/sharpshell)
* **Ripper & yeti.mmedia**：音樂 CD 音軌擷取核心 [CodeProject](https://www.codeproject.com/Articles/5458/C-Sharp-Ripper)
* **CommunityToolkit.Mvvm**：現代化 MVVM 架構與相依性注入 [GitHub](https://github.com/CommunityToolkit/dotnet)
* **Markdown.XAML**：WPF 內建 Markdown 文件渲染引擎 [GitHub](https://github.com/theunrepentantgeek/Markdown.XAML)
* **WpfAnimatedGif**：WPF 動態 GIF 圖片播放支援 [GitHub](https://github.com/XamlAnimatedGif/WpfAnimatedGif)

---

## 💖 致謝 (Thanks)

感謝原作者 [Adrien Allard (Tichau)](https://github.com/Tichau) 及所有開源社群貢獻者打造了優秀的 File Converter 軟體。

---

## 📄 授權 (License)

File Converter 遵循 **GPL v3 (GNU General Public License version 3)** 開源授權協定。詳情請參閱 [LICENSE.md](LICENSE.md) 或造訪 [GNU 官方網站](https://www.gnu.org/licenses/gpl.html)。
