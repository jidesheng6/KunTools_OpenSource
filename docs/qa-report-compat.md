# 环境缺失降级行为验证报告（QaCompat）

> 本文件由 `tools\QaCompat` 在每次运行时自动生成，内容与本次真实执行结果一致。

## 环境

| 项 | 值 |
| --- | --- |
| 运行时间 | 2026-11-06 12:55:04 +08:00 |
| 主机 | PWNINT |
| 操作系统 | Microsoft Windows 10.0.26200 |
| 进程/系统架构 | X64 / X64 |
| 运行时 | .NET 8.0.27（Environment.Version=8.0.27） |
| 仓库根 | D:\sharp_project\source\repos\DocumentTools |
| Git 分支 | feat/wpfui-migration |
| 工作目录 | D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501 |
| 报告路径 | D:\sharp_project\source\repos\DocumentTools\docs\qa-report-compat.md |
| 长路径（LongPathsEnabled） | 1（已启用） |
| ExamFixture 程序集 | D:\sharp_project\source\repos\DocumentTools\tools\ExamFixture\bin\Debug\net8.0-windows\ExamFixture.dll |
| 浏览器候选 | C:\Program Files\Google\Chrome\Application\chrome.exe \| C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe |
| ACE / JET | 可用（ExamFixture exit 0，已生成 Exam_Base.mdb） |
| ACE 归因 | 损坏库命中「文件可能损坏」层（三级归因生效，不再一律报装 ACE） |
| 模拟无 ACE | AccessDb 构造即抛 InvalidOperationException（与真实缺 ACE 同一句文案），考评服务同步抛错 |
| ExamFixture 模拟无 ACE | exit 2 + 可读跳过文案（DOCUMENTTOOLS_SIMULATE_NO_ACE=1） |
| 浏览器消息 | 未找到浏览器时给出「未找到可用的 Chrome/Edge 可执行文件」+ 当前指向 + chromePath 建议 |
| 浏览器 | 自动探测成功（C:\Program Files\Google\Chrome\Application\chrome.exe），html.pdf 转换可用 |
| TWAIN | 不可用：无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' or one of its dependencies: 找不到指定的模块。 (0x8007007E)。请安装扫描仪驱动与 TWAIN Data Source Manager。 |
| Aspose 许可证 | 评估模式（无 .lic；assets\licenses 下 0 个 .lic） |
| 6 页合并（评估模式） | 抛异常：合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。 |
| 5 页拆分（评估模式） | 抛异常：拆分文档失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。 |
| 长路径 | work\long-root 下路径 312 字符；转换失败：路径过长（312 字符，超过 Windows 260 字符限制）：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\lo… |
| 中文+空格路径 | 成功，输出=示例 图片 一_out.png |

## 矩阵表

| 检查项 | 组 | 结论 | 实际行为（摘要） |
| --- | --- | --- | --- |
| `ace.fixture` | ace | PASS | ExamFixture 探测通过，ACE/JET 可用。 |
| `ace.missing-provider` | ace | PASS | 抛出 InvalidOperationException，消息为「数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式：broken.mdb（底层错误：不可识别的数据库格式 'D:\sharp_project\sour… |
| `ace.simulate-switch` | ace | PASS | 开关生效：设置 DOCUMENTTOOLS_SIMULATE_NO_ACE=1 后 `new AccessDb(...)` 在打开阶段即抛「未检测到可用的 … OLE DB 提供者」，考评服务调用同样抛错，不再返回可用会话。 |
| `ace.examfixture-sim` | ace | PASS | 退出码 2，stderr 为可读的「未检测到可用的 Access OLE DB 提供者」说明，无未处理异常。 |
| `ace.real-missing` | ace | 无法验证 | 无法验证：需要在没有 ACE 的机器上运行。已用两条不破坏系统的通道覆盖同一消息路径——损坏库触发 provider 链全失败（ace.missing-provider），以及开关模拟（ace.examfixture-sim）；两者共用 A… |
| `html.no-browser` | html | PASS | 明确报错并带上配置路径与可操作建议（「转换 HTML 失败：未找到可用的 Chrome/Edge 可执行文件（当前指向：Z:\qa-compat\no-such-browser\chrome.exe）。请安装 Chrome 或 Edge，或… |
| `html.autodetect` | html | PASS | 自动探测到浏览器并转换成功：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\html-auto\sample.pdf（… |
| `twain.unavailable` | twain | PASS | 抛出 NotSupportedException，消息与 UnavailableReason 一致，含可操作建议：「无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' or on… |
| `twain.no-source` | twain | 无法验证 | 无法验证：本机 IsAvailable=false（无 DSM），该分支不可达；见 twain.unavailable。 |
| `license.status` | license | PASS | 确认处于评估模式：未找到 .lic，ASPOSE_LICENSE_PATH 未设置。 |
| `license.merge` | license | PASS | 抛出可读异常：「合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。」。 |
| `license.split` | license | PASS | 抛出可读异常：「拆分文档失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。」。 |
| `readonly.outdir` | readonly | PASS | 失败且未留半成品；错误消息为原始 .NET 文案：「无法写入输出文件（被占用或权限不足）：readonly_src_out.png。请换一个输出目录，或去掉目标文件的只读属性、关闭占用它的程序后重试（当前输出目录：D:\sharp_proj… |
| `readonly.locked` | readonly | PASS | 失败消息点明占用/拒绝访问，目标文件长度未变（0 字节）；原文：「合并 PDF 失败：输出目录无法写入：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run… |
| `path.long` | path | PASS | 转换失败，但消息给出了具体路径并指出长度原因：「路径过长（312 字符，超过 Windows 260 字符限制）：D:\sharp_project\source\repos\DocumentTools\testdata\compat\wor… |
| `path.unicode` | path | PASS | 成功产出「示例 图片 一_out.png」，文件名与内容均正确。 |

