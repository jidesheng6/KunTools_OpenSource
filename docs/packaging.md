# WPF 版单文件 / 免安装发布说明

面向 `src\DocumentTools.App`（WPF 前端，`net8.0-windows`）。旧版 WinForms（`DocumentTools\`）用
Costura.Fody 把托管 DLL 合并进 exe、把 `TWAINDSM.dll` 等非托管依赖内嵌后释放到磁盘；新版改用
**.NET 8 官方单文件发布**，不再需要 Fody/Costura（`DocumentTools.App.csproj` 里的 Costura 引用已移除）。

**一句话结论**：`dotnet publish -c Release -r win-x64` 产出一个 `DocumentTools.exe`（约 153 MiB / 160.6 MB）
加 3 类必须散落的伴生文件（`TWAINDSM.dll`、`assets\embedded\PersonCode.zip`、`embedded\` 目录），
整个目录拷到任意机器即可双击运行，不需要安装 .NET 运行时。

---

## 1. 发布命令

在仓库根目录执行：

```powershell
dotnet publish src\DocumentTools.App\DocumentTools.App.csproj -c Release -r win-x64 -o artifacts\publish\win-x64
```

- 必须带 `-r win-x64`：单文件与自包含属性只在设置了 `RuntimeIdentifier` 时生效（见 csproj 中
  `<PropertyGroup Condition="'$(RuntimeIdentifier)' != ''">`）。
- 输出目录 `artifacts\` 已被 `.gitignore` 忽略，可随意重发布覆盖。
- 首次发布会从 NuGet 缓存取 `Microsoft.WindowsDesktop.App.Runtime.win-x64` / `Microsoft.NETCore.App.Runtime.win-x64`；
  本机缓存齐全时全程离线，实测 ~19 s（增量 ~5 s）。

## 2. 产物清单（实测）

`artifacts\publish\win-x64` 共 14 个发布文件 + 1 个运行期生成的日志：

| 文件 | 体积 (B) | 是否随 exe 折入单文件 | 说明 |
| --- | ---: | --- | --- |
| `DocumentTools.exe` | 160,590,199 | — | 自包含单文件，含 .NET 8 运行时 + WPF + 全部托管依赖 + Aspose 组件 + `Aspose.Hook.dll` |
| `TWAINDSM.dll` | 206,240 | 否（`ExcludeFromSingleFile`） | NTwain 只按 exe 目录/System32 探测，见 §4.1 |
| `assets\embedded\PersonCode.zip` | 2,288,286 | 否（`ExcludeFromSingleFile`） | 个人代码库压缩包，运行期按相对路径读取 |
| `embedded\kmsaio.cmd` | 295,474 | 否 | 随程序分发的素材 |
| `embedded\ReNamerPro73.exe` | 4,781,902 | 否 | 同上 |
| `embedded\VoteReportDoc.doc` | 40,375 | 否 | 同上 |
| `embedded\registry.reg` | 12,857 | 否 | 同上 |
| `embedded\renamer-rules.rnp` | 6,004 | 否 | 同上 |
| `embedded\README.md` / `link-netfx35.txt` | 1,526 / 45 | 否 | 同上 |
| `DocumentTools.pdb` / `DocumentTools.Core.pdb` / `Aspose.Hook.pdb` | 89,980 / 126,184 / 16,376 | — | 调试符号，可分发给测试但不影响运行 |
| `DocumentTools.Core.xml` | 180,293 | — | XML 文档 |
| `logs\aspose-hook.log` | 5,817 | — | **运行后生成**，不在发布清单里 |

合计 **168,641,558 B**（约 160.8 MiB，含 `logs\`；首次发布不含日志时 168,635,741 B）。
用户实际只需保证 `DocumentTools.exe` 与 `TWAINDSM.dll`、`assets\`、`embedded\` 在同一目录层级。

运行时原生库会解压到 `%TEMP%\.net\DocumentTools\<hash>\`（实测 6 个文件）：
`D3DCompiler_47_cor3.dll`、`libSkiaSharp.dll`、`PenImc_cor3.dll`、`PresentationNative_cor3.dll`、
`vcruntime140_cor3.dll`、`wpfgfx_cor3.dll`。

## 3. csproj 关键属性与理由

`src\DocumentTools.App\DocumentTools.App.csproj` 中与发布相关的部分：

- `SelfContained=true`：产物不依赖机器上安装的 .NET 8 Desktop Runtime（"免安装"的前提）。
- `PublishSingleFile=true`：托管程序集与运行时折进一个 exe，替代旧版的 Costura 合并。
- `IncludeNativeLibrariesForSelfExtract=true`：**必须开**。SkiaSharp、WPF 的原生 DLL 需要真实文件路径才能
  `LoadLibrary`，单文件宿主会把它们解压到 `%TEMP%\.net\DocumentTools\<hash>\` 再加载；关掉它这些原生库
  根本不会被解压，`libSkiaSharp.dll` 加载失败。
- `EnableCompressionInSingleFile=true`：exe 从 334.6 MB 压到 160.6 MB（实测同版本对照：无压缩
  `DocumentTools.exe` 334,582,917 B / 目录合计 342.6 MB；压缩后 160,589,009 B / 168.6 MB）。
  代价是每次冷启动要把压缩流解到内存：实测到窗口出现 1.7–2.6 s（冷启动，含首次解压原生库）。
  **不要为了启动速度关掉它**——见 §6 第 1 条的反例实测。
- `PublishTrimmed=false`：WPF + Aspose + 反射式 Hook 大量使用反射/序列化，裁剪会切掉运行期才用到的成员。
- `PublishReadyToRun=false`：R2R 与单文件压缩叠加会显著放大体积，且对启动帮助有限，暂不启用。
- `ExcludeFromSingleFile="true"`：标记在 `TWAINDSM.dll`、`PersonCode.zip`、`assets\embedded\**` 上。
  单文件打包器默认会把 `Content`/`None` 内容项**一并折进 bundle**，只有显式排除的才是散文件（证据见 §4.3）。

未改动：`TargetFramework=net8.0-windows`、`UseWPF`、`AssemblyName=DocumentTools`、
`StartupObject=DocumentTools.App.Program`、`ApplicationIcon`、`AsposeHookPath`、
`EnsurePersonCodeArchive`、`EnsureAsposeHook`。

## 4. 三个难点：实测结论

### 4.1 `TWAINDSM.dll`（非托管）→ 必须放在 exe 旁边

NTwain 3.7.5 的 `PlatformInfo` 只用 `File.Exists` 在 **exe 所在目录 / `AppContext.BaseDirectory` /
System32 / Windows 目录 / 当前工作目录** 里找 `twaindsm.dll`，**不会**去找单文件解压目录
`%TEMP%\.net\DocumentTools\<hash>\`。

探针实测（同一份 `PublishSingleFile` 探针，只改 `TWAINDSM.dll` 是散文件还是折进 bundle）：

| 形态 | `DsmExists` | `ExpectedDsmPath` | `IsSupported` | `Session.Open()` |
| --- | --- | --- | --- | --- |
| DSM 散在 exe 旁（当前采用） | `True` | `<exe目录>\twaindsm.dll` | `True` | `Success` |
| DSM 折进 bundle | `False` | `C:\WINDOWS\system32\twaindsm.dll`（不存在） | `False` | 偶然 `Success`（见下） |

- 折进 bundle 时 `twaindsm.dll` 确实被解压到了 `%TEMP%\.net\...`，但 `PlatformInfo.DsmExists` 仍为
  `False`——NTwain 的可用性判断会显示"不可用"；`Open()` 之所以还能成功，是因为单文件宿主把解压目录
  加进了 Win32 DLL 搜索路径（未文档化行为）。结论：**不要依赖它，`TWAINDSM.dll` 一律散在 exe 旁。**
- 用 `IncludeNativeLibrariesForSelfExtract=true` 解压后的 DSM 路径同样不被 NTwain 识别，因此
  `TWAINDSM.dll` 必须带 `ExcludeFromSingleFile="true"`。

**单文件下还暴露了一个生产代码缺陷（需要 Core 侧跟进，本次未改）**：
`src\DocumentTools.Core\Services\Scan\TwainScanService.cs:206` 用

```csharp
TWIdentity id = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly());
```

单文件里所有程序集的 `Location` 都是空串，NTwain 内部会对 `""` 调 `Path.GetFileNameWithoutExtension`，
抛 `ArgumentException: The path is empty. (Parameter 'path')`。实测结果：多文件产物 `Identity(assembly)=OK`、
`Session.Open()=Success`；单文件产物 `Identity(assembly)=FAILED ArgumentException`，`TwainScanService`
会把它吞成"无法加载 Twain（NTwain/DSM）：The path is empty..."，扫描功能不可用。

已验证的单文件安全写法（NTwain 的公开工厂，探针实测 `Session.Open()=Success`）：

```csharp
AssemblyName asmName = Assembly.GetExecutingAssembly().GetName();
TWIdentity id = TWIdentity.Create(
    DataGroups.Image,
    asmName.Version ?? new Version(1, 0),
    asmName.Name ?? "DocumentTools",
    asmName.Name ?? "DocumentTools",
    asmName.Name ?? "DocumentTools",
    asmName.Name ?? "DocumentTools");
