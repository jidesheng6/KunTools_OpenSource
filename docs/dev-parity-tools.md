# 工具域 / 系统域 dev 对齐审计（task-39）

基准：`dev` 分支 tip `ce4cc88`（WinForms + `DocumentTools/Services/`）。读取方式 `git show dev:<path>`，未 checkout、未改动任何 dev 文件。
对照对象：新版 `src/`（WPF + Core 服务层），功能以 `src\DocumentTools.Core\Features\BuiltInFeatures.cs` 的 feature id 为准。
版本号：新版 `src\DocumentTools.App\Infrastructure\*`、`Core\Common\AppInfo.cs`。

判定三档：

| 档位 | 含义 |
| --- | --- |
| **已迁** | 新版有等价交互，主路径行为一致（差异写在备注） |
| **缺 → 已补** | dev 有、新版没有，本卡补上并加测试 |
| **不迁** | 有意不实现，依据写在第 4 节（死代码 / 已用别的方式实现 / 依赖不可用或收益不成比例） |

本表覆盖 Lead 指定的 12 页（TabPage_10/11/12/33/34/05/09/13/36/26/37/08）与 6 项必查。
**范围声明**：这是「页面级 + 指定能力级」对照，不是 dev 每个控件的逐控件 diff；未列入的控件仍需按页继续核。

## 1. 页面对照表

| dev TabPage（Designer） | dev 页面标题 | 新版 feature id | 新版承载 | 判定 |
| --- | --- | --- | --- | --- |
| TabPage_10 | WORD 内容批量替换 | `word.replace` | `src\DocumentTools.App\Features\Modules\Convert\ConvertFeatureModule.cs` | 已迁（字体/样式四键在上一卡补齐：`DocumentReplaceService.cs:44-62`） |
| TabPage_11 | EXCEL 内容批量替换 | `excel.replace` | 同上 | 已迁（`ignoreFootnotes` 有意不加，见 feature-gap §9） |
| TabPage_12 | 特殊二维码校验 | `code.datamatrix` | `src\DocumentTools.App\Features\Modules\Tools\DataMatrixFeatureModule.cs` | **部分迁**：只支持 Word/PDF 逐页解码；dev 的「图片目录批量 + OCR 归档」缺 → 见第 5 节 |
| TabPage_33 | 简易二维码生成 | `code.barcode` | `src\DocumentTools.App\Features\Modules\Tools\BarcodeFeatureModule.cs` | 已迁（尺寸/旋转在上一卡补齐：`BarcodeRenderOptions.cs`） |
| TabPage_34 | 身份证号码校验 | `idcard.check` | `src\DocumentTools.App\Features\Modules\Tools\IdCardFeatureModule.cs` | 已迁 |
| TabPage_05 | 姓名横转竖列 | `name.vertical` | `src\DocumentTools.App\Features\Modules\Tools\NameVerticalFeatureModule.cs` | 已迁（去数字/括号/双字对齐/拼音/重名在上一卡补齐：`NameVerticalService.cs`） |
| TabPage_09 | Twain 扫描仪调试 | `twain.debug` | `src\DocumentTools.App\Features\Modules\Tools\MiscToolModules.cs:32-39` | 已迁（内核 `Core\Services\Scan\TwainScanService.cs`） |
| TabPage_13 | 其他功能 | `other.misc` | `src\DocumentTools.App\Features\Modules\Tools\MiscToolModules.cs` | 已迁 |
| TabPage_36 | 补充功能 | `other.extra` | `MiscToolModules.cs` 的 `ExtraToolsView` | 已迁 + 本卡补「路径监视」延迟（第 3 节） |
| TabPage_26 | 系统工具 | `system.tools` | `src\DocumentTools.App\Features\Modules\SystemTools\SystemFeatureModules.cs` 的 `SystemToolsView` | 已迁 + 本卡补「锁定/解锁键盘」（第 3 节） |
| TabPage_37 | 网卡设置 | `system.network` | `SystemFeatureModules.cs` 的 `NetworkView` | 已迁 + 本卡补 IPv4 校验（第 3 节，真缺陷） |
| TabPage_08 | 关于软件 | `about` | `src\DocumentTools.App\Features\Modules\About\AboutFeatureModule.cs` | 已迁 + 本卡补 UAC 开关（第 3 节） |