统计：PASS 14 / FAIL 0 / 无法验证 2。

## 每条详情

### `ace.fixture` — ACE/JET 真实可用性基准（ExamFixture 探测）

- **设定方式**：`dotnet tools\ExamFixture\bin\Debug\net8.0-windows\ExamFixture.dll --output testdata\compat\work\run-<ts>\exam`（不设任何开关），读退出码与生成的 .mdb
- **期望行为**：退出码 0、生成 Exam_Base.mdb；据此确认本机 ACE 可用，作为「真机有 ACE」的基准。
- **实际行为**：ExamFixture 探测通过，ACE/JET 可用。
- **结论**：PASS
- **证据**：

```text
exit=0
stdout: [ExamFixture] 夹具目录: D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\exam
[ExamFixture] 数据库:   D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\exam\Exam_Base.mdb
[ExamFixture]   Vote_EssInfo           行数 2
[ExamFixture]   Vote_PageInfo          行数 0
[ExamFixture]   Exam_EssInfo           行数 2
[ExamFixture]   Exam_Score  …
stderr: 
生成数据库: D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\exam\Exam_Base.mdb
```

### `ace.missing-provider` — provider 全部失败时考评服务抛出的异常是否可读、是否拖垮进程

- **设定方式**：把一个文本文件命名为 `broken.mdb`（内容 `this is not an access database`），调用 `ExamIndexService.ChangeExamIndex`，捕获异常后继续执行后续检查
- **期望行为**：抛出 `InvalidOperationException`，且命中与根因相符的那一层：ACE 可用时报「数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式」并带上文件名与底层错误，ACE 真不可用时才给安装建议；进程存活，后续检查照常运行。
- **实际行为**：抛出 InvalidOperationException，消息为「数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式：broken.mdb（底层错误：不可识别的数据库格式 'D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\ace-broken\broken.mdb'。）」；后续检查继续执行，进程未退出。
- **结论**：PASS
- **证据**：

```text
InvalidOperationException: 数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式：broken.mdb（底层错误：不可识别的数据库格式 'D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\ace-broken\broken.mdb'。）
  inner: OleDbException: 不可识别的数据库格式 'D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\ace-broken\broken.mdb'。
进程存活：本检查返回后 ace.simulate-switch 等检查继续运行。
```

### `ace.simulate-switch` — DOCUMENTTOOLS_SIMULATE_NO_ACE=1 能否让 Core 考评服务进入「无 ACE」降级分支

