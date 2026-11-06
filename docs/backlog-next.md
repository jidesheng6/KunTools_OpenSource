# Backlog：需求盘点与验收标准（PM / task-1 → task-22 同步 → task-46 基准切换 → dev 对齐收口）

## 2026-11-06 第二轮：真实考评数据包对齐（闸门 PASS）

用户提供了真实数据包 `exambase/exam_tes`（`Exam_Base.mdb` 1.1MB + 95 个 `.exam` 结果包 + 416 份 `SelfReport/*.rpt`；`.exam` 是 Access 库，不是压缩包）。本轮以它为基准逐项跑真实数据，修掉一个只有真实库才会暴露的语义错误，并补齐 dev 的联动与右键交互。数据包已加入 `.gitignore`（1GB 级，不入库）。

### 真实数据实测通过

| 能力 | 实测结果 |
| --- | --- |
| `ListReports` | 12 张报表、4 张考评表（含 ExamName/ExamId 关联） |
| `ListExamRaids` | 4 个身份 + 合计 + 批量全部（合计串 `县级领导干部，科级领导干部，老同志，其他人员`） |
| `GetGridSize` | 如 ReportID=3 → 列 18 × 行 10 |
| `ListExamTableItems`（新增） | ExamID=2 → `第9个 / 第10个 / 总体评价 / 第1个…第8个` |
| `ListItemSelectors`（新增） | 首个项目 → `有 / 弃权`（「无效」按 legacy 排除） |
| `ExamInsertService.Insert` | 在副本上新建考评表成功（项目 1、选项 2、内置无效/弃权 2） |
| `ExamIndexService.ChangeExamIndex` | 新表 7 → 99 成功，物理顺序重建 |
| `ExamPackageService.Inspect/Describe/ListUnits` | 人员 1662、单位 95、`.exam` 95、SelfReport 有 |
| `ExamPackageService.Merge` | 副本互并：插入 1662 人、先清 95 个重复单位、709ms，目标 3324 人 / 95 单位 |

### 本轮修复

| 问题 | 真实数据证据 | 修复 |
| --- | --- | --- |
| **`ListReportItems` 读错列** | 真实库 `Report_Detail.Item` 存的是单元格坐标（`1|1`），项目名在 `Content` 的 `Item=` 段；旧实现返回 `1|1,2|1,…`，报表页项目筛选/批量参数/复制全错 | 按 legacy `Util.GetReportItems` + `VF_Report.QueryContent` 重写：`Kind='GridFormatData' AND Content LIKE 'Bool:%vftab%'` → 解析 `Item=` → 去重 → 自然排序 → 首位插入「所有项目」。真实库现输出 `所有项目 / 第1个 … 第10个 / 总体评价`；回归 `ExamReportParaServiceTests`（真实形态 Content） |
| `GetReportItemOrder` 会把「所有项目」排走 | — | 排序时剔除「所有项目」再自然排序，随后插回首位（legacy 是排完再 Insert(0)） |
| **报表页缺少「考评表项目 → 选项」联动** | dev `ExamItemsCombox_SelectedIndexChanged`（BodyForm.cs:6090-6130）→ `Util.GetExamItems_Selectors` | Core 新增 `ListExamTableItems` / `ListItemSelectors`；`ExamReportView` 报表结构卡新增「考评表项目 / 项目选项」两个下拉，选中项目即加载选项 |
| **报表项目列表没有右键菜单** | dev `ExamReportItemSortMenu` 挂在 `ExamReportRowMode_ExamList`（Designer:13017） | `_itemList.ContextMenu` = 报表项目排序 / 重置项目排序 |
| **缺少「获取…」三个复制动作** | dev `GetExamNameAndSelectorMenu`（Designer:17606）：获取全部项目名称（报表/考评表）/ 获取所选项目的选项 | 报表项目卡新增「获取…」按钮下拉：复制考评表项目名称 / 复制所选项目的选项（报表项目已由「复制项目列表」承担，不重复）；复制后只写状态行，不弹框 |
| 项目筛选是自由文本框 | dev `ExamReportRowMode_ExamList` 是可编辑下拉 | 改为可编辑下拉，下拉源 = 报表实际项目（「所有项目」首位） |
| **MSS3 页选错文件类型** | dev 对话框过滤器是 `*.mss3`（BodyForm.cs:4319-4323） | `ExamViewKit.Mss3Filter` + 选择框改用它；`Mss3VoteService.ListVotes` 对「缺少 Vote_EssInfo」给中文归因（`请选择 .mss3 文件`），不再抛英文 OleDb |
| 插入页缺「移除全部项目」 | dev `InsetExam_BacthMenu` 的 `InsertExam_BatchRemoveItem`（Designer:17921） | 项目与选项卡新增「清空项目」（只清表格，不碰库） |
| 数据包页缺「按序号移动」 | dev `ExamDataPakUnitOrder_QuickFindButton` + `MoveToOrder`（Designer:16194/16211） | 单位顺序条新增「序号框 + 移动到…」：夹到首/尾，仍只改本页顺序，点「保存单位顺序」才写库 |
| **卡片右边缘比文件列表多出侧栏宽度** | 转换页截图：列表右边缘 1150px、卡片 1190px（侧栏 4 个按钮占 40px） | `ConvertFeatureModule` 改为 Grid：卡片与列表同列（第 0 列），侧栏独占第 1 列；快照复核右边缘一致 |
| OCR 组件缺失提示是红色错误样式 | 用户要求「可选功能弱提示」 | 改为中性色标签说明「OCR 是可选功能，当前未安装…不安装不影响 DataMatrix 页码分类」，不弹窗、不阻断 |

**验证**：`dotnet build` 0/0；测试 519 通过 / 0 失败 / 3 跳过；`gate.ps1` 六门 PASS；UiSnapshot 47 页 `fatal/high/medium=0`。

### 后续补充（同轮，7 页控件级审计）

用 dev `BodyForm.Designer.cs` 的每个 Tab 控件清单逐项对照，补齐以下差异：

| 页 | dev 控件 | 本轮实现 |
| --- | --- | --- |
| `exam.report` | `ExamReportRowMode_ExamItemInsertSame` / `...SelectorInsertSame`「插入相同项目/选项」（默认勾选） | 批量参数卡新增勾选 → `InsertSameItemSelector`（Core 字段本就存在，只缺 UI） |
| `exam.report` | label173「评测主体修改内容为空则表示删除」 | `ReplaceContent` 的 `ChangeRaid` 支持「新 Raid 为空 = `Replace('Raid={旧}','Raid=')`」；两值都空时不改任何行（与 dev 实跑一致）。UI 不再把「新 Raid 为空」当缺参拦截；回归 2 例 |
| `exam.package` | `ExamDataPakUnitOrder_QuickFindButton`「查找」（BodyForm.cs:10392-10470） | 单位顺序条新增关键字框 +「查找」：按单位名称/类别/编码循环定位，关键字不变时从下一行继续 |
| `exam.vote` | `VoteTableHelpDate`「填入当前日期」（BodyForm.cs:5784-5790） | 排版卡新增按钮，按 `yyyy年MM月dd日` 填日期 |
| `exam.circle` | `ExamCircleFix_RotateLeft` / `RotateRight`「向左/向右旋转」 | 新增两个方向按钮（单次 90°，落盘仍走「另存带角度后缀」） |

已核对**无差异**的页：`exam.mss3`（选票列表 → 自动带入编码/名称、Item 列转备注/恢复长度）、`exam.single`（三档后缀 + 内置码图联动：勾选内置时清空并要求取消图片源，起始序号只在内置档可用）、`exam.insert`（项目/选项表、得分比重、批量模板、改号、清空项目）。

**7 页审计后的验证**：`dotnet build` 0/0；测试 521 通过 / 0 失败 / 3 跳过；FeatureSmoke 47/47 + 落点 95 项 0 失败；`gate.ps1` 六门 PASS。

## 2026-11-06 补齐轮（本轮新增，闸门 PASS）

用户反馈「重构后很多功能缺失」，本轮按「只信源码/实测、不重复造」复核后补齐三项缺口，全量闸门 `gate.ps1 -MaxSkips 3` **PASS**（build 0/0、test 516 通过 / 0 失败 / 3 跳过、FeatureSmoke 47/47、UiSnapshot 47 页 fatal/high/medium=0、QaFunction 62 项全 PASS、QaCompat 14 PASS）。