```

在本机（无 TWAIN 数据源、无系统 DSM）该写法能 `Open()` 成功并枚举到 0 个数据源，符合"没有扫描仪"的预期。

### 4.2 `libs\Aspose.Hook\Aspose.Hook.dll`（`<Reference>`）→ 折进 bundle 可正常加载

- 该引用是普通程序集引用（`HintPath=$(AsposeHookPath)`），单文件发布会把它折进 bundle：
  在 exe 里能搜到 `Aspose.Hook.dll` 字符串，发布目录里没有散落的 `Aspose.Hook.dll`（只有 `.pdb`）。
- 探针里 `Assembly.Load("Aspose.Hook")` 返回 `1.0.0.0 @ ''`（`Location` 为空正是单文件的特征），
  `Aspose.Hook.HookManager` 类型解析成功。
- 真实产物启动后 `logs\aspose-hook.log` 显示 Hook 完整生效：`PreloadAsposeAssemblies OK`、
  `mHookStarted = True`、6 个 Hook 全部 `Enabled`（`Invoke`/`ParseExact`/`DateTimeOpGreaterThan`/
  `StringCompare`/`StringIndexOf`/`XmlElementInnerText`）、
  `mAssembliesLicenseSetted[0]=Aspose.Words 26.1.0.0`、`[1]=Aspose.PDF 25.12.0.0`。

### 4.3 `assets\embedded\**` 与 `assets\licenses\**` 的复制行为

- **`assets\embedded\**` 必须散文件**：`EmbeddedAssetService`、`MiscToolModules`、`VoteReportService`、
  `PersonCodeLibrary` 都按 `AppContext.BaseDirectory\embedded\...` 读文件，打包进 bundle 就读不到。
- 单文件打包器会把 `Content`/`None` 项折进 bundle——这一点踩过坑：给
  `PersonCode.zip` 只加 `CopyToPublishDirectory` 时，首次发布**发布目录里没有该文件**，
  对 exe 做字符串搜索却能看到 `assets/embedded/PersonCode.zip` 出现在 bundle 清单里。
  加上 `ExcludeFromSingleFile="true"` 后 exe 从 162,097,446 B 降到 160,589,009 B，
  `PersonCode.zip` 2,288,286 B 回到发布目录，运行期可正常读取。
- **`assets\licenses\**` 已不再复制**：本构建不内置 Aspose 授权（见 §7），csproj 里原来的
  `None ..\..\assets\licenses\**\* → licenses\` 项已删除；仓内的 `assets\licenses\README.md` 保留作为历史说明。

## 5. 验证步骤（照着跑）

```powershell
# 1) 常规三道门（Debug，快）
dotnet build DocumentTools.sln -v q --nologo
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo
dotnet run --project tools\FeatureSmoke