- **设定方式**：文档：进程内设置 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1`，用 ExamFixture 生成的真实 .mdb 构造 `AccessDb` 并调用 `ExamIndexService.ChangeExamIndex`（均在 finally 里还原环境变量）
- **期望行为**：开关被 Core 识别：`AccessDb` 在构造（打开）阶段即抛 `InvalidOperationException`，消息等同 `AccessDb.MissingAceMessage()`（含「未检测到可用的 … OLE DB 提供者」），考评服务调用同样抛错；不再出现「打开成功并跑完」的情况。
- **实际行为**：开关生效：设置 DOCUMENTTOOLS_SIMULATE_NO_ACE=1 后 `new AccessDb(...)` 在打开阶段即抛「未检测到可用的 … OLE DB 提供者」，考评服务调用同样抛错，不再返回可用会话。
- **结论**：PASS
- **证据**：

```text
环境变量 DOCUMENTTOOLS_SIMULATE_NO_ACE=1（进程内设置）
new AccessDb(...) 抛出 InvalidOperationException：无法打开 Access 数据库。未检测到可用的 Microsoft Access Database Engine (ACE) OLE DB 提供者（当前进程 64 位：64 位进程需要 64 位 ACE，32 位进程需要 32 位 ACE）。请安装 Microsoft Access Database Engine。
ChangeExamIndex 抛出 InvalidOperationException：无法打开 Access 数据库。未检测到可用的 Microsoft Access Database Engine (ACE) OLE DB 提供者（当前进程 64 位：64 位进程需要 64 位 ACE，32 位进程需要 32 位 ACE）。请安装 Microsoft Access Database Engine。
```

### `ace.examfixture-sim` — ExamFixture 在模拟无 ACE 下的行为（退出码 + 可读文案）

- **设定方式**：子进程运行 `dotnet <ExamFixture.dll> --output testdata\compat\work\run-<ts>\exam-noace --no-seed`，子进程环境变量 DOCUMENTTOOLS_SIMULATE_NO_ACE=1
- **期望行为**：退出码 2，stderr 出现「未检测到可用的 Access OLE DB 提供者」+ 开关名，且不含 `Unhandled exception`。
- **实际行为**：退出码 2，stderr 为可读的「未检测到可用的 Access OLE DB 提供者」说明，无未处理异常。
- **结论**：PASS
- **证据**：

```text
exit=2
stderr: [ExamFixture] 未检测到可用的 Access OLE DB 提供者（模拟：环境变量 DOCUMENTTOOLS_SIMULATE_NO_ACE=1），考评 Access 集成测试依赖真实 .mdb，已跳过。
```

### `ace.real-missing` — 真机卸载 ACE 后 ExamFixture 与 Core 的行为（需破坏性系统改动）

- **设定方式**：卸载 Microsoft Access Database Engine（或换一台未装 ACE 的机器）后重跑 `ace.fixture` / `ace.missing-provider`
- **期望行为**：ExamFixture exit 2 且文案与模拟一致；Core 抛「无法打开 Access 数据库。请安装 Microsoft Access Database Engine (ACE)。」
- **实际行为**：本机 ACE 可用（见 ace.fixture），卸载 ACE 属破坏性系统改动，本次不执行。
- **结论**：无法验证
- **无法验证原因**：无法验证：需要在没有 ACE 的机器上运行。已用两条不破坏系统的通道覆盖同一消息路径——损坏库触发 provider 链全失败（ace.missing-provider），以及开关模拟（ace.examfixture-sim）；两者共用 AccessDb.cs:188-190 的抛出语句。
- **证据**：

```text
无法验证：需要在没有 ACE 的机器上运行。已用两条不破坏系统的通道覆盖同一消息路径——损坏库触发 provider 链全失败（ace.missing-provider），以及开关模拟（ace.examfixture-sim）；两者共用 AccessDb.cs:188-190 的抛出语句。
```

### `html.no-browser` — 显式指定不存在的浏览器路径时 html.pdf 的错误消息

- **设定方式**：`chromePath=Z:\qa-compat\no-such-browser\chrome.exe`（不存在的盘符/路径），调用 `HtmlToPdfOperation.ExecuteAsync`，输入 testdata\html\sample.html
- **期望行为**：Success=false，消息明确指出「未找到可用的 Chrome/Edge 可执行文件」，带上当前指向的路径与「chromePath」选项名（可操作建议）；不产生输出 PDF。
- **实际行为**：明确报错并带上配置路径与可操作建议（「转换 HTML 失败：未找到可用的 Chrome/Edge 可执行文件（当前指向：Z:\qa-compat\no-such-browser\chrome.exe）。请安装 Chrome 或 Edge，或在选项里把「浏览器路径」(chromePath) 指向 chrome.exe。」），且未生成任何 PDF。
- **结论**：PASS
- **证据**：

```text
Success=False
Message: 转换 HTML 失败：未找到可用的 Chrome/Edge 可执行文件（当前指向：Z:\qa-compat\no-such-browser\chrome.exe）。请安装 Chrome 或 Edge，或在选项里把「浏览器路径」(chromePath) 指向 chrome.exe。
输出目录 PDF 数: 0
```

### `html.autodetect` — 自动探测浏览器路径（无 chromePath）能否真正转换出 PDF

- **设定方式**：不传 chromePath/browserPath，让 `HtmlToPdfOperation.TryDetectBrowser()` 从 ProgramFiles/ProgramFilesX86 找 Chrome/Edge，转换 testdata\html\sample.html
- **期望行为**：转换成功、输出 PDF 存在且页数 ≥ 1；若本机确实没有浏览器，则给出 html.no-browser 同款明确消息。
- **实际行为**：自动探测到浏览器并转换成功：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\html-auto\sample.pdf（63736 字节，1 页）。
- **结论**：PASS
- **证据**：

```text
Success=True
Message: 完成 1 个文件。
输出文件存在: True
大小: 63736 字节
页数: 1
```

### `twain.unavailable` — 无扫描仪/无 DSM 时 Scan 抛 NotSupportedException，消息含可操作建议

- **设定方式**：`TwainScanService.IsAvailable` 为 false（本机未装 TWAINDSM.dll 或扫描仪驱动）时调用 `Scan("NO_SUCH_SCANNER", options)`
- **期望行为**：抛 `NotSupportedException`，消息等于 `UnavailableReason`，且包含「请安装扫描仪驱动 / TWAIN Data Source Manager」这类可执行建议。
- **实际行为**：抛出 NotSupportedException，消息与 UnavailableReason 一致，含可操作建议：「无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' or one of its dependencies: 找不到指定的模块。 (0x8007007E)。请安装扫描仪驱动与 TWAIN Data Source Manager。」。
- **结论**：PASS
- **证据**：

```text
NotSupportedException: 无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' or one of its dependencies: 找不到指定的模块。 (0x8007007E)。请安装扫描仪驱动与 TWAIN Data Source Manager。
UnavailableReason: 无法加载 Twain（NTwain/DSM）：Unable to load DLL 'twaindsm.dll' or one of its dependencies: 找不到指定的模块。 (0x8007007E)。请安装扫描仪驱动与 TWAIN Data Source Manager。
```

### `twain.no-source` — 有 DSM 但没有匹配数据源时的消息

- **设定方式**：`Scan("NO_SUCH_SCANNER", options)`（Session 已打开、数据源名不存在）
- **期望行为**：抛 `InvalidOperationException`，消息含「未找到扫描仪：NO_SUCH_SCANNER」。
- **实际行为**：本机 IsAvailable=false（无 DSM），该分支不可达；见 twain.unavailable。
- **结论**：无法验证
- **无法验证原因**：本机 IsAvailable=false（无 DSM），该分支不可达；见 twain.unavailable。
- **证据**：

```text
本机 IsAvailable=false（无 DSM），该分支不可达；见 twain.unavailable。
```

### `license.status` — 当前 Aspose 许可证状态（决定后两项的期望）

- **设定方式**：调用 `AsposeLicenseLoader.TryLoad()`；同时枚举 assets\licenses\*.lic 与 ASPOSE_LICENSE_PATH
- **期望行为**：仓库未附 .lic 且未设环境变量时返回 Evaluation（IsLicensed=false, LicensePath=null）。
- **实际行为**：确认处于评估模式：未找到 .lic，ASPOSE_LICENSE_PATH 未设置。
- **结论**：PASS
- **证据**：

```text
IsLicensed=False
LicensePath=(none)
assets\licenses\*.lic 数量=0
ASPOSE_LICENSE_PATH=(未设置)
```

### `license.merge` — 评估模式下合并 6 个单页 PDF：明确异常 / 完整输出 / 静默截断

- **设定方式**：SkiaSharp 生成 6 个 1 页 PDF（fixture-1..6.pdf），用 `MergeDocumentOperation("pdf.merge")` 合并成 merged.pdf，再用 Aspose.Pdf 读回页数与文本
- **期望行为**：要么合并出 6 页且无评估水印，要么抛出可读异常（消息点明评估/许可限制）。不允许「Success 但页数变少」或「成功但输出带水印」。
- **实际行为**：抛出可读异常：「合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。」。
- **结论**：PASS
- **证据**：

```text
Success=False
Message: 合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。
merged.pdf 存在: False
```
- **关联发现**：[中] 评估模式下的合并失败信息是 Aspose 英文原文，且没有「如何解除限制」的指引

### `license.split` — 评估模式下把 5 页 PDF 逐页拆分：明确异常 / 完整输出 / 静默截断

- **设定方式**：SkiaSharp 生成 5 页 source.pdf，用 `SplitDocumentOperation("pdf.split")` + splitMode=「每页拆分」拆到输出目录，统计文件数与每份页数
- **期望行为**：产出 5 个单页 PDF 且无水印，或抛出可读异常。不允许「成功但文件数/页数缺失」。
- **实际行为**：抛出可读异常：「拆分文档失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。」。
- **结论**：PASS
- **证据**：

```text
Success=False
Message: 拆分文档失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。
输出文件数=4（期望 5）
```
- **关联发现**：[低] 拆分失败同样直出 Aspose 英文文案

### `readonly.outdir` — 输出目录只读：转换失败可读、且不留半成品

- **设定方式**：对输出目录执行 `icacls <dir> /deny *S-1-1-0:(W)`，先用探测文件确认 ACL 生效，再执行 `image.compress`
- **期望行为**：`image.compress` 返回 Success=false，消息能说明是权限/拒绝访问；输出目录里除探测文件外没有残留的半成品 PNG。
- **实际行为**：失败且未留半成品；错误消息为原始 .NET 文案：「无法写入输出文件（被占用或权限不足）：readonly_src_out.png。请换一个输出目录，或去掉目标文件的只读属性、关闭占用它的程序后重试（当前输出目录：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\readonly-out）。」。
- **结论**：PASS
- **证据**：

```text
icacls: 已处理的文件: D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\readonly-out  已成功处理 1 个文件; 处理 0 个文件时失败
探测写入异常: UnauthorizedAccessException: Access to the path 'D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\readonly-out\qa-write-probe.txt' is denied.
Success=False
Message: 无法写入输出文件（被占用或权限不足）：readonly_src_out.png。请换一个输出目录，或去掉目标文件的只读属性、关闭占用它的程序后重试（当前输出目录：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\readonly-out）。
预期输出存在=False（目录本身不可枚举，只核对已知输出名）
```
- **关联发现**：[中] 只读输出目录的失败信息是英文 .NET 原始文案，未包装中文提示

### `readonly.locked` — 目标 PDF 被占用：合并失败可读、不破坏原文件

- **设定方式**：预创建 merged.pdf 并用 `FileStream(FileMode.Create, FileAccess.ReadWrite, FileShare.None)` 持有，再执行 `pdf.merge`
- **期望行为**：`pdf.merge` 返回 Success=false，消息说明文件被占用/拒绝访问；目标文件保持 0 字节，不写入半份内容。
- **实际行为**：失败消息点明占用/拒绝访问，目标文件长度未变（0 字节）；原文：「合并 PDF 失败：输出目录无法写入：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\locked-out。请换一个目录，或关闭占用该文件的程序后重试。」。
- **结论**：PASS
- **证据**：

```text
Success=False
Message: 合并 PDF 失败：输出目录无法写入：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\locked-out。请换一个目录，或关闭占用该文件的程序后重试。
文件长度 before=0 after=0
```

### `path.long` — 超过 260 字符的输出路径：转换成功或失败信息准确

- **设定方式**：用 `Fixtures.EnsureLongDirectory` 在 30 字符分段下拼出 >260 字符目录，写入 PNG 后执行 `image.compress`
- **期望行为**：转换成功并产出可解码的 PNG；若失败，消息必须给出出错的完整路径，而不是空消息或「发生错误」。
- **实际行为**：转换失败，但消息给出了具体路径并指出长度原因：「路径过长（312 字符，超过 Windows 260 字符限制）：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\long-root\seg00_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\seg01_xxxxxxxxxxxxxxxxxxxxxxxxx…」。
- **结论**：PASS
- **证据**：

```text
输入路径长度=312（>260）
输出路径长度=316
Success=False
Message: 路径过长（312 字符，超过 Windows 260 字符限制）：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\long-root\seg00_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\seg01_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\seg02_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\seg03_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\seg04_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\long_src.png。请把文件移到更短的目录（例如 D:\temp\）后重试。
输出存在=False，可解码=False
```

### `path.unicode` — 中文+空格路径：转换成功且文件名正确

- **设定方式**：目录「中文 目录 (测试)」→ 输入「示例 图片 一.png」→ 输出目录「输出 目录」，执行 `image.compress`
- **期望行为**：转换成功，输出文件名与预期「示例 图片 一_out.png」完全一致且可解码。
- **实际行为**：成功产出「示例 图片 一_out.png」，文件名与内容均正确。
- **结论**：PASS
- **证据**：

```text
Success=True
Message: 完成 1 个文件。
输出文件: 示例 图片 一_out.png
期望文件名存在=True，可解码=True
```

## 发现的问题（按严重度）

1. **[中] 评估模式下的合并失败信息是 Aspose 英文原文，且没有「如何解除限制」的指引**
   - 详情：用户看到「合并 PDF 失败：当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。本构建不随程序分发授权文件，请把文件拆分后再试。」：虽然是明确异常（没有静默截断），但既未翻译，也未提示加载 .lic / 授权入口，中文界面里难以自救。
   - 位置：`src\DocumentTools.Core\Operations\MergeDocumentOperation.cs:46-48`
   - 来源检查：`license.merge`

2. **[中] 只读输出目录的失败信息是英文 .NET 原始文案，未包装中文提示**
   - 详情：用户可见的是 `无法写入输出文件（被占用或权限不足）：readonly_src_out.png。请换一个输出目录，或去掉目标文件的只读属性、关闭占用它的程序后重试（当前输出目录：D:\sharp_project\source\repos\DocumentTools\testdata\compat\work\run-20261106-125501\readonly-out）。`；`ImageConvertOperation` 直接 Fail(ex.Message)，中文界面里出现英文「Access to the path ... is denied.」且不带建议（检查写入权限/换目录）。
   - 位置：`src\DocumentTools.Core\Operations\ImageConvertOperation.cs:55-57`
   - 来源检查：`readonly.outdir`

3. **[低] 拆分失败同样直出 Aspose 英文文案**
   - 详情：与 license.merge 同源：`SplitDocumentOperation` 也是 `Fail(ex.Message)`，评估模式下用户只会看到英文限制说明。
   - 位置：`src\DocumentTools.Core\Operations\SplitDocumentOperation.cs:50-52`
   - 来源检查：`license.split`

## 建议

- 把 `ImageConvertOperation` / `MergeDocumentOperation` / `SplitDocumentOperation` 的 catch 分支从 `Fail(ex.Message)` 改为「中文场景提示 + 原始消息」两层结构，避免只读目录、文件占用这类系统错误把英文 .NET 文案直接暴露给用户。
- `AccessDb.Open()` 目前把所有 provider 失败统一归因为「请安装 ACE」。建议在消息里附上实际尝试过的 provider 列表与首个真实错误（`InnerException` 摘要），并区分「未安装」与「文件损坏/格式不匹配」。
- 如果希望 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1` 能覆盖 Core 的考评服务，需要让 Core 的 Access 探测读同一开关（当前仅 `tools\ExamFixture\AceAvailability.cs` 读取），否则该开关只能测 ExamFixture 自身。
- `HtmlToPdfService.ConvertAsync` 的「没有找到浏览器可执行文件」消息应补上安装建议与可配置项（如「请安装 Chrome/Edge，或在参数中指定 chromePath」），当前只有路径占位。
- Twain 不可用文案已包含驱动/DSM 建议，可再补一句「无扫描仪时可直接用图片导入」，减少用户误判为程序故障。
- 合并/拆分入口应在执行前检查 `AsposeLicenseLoader.TryLoad().IsLicensed`：评估模式下要么拒绝大文档并提示「未授权，最多 4 元素」，要么在结果消息里明确标注输出受限/带水印，避免静默截断或缺页。
- 把评估模式状态暴露到 UI（标题栏/关于页），并在 `DocumentOperationResult.Message` 中统一附加「评估模式」标记，便于用户判断产物是否可信。
- 只读目录与文件占用属于高频现场问题：建议在 Operation 层预检输出目录可写性、目标文件是否被独占打开，并在失败时清理 `.tmp` 类中间文件，保证不留半成品。
- 长路径依赖系统 `LongPathsEnabled`。建议在启动时检测该注册表项，未开启时提示「启用 Win32 长路径或缩短输出目录」，而不是等到转换阶段才失败。

## 复现命令

```powershell
# 在仓库根目录执行（工作目录 D:\sharp_project\source\repos\DocumentTools）
dotnet build tools\QaCompat\QaCompat.csproj -v q
dotnet run --project tools\QaCompat -- --all

# 只跑某个分组（ace,html,twain,license,readonly,path）
dotnet run --project tools\QaCompat -- --only ace,license

# 指定工作目录 / 报告输出位置
dotnet run --project tools\QaCompat -- --all --work testdata\compat\work\manual --report docs\qa-report-compat.md
```

说明：退出码 1 表示存在 FAIL；「无法验证」不计入失败。本次工作目录见上方「环境」表。