| 项 | 之前状态 | 本轮结果 | 证据 |
| --- | --- | --- | --- |
| `code.datamatrix` 图片目录批量分类（dev `Util.cs:6181-6301` DataMatrix 分支） | 仅文档登记「待迁」 | **已实现**：Core `IDataMatrixImageClassifyService` + 8 例归档规则单测 + 1 例真解码冒烟；UI 底部新增「图片目录批量分类」卡 | `DataMatrixImageClassifyService.cs:24,152`、`DataMatrixFeatureModule.cs:315,365`、`DataMatrixImageClassifyTests.cs` |
| `exam.report`「复制评测主体合计身份」 | dev 里是死按钮；新版无此入口 | **已实现**：`BuildRaidClipboardText` + 「复制身份列表」按钮 | `ExamReportService.cs:650`、`ExamReportView.cs:188,564`、`ExamReportExtrasAccessTests` 3 例 |
| `exam.circle` 从扫描仪获取图片 | 未接 `ITwainScanService` | **已实现**：「扫描仪…」按钮，扫描到 `%LOCALAPPDATA%\DocumentTools\scans\{时间戳}` 并载入 | `ExamCircleView.cs:123,574` |
| `exam.report` 报表项目排序 / 重置 | 文档记「未迁（待取证）」 | **复核为已实现**（`GetReportItemOrder` + `_storedItems` 备份），未改动 | `ExamReportView.cs:504,530` |
| `exam.report` 字体输入 | 单框手填「字体,大小,加粗」，写错要等报表渲染才暴露 | **已补齐**：字体下拉（`InstalledFontCollection`）+ 字号（1–200 校验）+ 加粗勾选，拼回 Core 的三段串 | `ExamReportView.cs` `BuildFontSpec`/`InstalledFontNames`、`ApplyBulk` 字号校验 |
| `exam.report` 批量身份（全部身份） | 只能手打 `A\|B\|C\|A，B，C`，且文档记「Core 未消费合计段」 | **复核后修正 + 补齐**：Core 列模式本就按 `\|` 拆行（与 legacy `Util.cs:1153` 一致），缺的是 UI；新增身份下拉，选中即回填。回归锁 1 例 | `ExamReportView._bulkRaidPick`、`ExamReportParaServiceTests.ApplyDataFormat_BatchAllRaidValue_WritesEachIdentityPlusTotalRow` |
| OCR 考试图片分类（`Sdcb.PaddleOCR`） | 记「不迁，干净环境不可用」 | **已补齐（可选组件）**：依赖全部可从 NuGet 还原（推翻旧结论），做成独立 `DocumentTools.Ocr` 项目，宿主从 `ocr\` 按需加载；缺失时页面 OCR 选项禁用并给安装提示。**实测 ocr 目录 537MB**（V3/V4 模型均必须保留），主程序体积不变。端到端已跑通（中文识别 + 归档 + 画框） | `src\DocumentTools.Ocr\`、`OcrPluginLoader.cs`、`packaging.md` §8 |

**校验注意**：从 Git Bash 调 `gate.ps1` 必须先设 `[Console]::OutputEncoding=[Text.Encoding]::UTF8`，否则中文摘要被按 GBK 读取、数字解析成 -1 且门 1 误判 FAIL（本轮踩过，原生 PowerShell 窗口无此问题）。


**当前阶段：dev 对齐收口，待用户验收。**

- 生成：2026-11-05 19:40，**最近同步 2026-11-06 01:05（Asia/Shanghai，收口定稿）**，分支 `feat/wpfui-migration`。
- **基准分支 = `dev`（tip `ce4cc88`）**：业务行为与 UI 行为的对照物从「legacy WinForms 原版」升级为「`dev` 的服务层重构版」。**四条对齐线已收口**，判断面在 `docs\dev-parity-convert.md`（转换域）、`docs\dev-parity-tools.md`（工具/系统域）、`docs\dev-parity-exam.md`（考评域）、`docs\qa-report-ux.md` + `docs\ux-shell-polish.md` + `docs\ux-tools-polish.md`（UX 三线）；发布形态见 `docs\packaging.md`；索引见 `docs\INDEX.md`。本文件的 §1–§3 结论仍按「可判定的行为契约」写，与审计报告冲突时以报告为准。
- **本文件是 UI-A / UI-B / 开发 1–3 / 测试 1–2 的共同验收口径**。每条结论必须能判「对 / 不对」，不允许「更美观」「体验更好」这类不可判定表述。
- 证据约定：所有现状都带 `文件:行号`。**行号取数于 2026-11-06 01:02（收口定稿时逐条复核）**；`src\DocumentTools.App\**` 与 `FeatureOptionSchemas.cs` 仍可能被改动，行号只用于定位，**验收以「行为契约」为准**（本轮已按源码实际内容修正 9 组漂移行号，明细见 `docs\closeout-ledger.md`）。内核层（Operations / Services）行号相对稳定。
- 状态标记：`[已关闭]` = 行为达标且我复核过证据；`[部分]` = 部分子项达标；`[开放]` = 仍需动手。逐条证据与我的结论在 `docs\closeout-ledger.md`。
- 键名归属：参数键的**唯一归属文件**是 `src\DocumentTools.Core\Features\Options\FeatureOptionSchemas.cs`；Operation 映射归 `src\DocumentTools.Core\Operations\**` 与 `Services\Office\**`。UI 不得为了「先看到控件」临时改 Schema。

---

## 0. 验收门槛（每次交付前全绿，逐条可判定）

| 编号 | 命令 | 通过判据 | 最近实测 |
| --- | --- | --- | --- |
| G0 | `& .\tools\gate.ps1 -MaxSkips 3` | 总判定 `PASS`、退出码 0（六门串跑，任一门失败即停） | **通过态定稿：PASS / 退出码 0 / 142 s**（2026-11-06 01:00:08–01:02:30，日志 `%TEMP%\dt-gate-20261106-010008.log`）；下面各门的定稿数字取自这一跑。**跳过 3 = 三条 Word opt-in 白名单**（`gate.ps1` 逐条打印用例全名，`tools\gate.ps1:218`），上限是白名单不是免检额度：**无 Word 的机器传 `-MaxSkips 0` 也不该红**（`:28-33`）。耗时基线：`dotnet test` 约 54 s（整集串行）、全链约 142 s |
| G1 | `dotnet build DocumentTools.sln --no-incremental -v q --nologo` | `0 个警告`、`0 个错误`（出现 MSB3061/MSB3026「文件被 testhost 锁定」时等 20 秒重跑一次；仍红才算失败） | 闸门内 `0 个警告 / 0 个错误`（4.2 s）；20:50 单跑亦 0/0（20:46–20:49 曾连续撞到他人并发红：`ExamInsertView.cs` CS0103 `WeightOr`、`ExamVoteView.cs` CS0103 `ParseDouble`、`SingleDocumentNamingTests.cs` CS1674，均自愈） |
| G2 | `dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo` | `失败: 0` | **定稿（01:00 闸门内）：通过 505 / 失败 0 / 跳过 3（上限 3）/ 总计 508，53.8 s**；22:02 闸门内为 363/366（69.4 s），增量是本轮新增的 dev 对齐与 UX 回归用例。3 个跳过 = 三条 Word opt-in 白名单用例（需 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`），`gate.ps1` 逐条打印全名（`tools\gate.ps1:218`），无 Word 机器传 `-MaxSkips 0` 也不该红（`:28-33`）。20:57/21:03 记到的「失败 1 / 305」是旧二进制下的过时断言（`Task13OptionTests.cs:219-233` 现已按 3 档断言），已作废 |
| G3 | `dotnet run --project tools\FeatureSmoke -v q --nologo` | 两行：`功能总数 47，创建成功 47，布局通过 47` 与 `启动落点检查 95 项，失败 0` | 闸门内两行全绿（6.2 s，exit 0；第二行是 `NavigationFallback` 判定：可见功能按配置打开、被隐藏/未注册的启动功能回退首页并有说明） |
| G4 | `Get-ChildItem src,tests,tools -Recurse -Filter *.cs \| Where-Object FullName -notmatch '\\(bin\|obj)\\' \| Select-String -Pattern 'Content = "\?{3,}\|base\("\?{3,}\|PlaceholderText = "\?{3,}'` | 0 命中 | 20:05 0 命中 |
| G5 | `Get-ChildItem src,tests,tools -Recurse -Filter *.cs \| Where-Object FullName -notmatch '\\(bin\|obj)\\' \| Select-String -Pattern '锛\|銆\|鐨\|鏂\|鎴\|鍥剧\|鏂囨。\|闇€'` | 0 命中（GBK→UTF8 二次编码自检，G4 抓不到） | 20:05 0 命中 |
| G6 | `Get-ChildItem src\DocumentTools.App -Recurse -Filter *.cs \| Where-Object FullName -notmatch '\\(bin\|obj)\\' \| Select-String -Pattern 'using Aspose'` | 0 命中（唯一例外 `src\DocumentTools.App\Infrastructure\Licensing\AsposeBootstrap.cs`） | 20:05 0 命中 |
| G7 | `Get-ChildItem src\DocumentTools.Core -Recurse -Filter *.cs \| Where-Object FullName -notmatch '\\(bin\|obj)\\' \| Select-String -Pattern 'MessageBox\|System\.Windows\.(Controls\|Media)'` | 0 命中 | 20:05 0 命中 |
| G8 | `Get-ChildItem src\DocumentTools.App\Shell,src\DocumentTools.App\Features -Recurse -Filter *.cs \| Select-String -Pattern 'MessageBox\.Show'` | 只允许 1 条：`src\DocumentTools.App\Shell\CrashLog.cs:94`（容器未就绪时的崩溃兜底，`:93` 注释声明「全库唯一允许」）；出现任何其它命中即失败 | 20:05 仅 `src\DocumentTools.App\Shell\CrashLog.cs:94` 一条 |