## 2. 六项必查

### 2.1 OCR（`Sdcb.PaddleOCR`）— **待迁**，服务「考试图片分类」

dev 证据：

| 位置 | 内容 |
| --- | --- |
| `dev:DocumentTools/Util.cs:24-27` | `using Sdcb.PaddleInference / Sdcb.PaddleOCR / .Models / .Models.Local` |
| `dev:DocumentTools/Util.cs:6181-6301` | `RecognitionPictureDataMatrix(...)`：目录批量 + DataMatrix 解码 + 可选 OCR 归档（`:6216` `UseOcr && MatchOcrString` 才走 OCR，`:6216` 之前注释「进行 ocr 识别的时候不进行图片分类」） |
| `dev:DocumentTools/Util.cs:6216-6235` | `FullOcrModel TempOcrMode = LocalFullModels.ChineseV3;` + `PaddleOcrAll(..., PaddleDevice.Mkldnn())` + `AllowRotateDetection/Enable180Classification` |
| `dev:DocumentTools/Util.cs:6245-6280` | `Cv2.ImRead` → `lock (TempPaddleEngine) engine.Run(img)` → 每个 `PaddleOcrResultRegion.Text` 做 `Regex.IsMatch(Text, MatchOcrString)`，命中则复制/移动到 `目标目录\{匹配串}\`，`MatchOcrMark` 时 `Cv2.Rectangle(..., Scalar.OrangeRed, 2)` 画框写回 |
| `dev:DocumentTools/BodyForm.cs:7307-7346` | `ExamPicture_Class_Start_Click` 调用点（传 `ExamPicture_UseOcr.Checked`、`ExamPictureOcrMatchString.Text`、`Exam_Picture_OcrMark.Checked`） |
| `dev:DocumentTools/BodyForm.cs:5751` | `ExamPicture_UseOcr.Enabled = Environment.Is64BitProcess;`（Paddle 原生仅 x64） |
| `dev:DocumentTools/BodyForm.Designer.cs:14238`、`:14324`、`:14302`、`:14288` | `groupBox67`（**TabPage_36 补充功能**内）；「图像内容分类（OCR）」；「内容标注（OCR）」；「点击下载OCR离线算法包」 |

**三问直答**

1. **它服务哪个功能**：TabPage_36「补充功能」里的 `groupBox67`「考试图片分类」（`dev:DocumentTools/BodyForm.Designer.cs:14238`），入口 `ExamPicture_Class_Start_Click`（`dev:DocumentTools/BodyForm.cs:7307-7346`）→ `Util.RecognitionPictureDataMatrix`（`dev:DocumentTools/Util.cs:6181-6301`）的 OCR 分支 `:6216-6280`。OCR 是**条件分支**：只有「使用 OCR」勾选且「匹配文字」非空才执行，同一函数的 DataMatrix 分类部分不依赖它。**不是死代码**，调用链真实可达。
2. **模型从哪加载**：**内嵌资源，不是外置目录**。`dev:DocumentTools/FodyWeavers.xml` 的 `<ExcludeAssemblies>` 只是让三个模型程序集**不参与 Costura 合并**（以松散 DLL 随程序目录发布）；dev 代码取的是 `LocalFullModels.ChineseV3`（`Util.cs:6216`），模型即该程序集的内嵌资源，仓库与代码里没有「模型目录 / SetModelDir」约定。包内实测体积：`Sdcb.PaddleOCR.Models.Local` 的 DLL **20.8 MB**、`LocalV3` 的 DLL **132 MB**（2.7.0 与 2.7.0.1 相同）、`LocalV4` 两个 DLL 合计 **245.3 MB**、`Models.Shared` 0.2 MB —— dev 同时引三组，publish 光模型就 ≈**275 MB**。
3. **迁的代价 / 不迁的损失**：见下方「依赖是否可用」「代价」「结论」。

**调用面（全树扫过，给出范围）**：`git grep -n -I -E "Paddle|LocalFullModels|RecognitionPicture|UseOcr|PaddleOcr" dev -- DocumentTools` 共 **43 处命中**，只落在 `DocumentTools.csproj`、`FodyWeavers.xml`、`Util.cs`、`BodyForm.cs`/`BodyForm.Designer.cs` 这四个文件；`RecognitionPictureDataMatrix` 全树 **1 处实现**（`dev:DocumentTools/Util.cs:6181`）+ **1 个调用点**（`dev:DocumentTools/BodyForm.cs:7318`，即 `ExamPicture_Class_Start_Click`）。所以定性是「真实可达」，**不是**「csproj 引用但零调用」。

**spike 结论（2026-11-06，回答「`paddle_inference_c.dll` 从哪来」）**：**不在仓库、不在 NuGet、也不在 dev 的构建输出里**。
- 全盘扫过 `%TEMP%`、`C:\Users\pw`、`C:\Program Files`、`D:\sharp_project`（`-Depth 6`，模式 `*paddle*.dll`、`Sdcb*.dll`、`onnxruntime.dll`、`OpenCvSharpExtern.dll`）：dev 的 `DocumentTools\bin\{Debug,Release,x64\Debug,X64_Release,x86\Debug,X86_Release}` 里只有 `Sdcb.PaddleInference.dll`（托管，<0.1 MB）+ 三个模型 DLL（20.8 / 132 / 122.6 MB），**没有任何 `paddle_inference_c.dll` / `paddle_inference.dll`**；`OpenCvSharpExtern.dll` 倒是有（来自 `OpenCvSharp4.runtime.win`）。
- native 的真正来源是**外部分发**：`dev:DocumentTools/BodyForm.cs:8658-8659` 的「点击下载OCR离线算法包」→ `Util.GetEmbedResourceObj("Link_NetPan_Download_OcrMode")` → `dev:DocumentTools/Properties/Resources.resx:223-225` 的值是 **`https://www.123684.com/s/JXY5Vv-aCSwd`**（`<comment>OCR离线算法包的分享链接</comment>`）。即：模型/native 走第三方网盘人工下载，仓库与 csproj 都不提供（链接本身未下载验证）。
- 因此按 Lead 口径结案：**legacy 的 OCR 在干净环境本就不可用**（首次 `PaddleOcrAll` 会因缺 native 失败），要恢复必须先拿到那份网盘算法包并内化，属产品决策而非技术决策。已写入 `docs\feature-gap.md` §9；本轮不进主安装包。