# 2) 发布 + 冒烟
dotnet publish src\DocumentTools.App\DocumentTools.App.csproj -c Release -r win-x64 -o artifacts\publish\win-x64
Get-ChildItem artifacts\publish\win-x64 -Recurse -File | Measure-Object Length -Sum   # 期望约 168.6 MB

# 3) 双击 / 启动验证（看到主窗口即通过；随后必须关掉进程，否则锁住 bin 影响其他人构建）
Start-Process artifacts\publish\win-x64\DocumentTools.exe
Get-Process DocumentTools | Stop-Process -Force

# 4) Hook 是否真的挂上
Get-Content artifacts\publish\win-x64\logs\aspose-hook.log -Tail 25   # 应含 mHookStarted = True

# 5) 单文件是否真的自带原生库（解压目录）
Get-ChildItem "$env:TEMP\.net\DocumentTools" -Recurse -File | Select-Object Name, Length
```

发布产物冒烟实测：冷启动（先删 `%TEMP%\.net\DocumentTools`）1.7–2.6 s 出现 WPF 主窗口，进程常驻不退出。

## 6. 已知限制与残余风险

1. **不要关闭 `EnableCompressionInSingleFile`**：对照实验（`-p:EnableCompressionInSingleFile=false`）
   产出的 334.6 MB exe 在本机连续 3 次都**卡死在 `AsposeBootstrap.Start` 的 `StartHook` 之前**
   （进程常驻、CPU 0.1 s、无窗口、日志停在"Loaded Aspose assemblies (before-StartHook)"）。
   推断：无压缩时程序集直接从 bundle 文件内存映射，Crane.MethodHook 的代码改写在该映射形态下不能完成。
   当前保留压缩即为实测可启动的形态；若将来必须比较，请在测试机上单独验证而不是直接切换。
2. **IL3000 警告（只在 publish 出现，`dotnet build` 不报）**：`AsposeBootstrap.cs:29`/`:163` 打印
   `Assembly.Location` 做诊断，单文件下恒为空。属日志噪声，不影响功能；如需消警可改为
   `AppContext.BaseDirectory` + `Assembly.GetName().Version`。
3. **TWAIN 生产代码待修**（§4.1）：单文件产物里扫描页会显示"无法加载 Twain"，需把
   `TwainScanService.cs:206` 的 `CreateFromAssembly` 换成 `TWIdentity.Create(...)`。DSM 定位本身没问题，
   修复后即可用；本机无扫描仪，只能验证到 `Open()=Success` + 0 个数据源。
4. **首次启动会写 `%TEMP%\.net\DocumentTools\<hash>\`**（约 19 MB 原生库）。若用户环境把 `%TEMP%`
   指向不可写位置，需要退回"多文件自包含 + 不压缩"或用 `DOTNET_BUNDLE_EXTRACT_BASE_DIR` 指定目录。
5. **`logs\aspose-hook.log` 写在 exe 同目录**：目录只读（如放进 `Program Files`）时 Hook 日志写入失败，
   程序仍可运行但没有日志。免安装分发建议放在用户可写目录。
6. **未做代码签名**：SmartScreen 首次运行会提示"未知发布者"。
7. **体积较大（单文件 ~153 MB）**：与 Aspose 全组件 + WPF 运行时有关；`PublishTrimmed` 不可行（理由见 §3）。

## 7. Aspose 授权口径

本构建**不内置授权**：`assets\licenses\**` 不再随产物复制，运行时也不读 `ASPOSE_LICENSE_PATH`；
`AsposeLicenseLoader.TryLoad()` 只被动探测 `AppContext.BaseDirectory\licenses\*.lic`（有就加载，没有就
评估模式）。评估限制的识别与中文解释（`OperationErrors.IsEvaluationLimit` / `EvaluationHint`）保留，
行为说明见 `docs\feature-gap.md` §9。

`Aspose.Hook` 与授权无关——它是组件版本/行为适配机制，继续随产物折进单文件（§4.2）。

## 8. OCR 可选组件（`ocr\` 目录）

OCR「考试图片分类」（`code.datamatrix` 页的「图像内容分类（OCR）」）**不随主程序打包**：它是独立的
`src\DocumentTools.Ocr` 项目，单独发布到 exe 同级的 `ocr\` 目录，宿主启动时按需加载
（`DocumentTools.App\Infrastructure\Plugins\OcrPluginLoader.cs`）。

### 8.1 发布命令

```powershell
dotnet publish src\DocumentTools.App\DocumentTools.App.csproj -c Release -r win-x64 -o artifacts\publish\win-x64
dotnet publish src\DocumentTools.Ocr\DocumentTools.Ocr.csproj -c Release -r win-x64 -o artifacts\publish\win-x64\ocr
```

发布后目录布局：

```
win-x64\
  DocumentTools.exe          ← 主程序（~160MB，未变）
  TWAINDSM.dll
  assets\  embedded\
  ocr\                       ← 可选；缺了就禁用 OCR 选项，其它功能不受影响
    DocumentTools.Ocr.dll
    paddle_inference_c.dll  mkldnn.dll  mklml.dll  libiomp5md.dll
    onnxruntime.dll  paddle2onnx.dll  onnxruntime_providers_shared.dll
    OpenCvSharp.dll  OpenCvSharpExtern.dll  libSkiaSharp.dll
    Sdcb.PaddleOCR*.dll  Sdcb.PaddleOCR.Models.Local.dll
    Sdcb.PaddleOCR.Models.LocalV3.dll  Sdcb.PaddleOCR.Models.LocalV4.dll