G5 与 `docs\team-brief.md` 第 3 节口径一致。**一次跑完六门**用 `& .\tools\gate.ps1 -MaxSkips 3`（Windows PowerShell 5.1；脚本已带 BOM，缺 BOM 时 5.1 会按 ANSI 解析中文注释并报一串语法错误；开始前会因 `DocumentTools.exe`/`testhost` 占用而 ABORT，属正常防护）。**跳过 3 = 三条 Word opt-in 白名单用例**（需 `DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1`，`gate.ps1` 逐条打印用例全名，`tools\gate.ps1:218`）；上限是白名单不是免检额度，**无 Word 的机器传 `-MaxSkips 0` 也不该红**（`tools\gate.ps1:28-33`），判定逻辑见 `:205`。**写码禁令**：含中文的 `.cs` 必须 UTF-8；禁止 `Get-Content | Set-Content`、`>`、`Out-File`、`Set-Content` 直写 `.cs`（默认 ANSI 会把整文件中文变成 `锛?`，不可逆）。

---

## 1. 转换页通用契约（26 个 `GenericConvertView` 功能共用）

适用：`word.pdf` `pdf.word` `excel.pdf` `ppt.pdf` `image.pdf` `pdf.excel` `pdf.ppt` `word.excel` `excel.word` `word.epub` `epub.word` `epub.pdf` `word.image` `pdf.compliance` `pdf.merge` `word.merge` `pdf.split` `word.split` `ppt.split` `pdf.watermark.add` `pdf.watermark.remove` `pdf.compress` `html.pdf` `image.compress` `webp.image` `image.ico`（`src\DocumentTools.Core\Features\BuiltInFeatures.cs` 的 `ConvertFeatures`）。

| 编号 | 契约 | 判定方式 | 现状 |
| --- | --- | --- | --- |
| C1 | 纵向顺序：标题/描述 → 列表（占剩余高度）→ 参数分区（贴底之上）→ 操作条（贴底） | 列表顶边在参数卡顶边之上；操作条贴窗口底 | **达标**：`ConvertFeatureModule.cs:134`（`SetBody(BuildBottomActionLayout(listRow, _optionsHost, actionBar))`）+ `FeaturePageBase.cs:52-70` |
| C2 | 列表右侧竖排：上移 / 下移 / 移除选中 / 清空列表 | 4 个图标钮可见，点击后行序/行数按预期变化 | **达标**：`ConvertFeatureModule.cs:103-106` |
| C3 | 空列表点「开始处理」只改状态文本，**不弹目录框** | 文本 `请先点「选择文件」把要处理的文件加进列表。`，无文件夹对话框 | **达标**：`:149` |
| C4 | 取消目录：文本 `已取消，没有处理任何文件。`，不产出文件 | 文本逐字一致；输出目录无新文件 | **达标**：`:216`（含 `:347` 批处理路径） |
| C5 | 处理中禁用「开始处理」 | 按钮 `IsEnabled=false`，结束恢复 | **达标**：`:155` 禁用、`:204` 恢复 |
| C6 | 处理中含「第 N 个文件」；结束文本以 `处理完毕` 开头 | 3 文件时依次出现 `正在处理第 1 个文件`…`第 3 个文件`，结束 `处理完毕，成功 3 个文件。` | **达标**（转换页）：`:275-276` 进度、`:302-304` 汇总。**替换页仍整批**（`ReplaceFeatureModule.cs:149` 只报总数、`:152-157` 一次调用）→ 见 P1-2 残留 |
| C7 | 「保存到源目录」**逐文件**写各自源目录，不弹目录框 | `A\a.docx`+`B\b.docx` → `A\a.pdf` 与 `B\b.pdf`，不交叉、无目录框 | **达标**：`:183-188`（`writeToSource: true`，注释 `:185` 写明不需统一目录、不弹框）、`:283` 逐文件 `OutputDirectory`。唯一需要统一目录的只剩多对一（`image.pdf` 合并模式 `:168-181`、合并页 `:160`） |
| C8 | `ShowOpenTargetDialog` 时成功后问「处理完毕，是否需要打开转换后目录？」 | 出现确认框（标题「提示」），选是打开目录 | **达标**：`:412`；跨目录回写场景不会问（`singleOutputDirectory` 判定 `:307`、`:321`） |
| C9 | 「选择文件」过滤器按功能限定 | 过滤串与功能匹配（§2 各行） | **达标**：`:86`、`:124` 用 `FeatureOptionSchemas.GetFileFilter`（`FeatureOptionSchemas.cs:602-621`） |
| C10 | 拖入只收本页后缀；目录只展开一层 | `word.pdf` 拖 `.txt` 被拒并提示；拖含子目录的文件夹只加入第一层 | **达标**：`:86` 注入 `FileFilter`、`:87-89` 提示 `已忽略 {rejected} 个不支持的文件。`；目录一层展开在 `BorderedFileList` 内 |
| C11 | 下拉只许点选 | 所有 ComboBox `IsEditable=false` | **达标**：`OptionPanelBuilder.cs:137-139`（`IsEditable=false` + `DisplayMemberPath=Label` + `SelectedValuePath=Value`） |
| C12 | 网格线/行高跟随系统设置 | 改设置后新开页面生效 | **达标**：`ConvertFeatureModule.cs:79-81`、`AppSettings.cs:13-16` |
| C13 | 失败可判定 | 状态行转错误色并给出原因；不静默 | **达标（口径已变）**：逐文件失败汇总 `:302-304`（>`3` 条时另加「（另有 N 个失败未列出）」）、`:393` 错误色、`:423` 合并失败写 `result.Message`。**不再逐文件弹错误框**（比 legacy 少打扰）；若要恢复弹框，需 Lead 裁决 |

合并页（`word.merge` `pdf.merge`）额外：`outputFile` 完整路径、顺序=列表顺序、「开始处理」弹**保存文件**对话框、默认名 `合并.docx` / `合并.pdf`。现状：`ConvertFeatureModule.cs:160`（`RunMergeAsync`）、`MergeDocumentOperation.cs:30-44`（`Ok("合并完成。")`）。

---

## 2. 47 个功能逐条验收标准

「参数键」必须同时满足：① 出现在 `FeatureOptionSchemas`；② 出现在该功能面板；③ 被 Operation 真实读取。「输出物」写死扩展名。提示语一律给可逐字比对的原文。

### 2.1 开始 / 关于