依赖（`dev:DocumentTools/DocumentTools.csproj`）：`Sdcb.PaddleInference 2.5.0.1`、`Sdcb.PaddleOCR 2.7.0.3`、`Models.Local 2.7.0`、`Models.LocalV3 2.7.0.1`、`Models.LocalV4 2.7.0.1`、`Models.Shared 2.7.0.1`，以及 `OpenCvSharp4 + OpenCvSharp4.Extensions + OpenCvSharp4.runtime.win`（读图 + 画框）。

**「没查到」的部分（不要当成已确认）**：native 推理运行时**找不到来源**。`Sdcb.PaddleInference 2.5.0.1` 包内只有托管包装（`lib/net45`、`net6.0`、`netstandard2.0` 下各 33.5 / 36 / 34 KB 的 `Sdcb.PaddleInference.dll`）；dev csproj 没有任何 `Sdcb.PaddleInference.runtime.*` 引用；本机 nuget 缓存里也没有 `sdcb.*runtime*` 目录；`git ls-tree -r dev` 里没有任何 paddle/onnx 二进制。两种解释都无法排除：dev 的 OCR 在干净环境首次 `PaddleOcrAll` 就会因缺 `paddle_inference_c.dll` 失败，或开发者手工把 native DLL 放进输出目录（仓库无证据）。因此**首次推理耗时、CPU 占用、native DLL 体积均未实测**。

新版现状：`src\DocumentTools.Core\Services\Code\DataMatrixService.cs` 只有 `ReadDocument(string, int dpi = 300)`（逐页渲染 + `BarCodeReader(DecodeType.DataMatrix)`），**没有目录批量、没有 OCR**；全仓 grep `OCR/Ocr` 只命中 Aspose 的 `ExtractOcrSublayerOnly`（`OfficeConvertService.cs:259/497/516`，与 Paddle 无关）。

