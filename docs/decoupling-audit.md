# 视图与业务解耦审计（Exam 模块）

本文件是 `docs/architecture.md`「视图里没有业务知识（路径布局 / 分隔符 / 命名规则 / SQL / 编码）」这条自检的判例库：
每条判例都能独立看懂——「哪里把业务知识写进了视图」「这条知识归谁」「怎么验证」。新增代码按同款口径自查。

## 0. 范围与方法

范围：`src/DocumentTools.App/Features/Modules/Exam/` 全部视图（9 个文件）+ 它们调用的 `Core/Services/Exam/`、`Core/Common/`。

方法：在视图目录里逐个判定这些 token 的归属——`Path.` / `File.` / `Directory.` / `Split(` / `Regex` / `StartsWith(` / `EndsWith(` / `.db` / `.mdb` / `DateTime.Now.ToString(` / `Encoding.`——另外补查 `Contains('|')`、写库字面值、枚举 index 映射、哨兵字符串。

判定口径（沿用 architecture.md「考评页的例外与下限」）：

- **业务知识**＝「是什么」：哪些文件算考评库（`Exam_Base.mdb`）、选项串的分隔符集合、写进 `Vote_EssInfo.SelName` 的字面值、`所有项目` 哨兵、命名的后缀 `_fixed` / `_r90`、行/列模式与小数位档位到枚举的映射。→ 一律搬进 Core。
- **纯 UI**＝「怎么摆」：控件种类、尺寸、对话框过滤器字符串、把消息显示到状态行还是弹窗、缩放档位与旋转角度按钮。→ 留在视图。

表里「改前」行号＝发现耦合时磁盘上的行号（Lead 给的判例号与实测行号一致或近似，以实测为准）；「现」行号＝本次改造后的调用点，便于 `rg` 复核。

## 1. 判例表（Lead 指定的四处）

| # | 判例（改前 → 现） | 知识类别 | 移到哪 | 回归测试 |
| --- | --- | --- | --- | --- |
| 1 | `ExamPackageView.cs:96,101,185,227,233,362` 视图自拼 `Path.Combine(dir, "Exam_Base.mdb")` 判「是不是数据包」→ 现 `ExamPackageView.cs:85,101,110,193,235,241,370` 调 `_packages.IsPackageDirectory(...)` / `Describe(...)` | 数据包布局（哪种目录算考评数据包） | `Core/Services/Exam/ExamPackageService.cs` → `IExamPackageService.IsPackageDirectory/Describe`。放这儿的理由：`Exam_Base.mdb` 本来就是该服务的私有常量，视图拼路径等于把服务的文件布局抄了一遍，改常量就会漏一处 | `ExamDecouplingRulesTests.PackageDirectory_IsRecognisedByTheBaseDatabase` |
| 2 | `ExamPackageView.cs:107,221`（现 `115,229`）视图内 `Path.GetFullPath(a) == Path.GetFullPath(b)` 判同目录 | 路径规范化规则（大小写、尾分隔符） | `Core/Common/PathGuard.cs` → `PathGuard.SamePath` | `PathGuardTests.SamePath_ToleratesCaseTrailingSeparatorAndDots` |
| 3 | `ExamInsertView.cs:629-631` 私有 `SplitNames` 用 `[',', '，', ';', '；', '|']` 解析选项串 → 现 `ExamInsertView.cs:574` `Selectors = ExamInsertRules.ParseSelectorList(row.SelectorText)` | 输入格式（兼容哪些分隔符） | `Core/Services/Exam/ExamInsertModels.cs` → `ExamInsertRules.ParseSelectorList`（纯函数）。放这儿的理由：同一串在插入时解析、在体检/回读时还要按同一规则解释，规则必须和写库代码住一起 | `ExamDecouplingRulesTests.ParseSelectorList_AcceptsAllLegacySeparators` / `_TrimsAndDropsEmptyEntries` / `_Blank_ReturnsEmpty` |
| 4 | `ExamCircleView.cs:647-661` 私有 `UniqueSibling`（`_fixed` 起、冲突加 `_2`）→ 现 `ExamCircleView.cs:574,612` 调 `ExamCircleNaming.UniquePath` | 文件命名规则（避撞后缀与递增） | `Core/Common/UniqueFileName.cs` → `UniqueFileName.For`；`Core/Services/Exam/ExamCircleNaming.cs` → 后缀口径。放这儿的理由：避撞是通用文件名问题（考评模块之外也会用），而具体后缀属于考评图像业务 | `PathGuardTests.UniqueFileName_Collision_IncrementsFromOne` / `_CollisionIsCaseInsensitive` / `_NoCollision_AppendsSuffixAndKeepsExtension` / `_KeepsMultiPartExtensionAndRotationSuffixShape` |
| 5 | `ExamInsertView.cs:211,226,304,456,504,545` → 现 `210,225,303,455,503,544`；`ExamMss3View.cs:110,122,127,153,181,187,227,254` → 现 `107,119,124,150,178,184,224,251`；`ExamReportView.cs:233,248,310,337,369,572` → 现 `224,239,301,328,361,564`；`ExamCircleView.cs:243,473,509,561,592` → 现 `244,474,510,559,590`：每处 `File.Exists(...)` + 各写一句中文提示（「数据库不存在，请重新选择。」「请先选择考评数据库。」「图片不存在：…」） | 前置条件口径（什么算「已就绪」）与提示文案重复 20 遍 | `Core/Common/PathGuard.cs` → `MissingFileReason` / `MissingFolderReason` / `RequireFile` / `RequireFolder`。放这儿的理由：视图保留「显示到状态行还是弹窗」的自由，但「缺文件算缺什么」只该有一个判定 | `PathGuardTests.MissingFileReason_Blank_ReportsNotChosen` / `_Missing_ReportsNotExist_AndReadyReturnsNull` / `MissingFolderReason_DistinguishesBlankAndMissing` / `RequireFile_*` / `RequireFolder_Missing_ThrowsDirectoryNotFound` |