| FeatureId | 结构 | 参数键 | 提示（可判定） | 现状 |
| --- | --- | --- | --- | --- |
| `home` | 分类分组卡片墙，组头「分类名 + N 项」 | 无 | 空态 `还没有可以进入的功能：功能模块未注册，或已在「系统设置」里被隐藏。` | 达标：`HomeFeatureModule.cs:44,80-88,98-124,167` |
| `about` | 五卡：许可 / 运行信息 / 软件说明 / 引用库 / 捐赠 | 无 | 日志缺失 `Hook 日志还没有生成，先运行一次需要转换的功能再回来看。` | **达标**[已关闭 P2-3]：`AboutFeatureModule.cs:55-58` 许可、`:59-66` 运行信息、`:67-72` 软件说明、`:73-76` 引用库、`:77-81` 捐赠（`:81` 按钮「查看捐赠说明」） |

### 2.2 WORD

| FeatureId | 结构（原型 A） | 参数键 | 输出物 | 现状 |
| --- | --- | --- | --- | --- |
| `word.pdf` | 列表 + `输出与后续` + `转换参数` | `saveSource`/`openPdfOptions`/`embedFonts`/`updateFields`/`useLocalOffice`/`compliance`/`colorMode`/`msWordVersion`/`password`（**`removeTempDocx` 已删键**：实现不产生 temp docx 目录，见 `docs\feature-gap.md` §9） | `{同名}.pdf` | 达标：Schema `:211-224`（`:213-215` 输出与后续、`:216-223` 转换参数）；`useLocalOffice` 真接 Word COM（`OfficeConvertService.cs:31`→`:110`）；后处理 `ConvertFeatureModule.cs:394-414`（`openPdfOptions` 逐个打开 PDF） |
| `word.merge` | 列表 + 无参数区 | `outputFile`（内部键） | `合并.docx` | 达标：Schema `:543`（`[]`）、`MergeDocumentOperation.cs:30-44` |
| `word.split` | 列表 + `拆分设置` | `saveSource`/`splitMode`/`pagesPerFile`/`range`/`refreshPageNumber` | `{同名}_….docx` | 达标：Schema `:323` + `SplitSections`（定义 `:625`，条件追加 `:642-645`）；`SplitDocumentOperation.cs:61-62,84`（word 分支消费 `refreshPageNumber`/`resetPageNumber`） |
| `word.excel` | 列表 + 单选项 | `saveSource`/`exportSingleSheet` | `{同名}.xlsx` | 达标：Schema `:270-274`、`OfficeConvertOperation` 对应分支 |
| `word.image` | 列表 + `图像输出` | `saveSource`/`extension`/`dpi`/`gray`/`pageExport` | 由 `extension` 决定 | 达标：Schema `:276-296` |
| `word.epub` | 列表 + `EPUB 参数` | `author`/`coverImage`/`coverDpi` | `{同名}.epub` | 达标：Schema `:298-307` |
| `word.replace` | 列表 + `查找内容` / `替换内容` 双卡 + `匹配选项`/`WORD 替换选项`/`替换后字体样式` | `regex`/`matchCase`/`wholeWord`/`ignoreFootnotes`/`ignoreShapes`/`ignoreFields`/`ignoreFieldCodes`/`ignoreDeleted`/`ignoreInserted`/`regexSub`/`replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold` + `pairCount`/`pair{i}.find`/`pair{i}.replace` | `{同名}_替换.docx` | **达标**[已关闭 P1-4]：双卡 `ReplaceFeatureModule.cs:91-97`；键 `:550-571`（`ignoreFootnotes` `:556`、字体样式分区 `:566-570`，legacy 文案「更改替换的字体和大小」见 Designer `7732`）、`ReplaceDocumentOperation.cs:39,91-130` |

### 2.3 PDF

| FeatureId | 结构 | 参数键 | 输出物 | 现状 |
| --- | --- | --- | --- | --- |
| `pdf.word` | A | `saveSource`/`useOcr`/`useBullet`/`mode`/`password` | `{同名}.docx` | 达标：Schema `:216-229` |
| `pdf.excel` | A | `saveSource`/`oneSheet` | `{同名}.xlsx` | 达标：Schema `:245-249` |
| `pdf.ppt` | A | `renderImage`/`splitShape`/`extraOcr` | `{同名}.pptx` | 达标：Schema `:261-268` |
| `pdf.merge` | B 合并 | `outputFile` | `合并.pdf` | 达标：见 §1 合并额外项 |
| `pdf.split` | C 拆分 | `saveSource`/`splitMode`/`pagesPerFile`/`range`/`splitByBookmark`/`deleteStart`/`deleteEnd`（**无** `refreshPageNumber`） | `{同名}_….pdf` | **达标**[已关闭 P1-7，B 方案]：Schema `:309-312`；`SplitDocumentOperation.cs:67-68,78,114-133`（`deleteStart`+`deleteEnd` 都填才删页） |
| `pdf.watermark.add` | C，4 分区：`输出` / `水印内容` / `对齐与位置` / `平铺设置` | 20 键：`saveSource`/`text`/`image`/`opacity`/`rotate`/`fontName`/`fontSize`/`fontColor`/`horizontal`/`vertical`/`imageWidth`/`imageHeight`/`customPosition`/`positionX`/`positionY`/`fullScreen`/`repeatSpacingX`/`repeatSpacingY`/`underContent`/`useFileName` | `{同名}_水印.pdf` | **达标**：Schema `:329-354`（分区 `:331`/`:332`/`:340`/`:348`，`fontColor` `:339`、`horizontal` `:341`、`vertical` `:342`、`imageWidth/Height` `:343-344`、`customPosition` `:345`、`positionX/Y` `:346-347`、横纵间距 `:350-351`）；`PdfWatermarkOperation.cs:63`（`:105,:115-127`）；Service `PdfWatermarkService.cs:82,100,127,155,165-182` |
| `pdf.watermark.remove` | A，仅 `输出` | `saveSource` | `{同名}_无水印.pdf` | 达标且与 legacy 一致（legacy 该页只有列表+3 按钮：`DocumentTools\BodyForm.Designer.cs:4828-4904`） |
| `pdf.compliance` | A | `saveSource`/`version` | `{同名}.pdf` | 达标：Schema `:346-358` |
| `pdf.compress` | C，3 分区：`参数设置` / `资源精简` / `输出` | `quality`/`imageVersion`/`maxDpi`/`webOptimize`/`unembedFonts`/`flattenForms`/`flate`/`skipImages`/`removeUnusedStreams`/`linkDuplicateStreams`/`removeUnusedObjects` | `{同名}_压缩.pdf` | **达标**：Schema `:360-378`（`imageVersion` `:365`、资源三开关 `:373-376`）；Operation `PdfCompressOperation.cs:36,39-42,94`。**回归注意**：三开关**面板默认 true**，键缺省时同样走 `ParseBoolDefaultTrue`（`:39-41`） |
| `html.pdf` | C，4 分区：`输出` / `设置 chromium 路径` / `页面边距设置` / `生成PDF参数设置` | `chromePath`/`marginTop`/`marginBottom`/`marginLeft`/`marginRight`/`paper`/`landscape`/`useCss`/`waitResources`/`timeout`/`navTimeout` | `{文件名}.pdf`；URL 输入 `page_{N}.pdf` | **达标**：Schema `:422-439`（`landscape` `:434`、`navTimeout` `:438`）；`HtmlToPdfOperation.cs:35,40,48-49,119`；空输入 `请先选择 HTML 文件或填入 URL。` |

### 2.4 EXCEL

| FeatureId | 结构 | 参数键 | 输出物 | 现状 |
| --- | --- | --- | --- | --- |
| `excel.pdf` | A | `saveSource`/`eachSheet`/`fitOnePage`/`horPrint`/`verPrint`/`printScaleRow`/`printScaleCol`/`printScalePercentEnable`/`printScalePercent` | `{同名}.pdf` | 达标：Schema `:231-243` |
| `excel.word` | A，仅 `输出` | `saveSource` | `{同名}.docx` | 达标：Schema `:545-548` |
| `excel.replace` | 同 `word.replace` + `EXCEL 清空行数据` | 匹配 3 键（**不含** `ignoreFootnotes`）+ `excelStartRow`/`excelEndRow`/`excelRemoveRows` | `{同名}_替换.xlsx` | **达标**：双卡同 `ReplaceFeatureModule`；键 `:573-583`（清空行数据 `:579-582`）。Excel 无脚注语义，故不加该键 |