结论：**待迁**（依据是「能力真实可达但新版没有」，不是优先级判断）。
- 不迁的损失：用户失去「把一目录考试图片按 OCR 命中文字自动归档 + 在图上画框标注」；DataMatrix 分类/改名仍然可用，但需要另建「目录批量」入口（第 5 节第 1 项）。
- 迁的代价：仅 x64（`BodyForm.cs:5751` 已把开关限制在 64 位进程）；CPU 推理（`PaddleDevice.Mkldnn()`）即可，不需要 GPU；安装包至少 +132 MB（只带 V3 模型），保留 dev 的三组则 ≈275 MB，再加 native 运行时（未实测）；模型在 DLL 内嵌，**不需要**单独分发模型文件，但也没有「外置目录」可省体积。
- 建议：做成**可选组件**——主安装包不带，缺模型 DLL 时该页禁用并提示「未安装 OCR 离线算法包」（dev 的 `Designer.cs:14288` 本来就有这个下载入口）。工作量 4–6 人日：Core `IPictureClassificationService`（枚举目录 → 解码分类 → 可选 OCR 匹配 → 归档/画框）+ App 一个页（`code.datamatrix` 的第二 Tab 或新 feature）；单测用「合成 DataMatrix 图 + 假 OCR 结果」覆盖分类与归档，OCR 推理本身不进单测。

### 2.2 `AudioService`（NAudio 系统静音/音量）— **不迁**

| 位置 | 内容 |
| --- | --- |
| `dev:DocumentTools/Services/AudioService.cs` | `SetSystemAudioVolume(bool isSetMute, float audioVolumeValue)` / `GetSystemDefaultAudioStatus()`（`MMDeviceEnumerator` → 默认 Render 端点 → `AudioEndpointVolume`） |
| `dev:DocumentTools/BodyForm.cs:8165-8176`、`:10227-10232` | 彩蛋点击前：若系统静音则 `SetSystemAudioVolume(false, 0.3f)`，再 `PlayMp3WithStream("dtez", ...)` |
| `dev:DocumentTools/BodyForm.cs:3905-3916` | `PlayCXKMusic_Click`（`PlayMp3WithStream("zdsny")`） |
| `PlayMp3WithStream` 全部调用点 | `:924`（注释）、`:3909`、`:4672`、`:5017`、`:7267`、`:8172`、`:10232` |

依据：它**不是功能页**，唯二用途是「彩蛋自动取消静音」与「播放内置 mp3 音效」。新版 `src/` 全仓 grep 无 `mp3|SoundPlayer|NAudio|提示音`；彩蛋本身未迁（也不建议为它引入 NAudio + 音频资源）。功能上零业务依赖 → 不迁。

### 2.3 键盘锁定（`KeyBoardLockStatus` + `MouseKeyHook`）— **缺 → 已补**

| dev | 内容 |
| --- | --- |
| `dev:DocumentTools/BodyForm.cs:46` | `private readonly IKeyboardEvents GlobalHook = Hook.GlobalEvents();` |
| `dev:DocumentTools/BodyForm.cs:3933-3956` | `LockKeyBoard_Click` 挂 `KeyDown/KeyUp` → `e.Handled = true`（吞全部按键）；`KeyBoardLockStatus.Text = "已锁定"` + 红，`LockKeyBoard.Enabled = false`；`UnlockKeyBoard_Click` 反注册 + `"已解锁"` + 绿 |
| `dev:DocumentTools/BodyForm.Designer.cs:11001`（父 `:10849`） | `groupBox26`「键盘工具」在 **TabPage_26 系统工具** |