## 2. 本轮新发现的同类判例

Lead 的清单只覆盖「路径 / 分隔符 / 命名 / 存在性」，按同一口径扫下去还有下面这些「视图替 Core 决定是什么」的地方：

| # | 判例（改前 → 现） | 知识类别 | 移到哪 | 回归测试 |
| --- | --- | --- | --- | --- |
| 6 | `ExamInsertView.cs:21` 私有 `string[] TypeModes = ["人员纵排","人员纵排2","人员横排","单人标识"]` + `exam.TypeMode` 回填时视图内 `IndexOf` → 现 `ExamInsertView.cs:89,101` | 写库字面值（`Exam_*.TypeMode` 的合法取值域） | `Core/Services/Exam/ExamInsertModels.cs` → `ExamInsertRules.TypeModes` / `TypeModeIndex`；`ExamInsertRequest.TypeMode` 默认值改为引用 `PeopleVerticalTypeMode` | `TypeModes_AreTheLegacyWriteValues_InDropDownOrder` / `TypeModeIndex_MapsStoredValueToDropDownIndex` |
| 7 | `ExamMss3View.cs:42` `new[]{"方框","圆框"}`、`46-50` `new PageChoice(0,"第1页")…`、`205` 兜底 `?? "方框"` → 现 `42,45` + `?? Mss3VoteService.SquareTypeSquare` | 写库字面值（`Vote_EssInfo.SelName`）与页序 0 基约定 | `Core/Services/Exam/Mss3VoteService.cs` → `SquareTypes` / `PageChoices` / `SquareTypeSquare` | `Mss3_SquareTypes_ListsTheTwoAcceptedValues` / `Mss3_PageChoices_AreZeroBasedPageIndexes` |
| 8 | `ExamReportView.cs:31,414` 视图硬写 `"所有项目"` → 现 `30,406` | 哨兵值（Core 用它表示「不按项目拆分」） | `Core/Services/Exam/ExamReportService.cs` → 复用已有 `AllItemsName` | `AllItemsName_IsTheSentinelCoreRecognises` |
| 9 | `ExamReportView.cs:104` `choice.Value.Contains('|')` 判定「一个身份还是多个身份」 → 现 `103` | 复合值的分隔符约定 | `Core/Services/Exam/ExamReportService.cs` → `IsMultiRaidValue` | `IsMultiRaidValue_DetectsPipeJoinedIdentities` |
| 10 | `ExamReportView.cs:171` `new[]{"行模式","列模式"}`、`175-183` 六档小数位文案、`489` `(ExamReportNumberFormat)Math.Max(0, _bulkDecimals.SelectedIndex)` → 现 `170,475,481` | 下拉 index → 枚举的映射（顺序即合同） | `Core/Services/Exam/ExamReportService.cs` → `ModeOptions` / `IsRowMode` / `NumberFormatOptions` / `NumberFormatAt` | `ModeOptions_RowModeFirst_AndIsRowModeFollowsIt` / `NumberFormatAt_MapsDropDownIndexToEnum_AndClampsOutOfRange` |
| 11 | `ExamCircleView.cs:521-524` `Path.GetExtension` + `"_fixed"` 拼另存名 → 现 `522` | 命名规则（修复件后缀） | `Core/Services/Exam/ExamCircleNaming.cs` → `FixedFileName` / `FixedSuffix` / `RotationSuffix` / `AutoRotationSuffix` | `ExamCircleNaming_FixedFileName_KeepsStemAndExtension` / `ExamCircleNaming_Suffixes_MatchTheLegacySaveNames` |
| 12 | `ExamSingleView.cs:182-184` `Split('@', …)` 解析多个占位名 → 现 `181` | 输入格式（`@` 分隔的 legacy 兼容） | `Core/Services/Exam/SingleDocumentRequest.cs` → `SingleDocumentNaming.ParseAltTexts` / `AltTextSeparator` | `ParseAltTexts_SplitsOnAtSign_AndTrims` |
| 13 | `ExamVoteView.cs:150-153` `Split(['\r','\n'], …)` 每行一个候选人；`191-194` 四个列名默认值 `姓名/同意/不同意/弃权`；`164` 默认文件名 `选举结果报告单.docx` → 现 `150,188,161` | 输入格式 + 模板表头 + 默认产物名 | `Core/Services/Exam/IVoteReportService.cs` → `VoteReportRules.SplitCandidateNames` / `Default*Column` / `DefaultOutputFileName`；`VoteReportRequest` 四个列属性默认值改为引用常量 | `SplitCandidateNames_OnePerLine_DropsBlankLines` / `VoteReport_DefaultColumnsAndFileName_MatchTheTemplateHeaders` |
| 14 | `ExamMss3View.cs:291-303` 与 `ExamCircleView.cs:684-696` 各写一份私有 `OpenFolder`；4 个视图各自 `private const string AccessFilter = "Access 数据库|*.mdb;*.accdb|所有文件|*.*"`；`ExamVoteView.cs:207`、`ExamSingleView.cs:217` 各写 `Process.Start(new ProcessStartInfo{ UseShellExecute = true })` | 壳层动作（打开目录/文件）与对话框过滤器——属**纯 UI**，但重复 4 份就是缺陷 | `ExamViewKit.cs:22` → `AccessDatabaseFilter`；`114/129/133` → `OpenFolder` / `RevealFile` / `OpenFile` | 由 `FeatureSmoke` 47/47 覆盖（视图构造函数改动） |
| 15 | `ExamInsertView.cs:184` 重排表号提示里硬编码「10 张」→ 现 `182` | 表清单长度（改一处就撒谎） | 直接引用 `ExamIndexService.ExamIdBoundTables.Length` | 由 `FeatureSmoke` 与既有 ExamIndex 测试覆盖 |