### 2.5 PPT

| FeatureId | 结构 | 参数键 | 输出物 | 现状 |
| --- | --- | --- | --- | --- |
| `ppt.pdf` | A | `saveSource`/`exportNotes`/`hiddenSlides`/`drawBlackLine`/`password` | `{同名}.pdf` | **达标**[已关闭 P2-8]：Schema `:265` `Choice("exportNotes","备注导出位置",PptNotePositions,0)`，选项表 `:60-65` 为**三档**（不导出/页底完整/页底截断；`RightFull` 因出口静默退回 `None` 已撤档，注释 `:58-59`）；映射 `OfficeConvertOperation.cs:123` → `:217-237`（映射层仍兼容 `RightFull` 与旧布尔 `true/false`） |
| `ppt.split` | C 拆分 | `saveSource`/`splitMode`/`pagesPerFile`/`range`/`slideSize`/`slideScale`（**无** `refreshPageNumber`） | `{同名}_….pptx` | **达标**[已关闭 P1-7]：Schema `:324-327`（`slideSize`/`slideScale` 在 `:662-668`）；`SplitDocumentOperation.cs:97-98` |

### 2.6 图片 / 电子书

| FeatureId | 结构 | 参数键 | 输出物 | 现状 |
| --- | --- | --- | --- | --- |
| `image.pdf` | C，3 分区：`参数设置` / `页边距` / `输出` | `onePdfPerImage`/`compress`/`autoOptimize`/`orientation`/`marginTop`/`marginLeft`/`marginBottom`/`marginRight` | 单图 `{同名}.pdf`；合并 `images.pdf` + `已合并 {N} 张图片为 PDF。` | **达标**：Schema `:441-454`（`autoOptimize` `:447`、`orientation` `:448`）；Operation `OfficeConvertOperation.cs:297-309` |
| `image.compress` | C，5 分区：`保存参数设置` / `JPG/JPEG参数设置` / `PNG参数设置` / `BMP参数设置` / `输出` | `format`(PNG/JPG/JPEG/BMP/TIF/TIFF/GIF)/`useSourceExtension`/`noCompress`/`quality`/`jpegColorMode`/`pngLevel`/`pngColorType`/`bmpPixelBits` | `{同名}.{format}`（勾 `useSourceExtension` 时沿用源后缀） | **达标**[已关闭 P2-7]：Schema `:456-484`（`:474`/`:475`/`:483`）；`ImageConvertOperation.cs:71,74-75`。**注意**：`useSourceExtension` 默认 **false**（面板无跨字段禁用能力，故让「输出格式」如实生效，见 Schema `:472-473` 注释） |
| `webp.image` | A | `saveSource`/`format` | `{同名}.{png/jpg/bmp}` | 达标：Schema `:486-496` |
| `image.ico` | A，分区 `ICO图标转换` | `saveSource`/`format`(16/32/64/128/256)/`multiSize` | `{同名}.ico` | **达标**：Schema `:500-513`（ICO 256 `:510`、`multiSize` `:512`）；`ImageConvertOperation.cs:36,44-46`。`multiSize` 是相对 legacy（单尺寸 `DocumentTools\Util.cs:1774-1788`）的增强，不算缺口 |
| `epub.word` / `epub.pdf` | A，仅 `输出` | `saveSource` | `.docx` / `.pdf` | 达标：Schema `:545-548` |

### 2.7 工具（原型 E：无假文件列表）

| FeatureId | 结构 | 关键参数/控件 | 提示（可判定） | 现状 |
| --- | --- | --- | --- | --- |
| `code.barcode` | 分区：`条码内容设置` / `条码外观设置` / `条码标题设置` / `LOGO 设置` / `条码预览` / `条码识别`；按钮 生成预览/批量生成/保存 PNG/批量保存/复制图片/识别条码/复制识别内容 | `symbology`/`moduleSize`/`dpi`/`sizeMode`/`barcodeWidth`/`barcodeHeight`/`rotate`/`caption`/`captionPosition`/`captionFontSize` + LOGO | `请先输入条码内容，再点「生成预览」。`、`未识别到条码内容。`、`还没有识别结果：请先「识别条码」。` | **达标**[已关闭 P1-3 + P2-1]：外观键 Schema `:407-410`；识别 `BarcodeFeatureModule.cs:90,93,94`，Core `IBarcodeService.cs:25` + `BarcodeService.cs:59,73` |
| `code.datamatrix` | 上：文件列表；下：`解析信息` 结果树 | 无参数键 | `请先点「选择文件」加入要校验的 Word/PDF。`、`校验完成：{decoded} 个文件识别到 {found} 处 DataMatrix` | **达标**[已关闭 P2-5]：`DataMatrixFeatureModule.cs:54`（拖放过滤）、`:72-73`（`Card("解析信息", _treeFrame)`） |
| `idcard.check` | 分区 `身份证号码` / `校验结果`（4 行：出生日期/性别/顺序码/校验码） | 无参数键 | `校验通过：编号格式与校验码均正确。`、`号码格式不正确：请确认是 18 位数字（末位可为 X）。`、`校验码不正确：末位应为 {X}。` | **达标**[已关闭 P2-2]：`IdCardFeatureModule.cs:44-47` |
| `name.vertical` | 三卡 `源文本` / `重名检查` / `竖排结果` + 三张参数分区（文本清洗 / 文字处理 / 拼音与重名） | `removeNumber`/`removeSquare`/`distinct`/`doubleAlign`/`replaceEnabled`/`replaceSearch`/`replaceTarget`/`replaceRegex`/`pinyinEnabled`/`pinyinMode` | `还没有可导出的结果：请先点「转换」。`、`已导出到 {path}`、`导出失败：{message}` | **达标**[已关闭 P2-6]：UI 全接线 `NameVerticalFeatureModule.cs:109,113-114,124,154,170,188-190,219-225`；Core `NameVerticalService.cs:10,39,73,82,120,135`；Schema `:515-531` |
| `twain.debug` | 4 分区：`TWAIN 设置` / `扫描配置` / `数据信息` / `扫描图片预览` | DPI/双面/显示扫描仪 UI/跳过空白页 | `未检测到设备。`、`扫描完成：{N} 个文件。` | **达标**[已关闭 P2-4]：`MiscToolModules.cs:102-103,107-108,113-114,122`；接口语义 `ITwainScanService.cs:22-26`（无设备抛 `NotSupportedException`、没扫到返回空集合） |
| `other.misc` | `内嵌工具` 分组（下载 .NET FX 3.5 / 注册表脚本 / KMS 工具 / ReNamer） | 无参数键 | 缺资源 `未找到资源…`（`MiscToolModules.cs`） | 达标：对应 legacy `TabPage_13`（Designer `10540/10577/10770/10798`）；无 Primary 按钮是 Lead 批准的取舍 |
| `other.extra` | 分区 `金额大写` / `路径监视` / 说明 | 无参数键 | `请输入有效金额（大于 0）。`、`请先选择有效目录。` | 达标：legacy 对应 `TabPage_36`（Designer `14905`） |

### 2.8 考评（原型 F，7 页，无假文件列表）