```

### 8.2 体积（实测 2026-11-06）

| 组成 | 体积 |
| --- | ---: |
| `paddle_inference_c.dll` + `mkldnn.dll` + `mklml.dll` + `libiomp5md.dll` | 178.6 MB |
| OpenCV（`OpenCvSharpExtern.dll` + 托管 `OpenCvSharp.dll`） | 59.8 MB |
| 模型（`Models.Local` 20.8 + `LocalV3` 132.0 + `LocalV4` 122.6） | 275.4 MB |
| 其它（onnxruntime/paddle2onnx/libSkiaSharp/Sdcb 托管） | ~23 MB |
| **`ocr\` 合计** | **537 MB** |

主程序体积不变；只有需要 OCR 的机器才拷这个目录。

### 8.3 不能裁的文件（实测证据）

- `Sdcb.PaddleOCR.Models.LocalV3/LocalV4` **不能排除**：`LocalFullModels.ChineseV3` 经
  `Models.Local.Details.Utils.LocalModel` 分派，JIT 解析 V3/V4 两个分支，缺任一即
  `FileNotFoundException: Sdcb.PaddleOCR.Models.LocalV4`。
- `mklml.dll` / `onnxruntime.dll` / `paddle2onnx.dll` **不能删**：`PaddleInferenceLibLoader` 把它们列为
  `paddle_inference_c` 的已知依赖，删 `paddle2onnx.dll` 会直接 `DllNotFoundException`（依赖状态表里
  它是 `FAIL`，随后整库加载失败）。
- 唯一保留的清理项：`opencv_videoio_ffmpeg*_64.dll`（视频解码，OCR 只处理静态图）——由 csproj 的
  `TrimOcrPublish` 目标在 publish 后删除（省 ~25MB）。

### 8.4 加载器要点

- **接口身份**：`DocumentTools.Core` 由宿主提供，`OcrLoadContext` 对该程序集返回 `null`（交回 Default），
  否则插件会加载第二份 Core，`IOcrImageClassifyService` 会变成两个不同类型。
- **原生探测**：加载前把 `ocr\` 加入 `PATH`，供 `LoadLibrary` 兜底；同时用
  `AssemblyDependencyResolver` 解析托管依赖与 `runtimes\win-x64\native`。
- **降级**：`ocr\DocumentTools.Ocr.dll` 不存在或加载失败时，`code.datamatrix` 页的 OCR 三个控件保持禁用，
  卡片下方显示中文原因（「OCR 组件未安装：把发布产物的 ocr 目录放到程序目录…」），DataMatrix 页码分类不受影响。

### 8.5 端到端验证（2026-11-06，本机）

发布上述两包后，用真实加载器跑「语文试卷第一页 / 数学试卷第二页」两张生成图：

```
LOADED=True AVAILABLE=True
TOTAL=2 MATCHED=1 UNMATCHED=1 FAILED=0
FOLDER=语文 COUNT=1
MARKED_EXISTS=True SRC_KEPT=True B_NOT_ARCHIVED=True
```

即：中文识别生效、命中图归档到 `语文\`、内容标注（红框）落盘、未命中图未被搬动、源文件按「保留」设置保留。