## 3. 本轮新增 / 修改的 Core API

```csharp
// Core/Common/PathGuard.cs
public static void   RequireExistingDirectory(string? path, string paramName);
public static string RequireFile(string? path, string label = "文件");                       // 返回绝对路径
public static string RequireFolder(string? path, string label = "目录");                     // 返回绝对路径
public static string? MissingFileReason(string? path, string label = "文件");                // null = 就绪
public static string? MissingFolderReason(string? path, string label = "目录");
public static bool    SamePath(string? left, string? right);                                  // 大小写不敏感、容忍尾分隔符

// Core/Common/UniqueFileName.cs（新文件）
public static string For(string path, string suffix);   // stem+suffix+ext，冲突则 _1/_2…，扩展名保留、大小写不敏感

// Core/Services/Exam/ExamPackageService.cs — IExamPackageService
bool   IsPackageDirectory(string directory);
string Describe(string directory);                       // 未选择 / 不存在 / 不是数据包 / Inspect().Summary（不带句号）

// Core/Services/Exam/ExamInsertModels.cs — ExamInsertRules
public const string PeopleVerticalTypeMode = "人员纵排";
public const string PeopleVertical2TypeMode = "人员纵排2";
public const string PeopleHorizontalTypeMode = "人员横排";
public const string SingleMarkTypeMode = "单人标识";
public static IReadOnlyList<string> TypeModes { get; }
public static int TypeModeIndex(string? typeMode);                        // 未知 → -1
public static IReadOnlyList<string> ParseSelectorList(string? text);

// Core/Services/Exam/Mss3VoteService.cs
public const string SquareTypeSquare = "方框";
public const string SquareTypeCircle = "圆框";
public static IReadOnlyList<string> SquareTypes { get; }
public static IReadOnlyList<(int Index, string Label)> PageChoices { get; }  // (0,"第1页"), (1,"第2页")

// Core/Services/Exam/ExamReportService.cs
public static IReadOnlyList<string> ModeOptions { get; }                                  // 行模式 / 列模式
public static bool IsRowMode(int optionIndex);
public static IReadOnlyList<(string Label, ExamReportNumberFormat Format)> NumberFormatOptions { get; }
public static ExamReportNumberFormat NumberFormatAt(int optionIndex);                     // Clamp 到合法区间
public static bool IsMultiRaidValue(string? raid);                                        // 含 '|'

// Core/Services/Exam/ExamCircleNaming.cs（新文件）
public const string FixedSuffix = "_fixed";
public static string RotationSuffix(int degrees);        // "_r90"
public static string AutoRotationSuffix(int degrees);    // "_auto90"
public static string FixedFileName(string sourcePath);
public static string UniquePath(string sourcePath, string suffix);

// Core/Services/Exam/IVoteReportService.cs — VoteReportRules
public static IReadOnlyList<string> SplitCandidateNames(string? text);
public const string DefaultNameColumn = "姓名";
public const string DefaultAgreeColumn = "同意";
public const string DefaultUnAgreeColumn = "不同意";
public const string DefaultAbstainColumn = "弃权";
public const string DefaultOutputFileName = "选举结果报告单.docx";

// Core/Services/Exam/SingleDocumentRequest.cs — SingleDocumentNaming
public const char AltTextSeparator = '@';
public static IReadOnlyList<string> ParseAltTexts(string? text);
```