| FeatureId | 分区/字段 | 提示（可判定） | 现状 |
| --- | --- | --- | --- |
| `exam.mss3` | 数据库 / 选票图片 / 维护 | `数据库不存在，请重新选择。`、`已插入：{voteId} 第 {pageIndex} 页（{squareType}）。` | 达标：`ExamMss3View.cs:26-34,112,168,198` |
| `exam.single` | 模板/图片目录/输出目录/份数/Alt 文本（默认 `人员码`） | `未找到可处理的图片。`、`已生成 {n} 份文档：{output}` | 达标：`ExamSingleView.cs:20-33,104,109` |
| `exam.insert` | 数据库 / 考评表 / 项目行 / **`批量项目`** / **`批量选项`** / **`重排表号`** | `数据库不存在，请重新选择。` | **达标（扩充）**：卡片 `ExamInsertView.cs:95,103,109,131-132,144-145,153-154`；改号 `ExamIndexService.cs:93,284`；模板 `ExamBatchInsertTemplates.cs:28-41`（11 组内置选项） |
| `exam.report` | 数据库 / 报表列表 / **`报表结构`** / 替换（含 `新的数据格式`、`只替换该 Raid`） | `扫描 {ScannedRows} 行，修改 {ChangedRows} 行。`、`请填写新的数据格式。` | **达标（扩充）**：`ExamReportView.cs:106-119`（报表结构卡）+ `:121-135`（内容替换卡，新数据格式 `:134`）+ `:269`（空值拦截）；服务 `ExamReportService.cs:75,77,89,190-192` |
| `exam.vote` | 文本 / 人员（4 勾选）+ 排版三项（字号/行高/生成日期） | Schema `报告单排版` 三键 `voteFontSize`/`voteRowHeight`/`voteGenDate`；UI 消费前两键 | `未找到模板 embedded/VoteReportDoc.doc。`、`已生成 {n} 人的报告单：{path}` | 达标：`ExamVoteView.cs:20-53,104,128`；排版：默认值取自 Schema `SchemaDefault`（`:64,:70`）、生成日期直传 `GenerationDate`（`:189`，字段 `IVoteReportService.cs:35`）；**`voteGenDate` 无消费点 → 见 P2-10** |
| `exam.package` | 目录 / 单位 + 2 勾选 | `目标目录下没有 Exam_Base.mdb。`、`合并完成：{Summary}`、`已重置目标数据包（删除 {rows} 行人员/单位记录）。` | 达标：`ExamPackageView.cs:24-42,133,222,255`；写库结果只有合成库覆盖（§5） |
| `exam.circle` | 左右区 + 阈值/最大匹配/旋转 | `图片不存在：…`、`已修复 {n} 处并保存：{result}`、`已旋转 {deg}° 并另存：{rotated}` | 达标：`ExamCircleView.cs:28-88,115-173,414,448` |

### 2.9 系统 / 设置

| FeatureId | 分区/字段 | 判定点 | 现状 |
| --- | --- | --- | --- |
| `system.settings` | `交互`（询问打开目录/高亮导航/网格线/行高）、`启动与隐藏`（起始功能/隐藏功能）、`路径`（Chrome/Mss/Exam） | 保存写 `%LOCALAPPDATA%\DocumentTools\settings.json`；行高夹到 18..64；空起始功能回落 `home`；页面必须写明「未迁移：FormWidth / FormHeight / WidgetAnchorMode」 | 达标：`SystemFeatureModules.cs` 设置页 + `AppSettings.cs:13-28` |
| `system.tools` | `系统配置` / `快捷方式` / `键盘工具`（三卡，按钮内嵌各自卡） | 需管理员项在无权限时给提示而非静默失败 | 达标（源码级）；提权分支见 §5 第 2 条 |
| `system.network` | 上：网卡列表（名称/状态/IPv4/DHCP/描述）；下：`IP 设置`（IP/掩码/网关/DNS1/DNS2）+ 启用/禁用/静态/DHCP | 列表非文档列表壳；失败有错误文案 | 达标；**已知 Low**：名称列与描述列截断无省略号（截图可见，`_issues.csv` 6 条） |

---

## 3. 缺口清单

### 3.1 P0（编码/构建底线，已闭环，保留为例行闸门）

**P0-1 中文乱码与构建阻断 —— 已修复（Lead，19:25 恢复）**
- 历史现象：`src\DocumentTools.Core\Services\Office\OfficeConvertService.cs` 被 PowerShell 机械重写为 GBK→UTF8 二次编码，字符串收尾引号被吞 → Core 10 个错误（CS1010/CS1026/CS1002/CS8086）。
- 期望：任何提交都不得让 G1/G5 变红；`.cs` 一律 UTF-8。
- 验收：G1 `0 个警告 0 个错误`；G5 0 命中。最近实测 19:57 `0/0`、G4/G5 均 0 命中。
- 角色：全员；规则已进 `docs\team-brief.md` 编码硬规则。

### 3.2 P1（合同违约 / spec 明文要求）

| 编号 | 现象（证据） | 期望行为 | 验收方式（回归命令） | 状态 |
| --- | --- | --- | --- | --- |
| **P1-1** | 曾把「保存到源目录」写成所有文件都进 `files[0]` 的目录 | 每个文件写回**它自己**的目录，不弹框；只有多对一合并才需统一目录 | `A\a.docx`+`B\b.docx` 勾「保存到源目录」→ 生成 `A\a.pdf`、`B\b.pdf`，无目录框；`image.pdf` 合并模式改为 `images.pdf` 进选定目录 | **[已关闭]** `ConvertFeatureModule.cs:183-188,283`（Lead 定案口径） |
| **P1-2** | 处理中只报总数、结束用 `result.Message` | 逐文件 `正在处理第 N 个文件`；结束首段 `处理完毕` | 3 文件跑一遍，状态序列含 `第 1/2/3 个文件` 与 `处理完毕，成功 3 个文件。` | **[部分]** 转换页 `:275-276,302-304` 已达标；`word.replace`/`excel.replace` 仍整批（`ReplaceFeatureModule.cs:149,152-157`）—— **两页 legacy 亦为整批，逐文件属增强而非等价性要求**，故不阻断 |
| **P1-3** | `code.barcode` 无「识别条码」 | 选 1 张图 → 状态区给内容 + 「复制识别内容」 | 用本页「保存 PNG」生成的图回读：Code39 精确相等、QR 断前缀（评估版会追加水印句）；纯白图 → `未识别到条码内容。` | **[已关闭]** `IBarcodeService.cs:25` + `BarcodeFeatureModule.cs:90-94` + `BarcodeServiceTests.cs` 5 例 |
| **P1-4** | 查找/替换挤在一张卡 | 两张卡 `查找内容` / `替换内容` | 快照出现两个标题；新增/移除组后结构不变 | **[已关闭]** `ReplaceFeatureModule.cs:91-97` |
| **P1-5** | 壳层两处直接 `MessageBox` | 改注入 `IDialogService.Info/Error` | G8 0 命中；未注册入口文案不变 | **[已关闭]** `MainWindow.xaml.cs:21,258,336` |
| **P1-6** | 拖放不做后缀过滤 | 拖放与「选择文件」同过滤串；被拒不进列表 | `word.pdf` 拖 `.txt` → 不入列表且提示 `已忽略 1 个不支持的文件。`；`image.compress` 拖 `.png` → 加入 | **[已关闭]** `ConvertFeatureModule.cs:91-95`（另两页同接线） |
| **P1-7** | `refreshPageNumber` 在 pdf/ppt 面板出现但分支不读 | B 方案：`pdf.split`/`ppt.split` 不显示该勾选，`word.split` 保留 | `ppt.split`/`pdf.split` 面板无「拆分后更新页码」，`word.split` 仍有；勾选后 word 拆分首页页码为 1 | **[已关闭]** `FeatureOptionSchemas.cs:317-327,623-627,640-642` |

**一轮回归命令**（P1 全量）：`dotnet build DocumentTools.sln --no-incremental -v q --nologo` → `dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo` → `dotnet run --project tools\FeatureSmoke -v q --nologo` → `dotnet run --project tools\QaFunction`（退出码 0）。

### 3.3 P2（迁移完整性 / legacy 对齐）