业务规则：锁定后拦截**所有**按键；解锁只能靠鼠标点按钮；`Ctrl+Alt+Del`（SAS）钩不住；进程退出时系统自动撤销钩子。
新版补齐（本卡）：`src\DocumentTools.Core\Services\System\IKeyboardLockService.cs`（接口 + `IKeyboardHook` 测试缝）、`KeyboardLockService.cs`（状态机，装一次/拆一次）、`Win32KeyboardHook.cs`（`SetWindowsHookEx(WH_KEYBOARD_LL)`，回调返回 1 吞键，委托保活）；`SystemFeatureModules.cs` 的「键盘工具」卡加「锁定键盘 / 解锁键盘」+ 状态行，`Unloaded` 自动解锁（离开页面不解锁会把用户锁在键盘外，且没有可见入口）。
测试：`tests\DocumentTools.Tests\KeyboardLockServiceTests.cs`（6 例，假钩子；**测试进程内绝不安装真实全局钩子**）。

### 2.4 文件监控 + 自动生成测试文件 — **已迁（补延迟）**，造文件按钮**不迁**

| dev | 内容 |
| --- | --- |
| `dev:DocumentTools/BodyForm.cs:7697-7890` | `SelectWatcherPath_Click` / `StartPathWatcher_Click` / `TempSystemFileWatcher_Watch_JPG_TIFF_Created`（`:7725-7791`）/ `StopPathWatcher_Click` / `WatcherMakeTestFile_MouseClick` |
| `dev:DocumentTools/BodyForm.cs:7749-7770` | 命中扩展名后 `Task.Delay(WatcherTimeDelayOut.Value)` → `File.Exists(e.FullPath)` → 更新 `LatestWatcherFileNameBox` |
| `dev:DocumentTools/BodyForm.cs:7771-7790` | `WatcherSendNotify` 勾选时 Win10+ 走 `ToastContentBuilder`，否则 `MainNotify.ShowBalloonTip` |
| `dev:DocumentTools/WatcherMakeTestFileHelper.cs` | `RandomTempFileName` + 6 个空文件（`.jpg/.jpg.err/.jpg.han/.tif/.tif.err/.tif.han`），给监控功能自测用 |
| `dev:DocumentTools/BodyForm.Designer.cs:14075`（父 `:14040`）、`:14111-14128`、`:14137` | `groupBox69`「图片目录文件监控」在 **TabPage_36**；`WatcherTimeDelayOut` = Minimum 100 / 默认 500 / Maximum 6553600 ms；`WatcherSendNotify.Text = "系统通知"` |

新版已迁主体：`src\DocumentTools.App\Features\Modules\Tools\MiscToolModules.cs` 的 `ExtraToolsView`（`StartWatch` / `OnCreated` / `StopWatch`，最多保留 50 条，`Unloaded` 停止）。
本卡补的差异：
- 无延迟：原实现 `Created` 立刻入列，正在写入的文件会以残缺状态出现（与 dev 的 500ms + `File.Exists` 二次确认不等价）。现改为 Core `NewFileWatchFilter`（`src\DocumentTools.Core\Services\System\NewFileWatchFilter.cs`：目标扩展名、忽略产物、`Min/Max/Default` 延迟、`ParseDelay`）+ App 侧等待后再判 `File.Exists`，并在卡片上加「延迟(ms)」输入。
- dev 的忽略列表是**死分支**（`Path.GetExtension("a.jpg.err")` 只返回 `.err`，永远命中不了 `.jpg.err`）。新实现按「整名以忽略后缀结尾」判断，可观察行为与 dev 一致（这些产物本来也不在目标扩展名里），但意图成真。
- 「系统通知」（Toast/气泡）：**不迁**。依据：新版无托盘图标宿主（dev 用 `MainNotify` NotifyIcon），Win10+ Toast 需要 `Microsoft.Toolkit.Uwp.Notifications` 一类 UWP 通知包；本页已有事件列表与状态行，通知属于锦上添花，引入依赖不划算。
- 「生成测试文件」菜单：**不迁**。依据：`WatcherMakeTestFileHelper` 是给监控功能自测用的脚手架（6 个空文件），新版由 `NewFileWatchFilterTests` 直接覆盖过滤规则，界面上多一个按钮反而增加误触。

### 2.5 UAC（`DisableAdminBehavior`）与网卡 IP 输入

UAC：