## 4. 未修项与理由

| 位置 | 内容 | 为什么保留 |
| --- | --- | --- |
| `ExamPackageView.cs:14`（XML 注释）、`:40`（页面副标题） | 含 `Exam_Base.mdb` / `*.exam` / `SelfReport` | 面向用户的说明文字（告诉操作者这套数据包里有什么），不是判定逻辑；判定已全部走 `IsPackageDirectory` / `Describe` |
| `ExamReportView.cs:64` | 副标题含「所有项目」 | 同上，UI 文案；默认值与判定已引用 `ExamReportService.AllItemsName` |
| `ExamInsertView.cs` 的 `InsertItemRow.SelectorText = "同意,不同意,弃权"`、`CreateSample()`（「德能勤绩」「优秀,良好,合格,不合格」） | 示例种子数据 | 这是演示数据而非解析规则：它不参与任何分支判断，写死反而符合「示例」语义 |
| `ExamCircleView.cs` 的缩放档位（0.5/0.75/1/1.5/2/3 倍）与旋转角（90/180/270）下拉 | index → 数值映射 | 纯呈现：数值只喂给 `Image.RenderTransform` 与 `Rotate(degrees)` 参数，不影响任何业务结果；真正属于业务的「避撞命名」已抽到 `ExamCircleNaming` |
| 各视图的 `SaveFile` / `PickFile` 过滤器（`图片|*.png;…`、`Word 文档|*.docx`） | 对话框过滤器 | UI 关注点；其中「哪些扩展名算 Access 库」这一类已被收敛为 `ExamViewKit.AccessDatabaseFilter` 单点 |
| `ExamReportView.cs` 的 `Clipboard.SetText`、`ExamCircleView.cs:256` 的 `new Uri(Path.GetFullPath(path))` | 剪贴板 / WPF 位图源 | 纯壳层交互与控件前置条件，无业务语义 |
| `src/DocumentTools.App/Features/Modules/Convert/ConvertFeatureModule.cs:404` | 同类「视图内 `File.Exists` + 自写提示」 | **已修（2026-11-06）**：`EndsWith(".pdf") && File.Exists(f)` 改为 `PathGuard.MissingFileReason(f, "输出文件") is null`，与判例 5 同款写法 |