| 编号 | 现象（证据） | 期望 | 验收方式（回归命令） | 状态 |
| --- | --- | --- | --- | --- |
| **P2-1** | 条码缺 旋转/尺寸模式/宽/高 | 补 `rotate`/`sizeMode`/`barcodeWidth`/`barcodeHeight` | 旋转 90° 宽高互换；`sizeMode=固定`+宽 300 → 产物宽 300 | **[已关闭]**（产物断言已补）Schema `:407-410`、`BarcodeRenderOptions.cs:25,28,31,34`、`BarcodeService.cs:56`；断言 `tools\QaFunction\Program.Extended.cs:99`（`:1069` 注释、`:1126` 宽高互换 `Require`） |
| **P2-2** | 身份证结果 3 项 | 补第 4 行 `校验码` | 合法号码 → 4 行；错校验码 → 该行指出应为 X | **[已关闭]** `IdCardFeatureModule.cs:44-47` |
| **P2-3** | 关于页只有许可/运行信息 | 补 `捐赠` 与 `软件说明` | 快照出现两卡；点捐赠不静默失败 | **[已关闭]** `AboutFeatureModule.cs:67-72,77-81,327` |
| **P2-4** | twain 只有 2 分区 | 对齐 legacy 4 分区 | 快照可读到 `TWAIN 设置 / 扫描配置 / 数据信息 / 扫描图片预览` | **[已关闭]** `MiscToolModules.cs:102-103,107-108,113-114,122` |
| **P2-5** | DataMatrix 结果树无卡 | 外套 `解析信息` 卡 | 快照可读到 `解析信息` | **[已关闭]** `DataMatrixFeatureModule.cs:73` |
| **P2-6** | name.vertical 缺拼音/双字对齐/替换/计数/导出 | 按 legacy 语义补全 | 选「首字母大写」`张三` → 拼音首字母；导出 TXT 文件存在；计数随输入变化 | **[已关闭]** Core `NameVerticalService.cs:135` + Schema `:525-541` + UI `NameVerticalFeatureModule.cs:154,170,219-225` |
| **P2-7** | image.compress 缺「不压缩」「沿用源后缀」与 BMP 细分 | 补 `noCompress`/`useSourceExtension`/`bmpPixelBits` | 勾「不执行压缩操作」→ 产物与源字节一致；勾「沿用源后缀」→ 后缀随源 | **[已关闭]**（产物断言已补）Schema `:466-493`（`:484,:485,:493`）、`ImageConvertOperation.cs:71,74-75`；格式表另含 TIF/TIFF/GIF `:470-481`；断言 `tools\QaFunction\Program.Extended.cs:100-102`（`:1250/:1277/:1309`） |
| **P2-8** | `ppt.pdf` 备注位置写死 | 升级为 Choice（按实现真实能力给档） | 面板可选三档；选「页底完整」→ 产物备注在页底 | **[已关闭]**（产物断言已补）Schema `:60-65`（`PptNotePositions` 三档）+ `:265`、`OfficeConvertOperation.cs:123,217-237`；断言 `tools\QaFunction\Program.Extended.cs:103` + `FileProbe.cs:263`（第一页 MediaBox 尺寸）；单测已同步为 3 档（`Task13OptionTests.cs:219-233`） |
| **P2-9** | 替换页无「忽略脚注」 | word/excel 的 `匹配选项` 补该勾选 | 含脚注样本中脚注文本不被替换 | **[已关闭（word）/ 不适用（excel）]** Schema `:556`、`ReplaceDocumentOperation.cs:39` |
| **P2-10** | **假参数**：Schema 有 `voteGenDate`，全仓无读取点 | 接线到 `GenerationDate`（Lead 定案：不删键） | 填了日期→产物日期随之变；留空→当天 | **[已关闭]**（接线完成）`FeatureOptionSchemas.cs:592`；`ExamVoteView.cs:73` 定义、`:76` `SchemaDefault("voteGenDate", …)`、`:116` 布局、`:190` `GenerationDate = ExamViewKit.Value(_genDate)`；请求字段 `IVoteReportService.cs:35`；守门 `FeatureOptionSchemaReachabilityTests.cs:41,55`。理由：`voteFontSize`/`voteRowHeight` 同卡有消费点（`:64,:70`），删键会让「三参数只两个生效」退化成「只剩两个」 |

### 3.4 本轮已闭环（勿再当缺口返工）

| 项 | 证据（当前行号） |
| --- | --- |
| 水印全参（对齐/坐标/字体色/图片宽高/横纵间距） | Schema `:329-354` + `PdfWatermarkOperation.cs:63,105,115-127` + `PdfWatermarkService.cs:82,100,127,155,165-182` |
| `html.pdf` 纸张方向 + 导航超时；`paper` legacy 索引 0-10 | Schema `:422-439`（`:433,:434,:438`）+ `HtmlToPdfOperation.cs:35,40,48-49,119` |
| `pdf.compress` 图片压缩版本 + 资源三开关 | Schema `:360-378`（`:365,:373-376`）+ `PdfCompressOperation.cs:36,39-42,94` |
| `ppt.split` `slideSize`/`slideScale`；`pdf.split` `deleteStart`/`deleteEnd` | Schema `:319-327`（`deleteStart`/`deleteEnd` `:649-654`、`slideSize`/`slideScale` `:662-668`）+ `SplitDocumentOperation.cs:67-68,78,97-98,114-133` |
| `image.pdf` 自动优化 / 页面方向 / 四边距 | Schema `:441-454` + `OfficeConvertOperation.cs:297-309` |
| `image.compress` 沿用源后缀 / 不压缩 / JPG-PNG-BMP 细分 | Schema `:456-484` + `ImageConvertOperation.cs:71,74-75` |
| `image.ico` ICO 256 与 `multiSize` | Schema `:500-513` + `ImageConvertOperation.cs:36,44-46` |
| 考评改号 / 物理顺序重建 | `ExamIndexService.cs:93,139,190,284,348,353` + `ExamInsertView.cs:153-154,164` |
| 考评批量项目 / 批量选项（11 组内置选项、中文序号、剪贴板解析） | `ExamBatchInsertTemplates.cs:28-41,47,77,100,121,157,172` + `ExamInsertView.cs:131-132,144-145` |
| 报表结构（`Kind=` 替换 / `OldRaid` 定点替换 / 身份下拉） | `ExamReportService.cs:28,37,75,77,89,173-192` + `ExamReportView.cs:106-119`（报表结构卡）+ `:121-135`（内容替换卡，新数据格式 `:134`）+ `:269`（空值拦截） |
| `twain.debug` 接口注释按实现改写 | `ITwainScanService.cs:22-26` |
| TIF / GIF 输出（自写编码器，SkiaSharp 无 TIFF 编码器、`Encode(Gif)` 返回 null） | Schema `:470-481`（格式表含 TIF/TIFF/GIF）+ `ImageConversionService.cs:231,233` + `src\DocumentTools.Core\Services\Image\TiffEncoder.cs`、`GifEncoder.cs`；体积/时延实测见 `docs\feature-gap.md:164,167` |
| `word.pdf` 删 `removeTempDocx` 假键；`useLocalOffice` 真接 Word COM | `src` grep `removeTempDocx` 0 命中 + Schema `:211-224`；`OfficeConvertService.cs:30 WordToPdf` → `:41` 分支 → `:118 WordToPdfViaLocalOffice` |
| `word.replace` 只认 `regexSub`；`excel.replace` 删行区间不再多删一行 | `ReplaceDocumentOperation.cs:45,47`（legacy 也只由 `UseRegexSub` 驱动（`DocumentTools\Util.cs:5380` 参数、`:5410` `UseSubstitutions = UseRegexSub`；源码注释里的 `DocumentTools\Util.cs:5461` 是旧行号（现指向 Excel 保存行），实为 Excel 保存行））；`DocumentReplaceService.cs:118,129`（`totalRows = end - start + 1`） |
| `word.replace` 替换后字体样式（legacy `UseNewStyle` 死开关重做） | Schema `:566-570`（`replaceUseNewStyle`/`replaceFontName`/`replaceFontSize`/`replaceBold`）；legacy 侧形参未被使用见 `docs\feature-gap.md:187` |
| `exam.package` 合并失败消息按端归因 + 事务回滚说明 | `ExamPackageService.cs:346-351`（`:344` 先 `Rollback()`；`:348-351` 文案含 `{targetRoot}`/`{BaseDatabaseName}`/「事务已回滚」）；UI 守卫 `ExamPackageView.cs:158,178,283`（前置目录与 `Exam_Base.mdb` 检查） |
| `exam.single` 内置测评人码图（001–999） | `ExamSingleView.cs:30,37,69,77`（勾选/提示/区块名/Hint）；端到端 `tools\QaFunction\Program.Extended.cs:126` |
| `P2-1` 条码外观 / `P2-7` 图片编码三键 / `P2-8` 备注导出位置 / `P2-10` 生成日期：产物级断言补齐 | `tools\QaFunction\Program.Extended.cs:99`（条码 `:1069,:1126`）、`:100-102`（`:1250,:1277,:1309`）、`:103` + `FileProbe.cs:263`（MediaBox）、`ExamVoteView.cs:73-79,116,190` |

---

## 4. 经查证「并不缺」（防止被误当缺口返工）