| dev | 内容 |
| --- | --- |
| `dev:DocumentTools/BodyForm.cs:5657-5675` | `DisableAdminBehavior_Click`：非管理员提示「请使用管理员身份重新运行软件执行修改」；管理员按文案切换 `SetAdministratorBehavior(true/false)`，之后提示「重启电脑后设置生效」 |
| `dev:DocumentTools/BodyForm.cs:485-489` | 启动时按状态设文案「禁用/启用管理员批准模式」 |
| `dev:DocumentTools/Util.cs:3528-3537` | `GetAdministratorBehavior()` 读 `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System` 的 `EnableLUA` |
| `dev:DocumentTools/Util.cs:5477-5497` | `SetAdministratorBehavior(bool)` 写同一键 DWORD 1/0 |
| `dev:DocumentTools/BodyForm.Designer.cs:15887`（父 `:15663`） | `groupBox3`「开发信息」在 **TabPage_08 关于软件** |

新版：`IWindowsSystemToolsService` 原无 UAC 成员 → **缺 → 已补**：`IWindowsSystemToolsService.IsUacEnabled()/SetUacEnabled(bool)`、`WindowsSystemToolsService.cs`（复用 `RequireAdmin`/`OpenOrCreate`，读不到按默认 `1` 处理）、关于页新增「管理员批准模式（UAC）」卡（状态行 + 切换按钮 + 确认对话框 + 「重启后生效」），测试 `WindowsSystemToolsTests`（只读与非管理员拒绝，**不执行写入成功分支**）。

网卡 IP 输入：

| dev | 内容 |
| --- | --- |
| `dev:DocumentTools/IPAddressInputTextBox.cs` | 继承 `TextBox`，`CreateParams.ClassName = "SysIPAddress32"`（Win32 IP 地址控件：4 段 0–255 + 内置校验） |
| `dev:DocumentTools/BodyForm.Designer.cs`（TabPage_37） | 网卡页的 IP/掩码/网关字段用该控件 |

新版：`NetworkView` 是普通 `TextBox`，`NetworkAdapterService.ApplyStatic` 只判空（原 `:75-78`）→ **真缺陷，已修**：新增 `NetworkAddressValidator`（严格 4 段 0–255、拒绝前导 0、掩码必须连续、`0.0.0.0` 拒绝、网关/DNS 可空但填了必须合法），`ApplyStatic` 在 WMI/netsh 之前调用；测试 `NetworkAdapterValidationTests`（13 例，只走校验层，不触发 WMI/netsh）。

### 2.6 不迁项的依据汇总

见第 4 节（与 2.2 的 AudioService、2.4 的通知与造文件按钮）。

## 3. 本轮真修 / 补齐清单

| 项 | 文件 | 测试 |
| --- | --- | --- |
| 网卡静态 IP 无格式校验（dev 有 `SysIPAddress32`） | 新增 `src\DocumentTools.Core\Services\System\NetworkAddressValidator.cs`；`NetworkAdapterService.cs` 的 `ApplyStatic` 改为先校验 | `tests\DocumentTools.Tests\NetworkAdapterValidationTests.cs` |
| 路径监视缺延迟与二次确认 | 新增 `src\DocumentTools.Core\Services\System\NewFileWatchFilter.cs`；`MiscToolModules.cs` 的 `ExtraToolsView` 加「延迟(ms)」+ `File.Exists` 二次确认 | `tests\DocumentTools.Tests\NewFileWatchFilterTests.cs` |
| 缺「锁定/解锁键盘」 | 新增 `IKeyboardLockService.cs` / `KeyboardLockService.cs` / `Win32KeyboardHook.cs`；`SystemFeatureModules.cs` 键盘工具卡 + `Unloaded` 自动解锁；DI 注册 `CoreServiceCollectionExtensions.cs` | `tests\DocumentTools.Tests\KeyboardLockServiceTests.cs` |
| 缺 UAC 开关 | `IWindowsSystemToolsService.cs` / `WindowsSystemToolsService.cs`；`AboutFeatureModule.cs` 新增卡片 | `tests\DocumentTools.Tests\WindowsSystemToolsServiceTests.cs` |

## 4. 判定不迁（依据）