## 5. 复验

```powershell
dotnet build DocumentTools.sln -v q --nologo            # 0 警告 0 错误
dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo   # 失败 0
dotnet run --project tools\FeatureSmoke                # 47/47
Get-ChildItem src -Recurse -Filter *.cs | Where-Object FullName -notmatch '\\(bin|obj)\\' | Select-String -Pattern '锛|銆|鐨|鏂'
```

改造后视图目录里仅剩两处存在性判断，均在 `ExamViewKit` 的壳层动作里（`OpenFolder` 的 `Directory.Exists`、`OpenFile` 的 `File.Exists`），属「壳层动作的前置守卫」，不是业务判定：

```powershell
rg -n "File\.Exists|Directory\.Exists|Path\.Combine|Path\.GetExtension|\.Split\(" src\DocumentTools.App\Features\Modules\Exam
```

## 6. 转交上游的待办

- ~~`src/DocumentTools.App/Features/Modules/Convert/ConvertFeatureModule.cs:404`~~ **已闭环（2026-11-06）**：改为 `PathGuard.MissingFileReason(f, "输出文件") is null`，由 `convert-edge-dev` 在 task-38 后续同类问题里一并处理。

### 6.1 其他模块普查（2026-11-06 收口，`Tools/` `SystemTools/` `Shell/` `Ui/`）

口径同第 0 节。按第 0 节的 token 全量扫（`Path.` / `File.` / `Directory.` / `Split(` / `Regex` / `StartsWith(` / `EndsWith(` / `.db` / `.mdb` / `.accdb` / `SELECT` / `INSERT` / `DateTime.Now.ToString(` / `Encoding.` / 硬编码后缀），结果如下：

| 目录 | 命中 | 判定 |
| --- | --- | --- |
| `Features/Modules/Tools/` | 0 | 干净：条码/DataMatrix/替换/身份证/姓名竖排/Twain/杂项均只收集键值、按 Service 返回值渲染，无路径布局/分隔符/SQL/命名规则 |
| `Features/Modules/SystemTools/` | 2 | 均非业务：① `SystemFeatureModules.cs:267` `Reason()` 去掉「字段名：」展示前缀（消息来自 Core `SettingsFieldStatus.Text`，属**展示格式**）；② `:639-640` `OpenStoreFile()` 建目录 + 文件不存在先 `Save()` 再 `Process.Start` 打开配置文件（属**壳层打开动作**，用的是 `AppSettings.StorePath` / `IAppSettings.Save` 这类 Core API，未复制业务语义） |
| `Shell/` | `CrashLog.cs:21,24,60,63,118-122` | 崩溃日志路径与 `Encoding.UTF8` 落盘，属**基础设施**（日志），不是文档/考评业务知识 |
| `Ui/` | 0 | 干净 |

结论：`Exam/` 之外无业务知识外泄点，无需新增 Core API。回归仍以 `dotnet test` + `FeatureSmoke` 为准（本轮 521 通过 / 0 失败 / 3 跳过、47/47 + 落点 95/0）。