| 项 | 结论 | 依据 |
| --- | --- | --- |
| 47 个功能全部注册、均有图标 | 不缺 | `BuiltInFeatures.cs`（47 项）；`FeatureIcons.cs` 全 Id → `SymbolRegular`，未知 Id 前缀推断；FeatureSmoke 19:59 打印 47/47/47 |
| 侧栏分组折叠 / 搜索强制展开 / 激活自动展开 | 不缺 | `MainWindow.xaml.cs:103-155`（分组）、`:106`（搜索强制展开）、`:185-217`（激活展开高亮） |
| 起始页与隐藏功能 | 不缺 | `AppSettings.cs:13` + `MainWindow.xaml.cs:46-58` |
| 网格线/行高可配 | 不缺 | `ConvertFeatureModule.cs:79-81`、`BorderedFileList.cs:87-92`（夹取 18–64）、`:341`（行高 +8） |
| 「添加文件≠处理」、空列表不弹目录 | 不缺且优于 legacy | `ConvertFeatureModule.cs:149`；legacy 空列表静默无操作（`DocumentTools\UI.cs:251`） |
| 合并顺序=列表顺序 + 保存文件对话框 + `outputFile` | 不缺 | `ConvertFeatureModule.cs:160`、`MergeDocumentOperation.cs:30-44` |
| 处理后询问打开目录（可关） | 不缺 | `ConvertFeatureModule.cs:412`（跨目录回写场景按 `:307,:321` 不问） |
| `openPdfOptions` 不缺；`removeTempDocx` 已按实现删键 | 不缺（删键正确） | `ConvertFeatureModule.cs:394-414`（逐个打开产出的 PDF）；`src` 层 grep `removeTempDocx` 0 命中，`OfficeConvertService.WordToPdf`（`:20`）与 `WordToPdfViaLocalOffice`（`:110`）都不生成 temp docx 目录 → 保留该勾选等于假参数，删键优于留假控件 |
| `pdf.watermark.remove`、`excel.word`、`epub.word`、`epub.pdf` 只有 `saveSource` | 不缺（与 legacy 一致） | Schema `:545-548`、`:595-598`（默认 `_`）；legacy 移除水印页只有列表+3 按钮（Designer `4828-4904`） |
| 合并页没有 `saveSource` | 不缺（legacy 合并走 SaveFile） | Schema `:543` = `[]` |
| `image.ico` 多尺寸 | 增强，非缺口 | legacy 单尺寸（`Util.cs:1774-1788`、`DocumentTools\UI.cs:448-450`），WPF 多一档 `multiSize` |
| 「删除PDF页面」（legacy 死控件） | 不列缺口 | Designer 有 `SplitePdf_DeletePdfPages`（`3631-3673`）但 `DocumentTools\BodyForm.cs` 无引用、无 Click；WPF 现以 `deleteStart`+`deleteEnd` 支持 |
| Excel→PDF 8 个打印参数、WORD→PDF 7 个转换参数、PDF 版本转换 | 不缺 | Schema `:200-215,:231-243,:346-358` |
| 考评 7 页 / 工具页无假文件列表 | 不缺 | 7 页全走 `ExamViewKit`；`BorderedFileList` 仅 3 处实例化（转换页、替换页、DataMatrix 页），三者 legacy 本就是真列表页 |
| `system.settings` 不含窗体大小/锚点 | 不缺（故意） | 页面自带说明 + `docs\feature-ux-spec.md` §4 |
| App 不直调 Aspose / Core 不含 WPF 与对话框 | 不缺 | G6/G7 实测 0 命中；Core 目标 `net8.0-windows` 仅因 OleDb/Registry/WMI/TWAIN/System.Drawing |

---

## 5. 未验证 / 残余风险（不参与验收，需 Lead 决定是否补测）

1. **无 ACE 环境会跳过考评 Access 用例**：`tools\ExamFixture\Program.cs:99` 判定 ACE 不可用、`:102` `return 2`，`[AceFact]` 用例随之跳过——「全绿」不代表考评写库路径被执行。
2. **`system.*` 写操作未在隔离机验证**：暂停更新天数 / 快捷方式小箭头 / 粘滞键 / 静态 IP / DHCP 会改真实系统设置；源码有管理员校验与 WMI→netsh 回退，但没有一次性隔离机上的回滚验证。
3. **评估版限制**：`assets\licenses` 无 `.lic`，Aspose 评估版每次转换只处理前 4 个元素并在产物中插水印句（条码解码结果尾部也会被追加水印文本，故 QR 用例只断前缀）。任何「产物字节级一致」断言不可复现。
4. **GUI 人工点测未做**：47 页「能建能排」有 FeatureSmoke、视觉树有 UiSnapshot、端到端有 QaFunction，但没有真人逐页点测（拖放、右键菜单、取消路径、粘贴路径）。
5. **`ExamPackageService.Reset` 只有合成库覆盖**：只在 `ExamFixture` 生成的 `.mdb` 上验证；真实考评库的历史表结构差异（自增列/附加索引）未覆盖。`P1-2` 替换页整批行为同理只有源码与快照证据，无逐文件状态断言。
6. **TIF/GIF 自研编码器有体积税**：A4 300dpi 实测白页 LZW 0.17 MB（0.7%）、表格文字 0.38 MB（1.5%）、彩色噪声 25.55 MB（102.6%，比未压缩更大），编码 0.44/0.50/1.25 s（`docs\feature-gap.md:164`）。使用口径：扫描件/二值文档选 TIF，彩色照片选 JPEG。**不要对该输出写「体积必须变小」类断言**。
7. **并发窗口内门槛会瞬态变红**：本轮 G1 20:46–20:49 连续撞 `ExamInsertView.cs` CS0103 `WeightOr`、`ExamVoteView.cs` CS0103 `ParseDouble`、`SingleDocumentNamingTests.cs` CS1674；G2 20:51 曾 5 红；UiSnapshot 曾报 `medium=1`（system.settings 空文本按钮，单页复跑 0 issue、全量复跑 `medium=0`）。判红绿须同一时刻连跑 G1→G2→G3。
8. **实体扫描仪未实机验证**：`twain.debug` 走 NTwain 真驱动（`ITwainScanService.cs:22-26` 注释写明无设备时抛 `NotSupportedException`、消息取 `UnavailableReason`），但本机无扫描仪，扫描落盘与多页/双面/跳过空白页选项只在无设备分支上验证过。

---

## 6. 责任矩阵与执行顺序（按当前状态）

| 顺序 | 事项 | 角色 / 归属 | 状态 |
| --- | --- | --- | --- |
| 1 | G1–G8 全绿（含 G5 乱码、G8 MessageBox） | 全员，提交前自检 | 常态化闸门 |
| 2 | P1-1、P1-2（转换页） | UI + 开发回调 | **已完成** |
| 3 | P1-4、P1-5、P1-6、P2-2、P2-4、P2-5 | UI-A / UI-B | **已完成** |
| 4 | P1-3 条码识别（Core + 面板） | Lead + UI | **已完成** |
| 5 | P1-7 键裁剪（B 方案） | 开发 2 | **已完成** |
| 6 | P2-1、P2-7、P2-8、P2-9（键与新字段） | 开发 2 | **已完成** |
| 7 | P2-8 映射、P2-7 服务参数 | 开发 3 | **已完成** |
| 8 | P2-3 关于页、P2-6 姓名纵排面板 | UI-A / UI-B | **已完成** |
| 9 | 考评改号 / 批量模板 / 报表结构 | 开发 1 + UI | **已完成** |
| 10 | 端到端与快照回归（`QaFunction` / `UiSnapshot` / `FeatureSmoke` / `dotnet test`） | 测试 1 / 测试 2 | **已完成**：定稿闸门 PASS（G0，366 例 0 失败；QaFunction 16+30+4 全 PASS；UiSnapshot 47 页 `fatal/high/medium=0`） |
| 11 | §5 残余风险的补测决策 | Lead | 收口：4 项外部条件（VFSoft 真实库 / 隔离机 / 实体扫描仪 / Aspose 授权）报「未验证」，其余按 §5 逐条接受 |
| 12 | `README.md` / `CONTRIBUTING.md` 对齐现实 + 本文件与账本再同步 | PM（task-34） | **已完成**（22:0x 定稿同步） |