| 项 | 依据 |
| --- | --- |
| `AudioService`（NAudio 静音/音量） | 事实面：唯一消费者是彩蛋与内置音效（`BodyForm.cs:8165-8176`、`:10227-10232`、`:3905-3916`），新版无彩蛋、无音频资源（全仓无 mp3/SoundPlayer/NAudio）。**判断面**：它属于彩蛋/趣味音效、**无业务语义**；迁移目标文档（`docs\architecture.md`「功能不丢」）未把彩蛋列为必须项，**Lead 已确认放弃**（task-39 验收，2026-11-06）。 |
| 「系统通知」（Toast/气泡） | 事实面：新版无托盘宿主，Win10+ Toast 需要额外 UWP 通知包。**判断面**：本页已有事件列表与状态行，通知不承载信息，属装饰；Lead 已确认放弃。 |
| 「生成测试文件」菜单 | dev 自测脚手架（`WatcherMakeTestFileHelper.cs`），新版用单测覆盖过滤规则 |
| dev 的 `.jpg.err` 忽略分支 | dev 里是死代码（`GetExtension` 只返回 `.err`）；新版把意图实现出来，可观察行为一致 |

## 5. 待迁（留给后续卡）

1. **图片目录批量分类**（dev `Util.cs:6181-6301` 的 DataMatrix 部分，不含 OCR）—— **已补齐（2026-11-06）**。<br/>实现：`IDataMatrixImageClassifyService` / `DataMatrixImageClassifyService`（`src\DocumentTools.Core\Services\Code\`）+ `IDataMatrixImageClassifyService` 的单测 8 例（注入假解码器，覆盖复制/移动/子目录/异常目录/后缀过滤/失败不中断）+ 真解码冒烟 1 例；UI 在 `DataMatrixFeatureModule.BuildClassifyCard`（`:315`），含源/目标目录、包含子目录、保留源文件、线程数量（自动/1/8 的倍数，与 dev 同口径）。<br/>5 处有意偏离（异常目录死代码、子目录拼到盘符根、「保留源文件」死控件、后缀大小写、多 T 码多份）见 `feature-gap.md` §9。
2. **OCR 文字归档 + 内容标注**（`Util.cs:6216-6280`）—— **已补齐为可选组件（2026-11-06）**。<br/>依赖口径修正：`Sdcb.PaddleOCR` + `Models.Local` + native `runtime.win64.mkl` + `OpenCvSharp4` 全部可从 NuGet 还原，本机已跑通「中文图 → 识别 → 归档 + 画框」端到端（证据见 `packaging.md` §8.5）。实现：`src\DocumentTools.Ocr` 独立项目（`OcrImageClassifyService`），宿主 `OcrPluginLoader` 从 exe 同级 `ocr\` 按需加载；组件缺失时页面 OCR 选项禁用并给中文安装提示，主程序体积不变。<br/>**实测体积 537 MB**（模型 V3/V4 都必须保留，见 `packaging.md` §8.3），不是初估的 194 MB——按用户决策做成可选组件，只有需要 OCR 的机器才拷。

## 6. 验证

| 命令 | 结果 |
| --- | --- |
| `dotnet build DocumentTools.sln --no-incremental -v q --nologo` | **0 错误**；4 个警告全部来自他人文件 `tests\DocumentTools.Tests\ExamDecouplingRulesTests.cs:141-144`（xUnit2000，Exam 域），本轮新增/改动文件 0 警告 |
| `dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj` | 498 总计 / 491 通过 / 3 跳过 / **4 失败**，失败全部是 `PersonCodeLibraryTests`（他人新文件，人员代码库 zip 夹具），本轮相关 165 例全绿 |
| `dotnet run --project tools\FeatureSmoke` | 功能总数 47 / 创建成功 47 / 布局通过 47；启动落点检查 95 项，失败 0 |
| 编码自检（`Content = "???"` 类 + GBK 二次编码） | `mojibake-hits: 0`、`gbk-hits: 0` |

基线噪声：构建期间多次撞到 `testhost`/`.NET Host` 锁 dll（`MSB3026/MSB3027/MSB3021/CS2012`）与并行改文件的中间态（`ExamInsertView.cs` `CS0103 TypeModes`、`ExamMss3View.cs` `CS0103 OpenFolder`、`BarcodeFeatureModule.cs` `CS0103 PathInputs`），按 brief 等 25–45 秒重试后消失。
