# 会话交接（2026-11-06 15:0x 收口复核）

本文件用于在**新会话**里继续工作。当前工作树是脏的（大量未提交，含新增 `src/`、`tests/`、`docs/`），
**不要** `git reset --hard` / `checkout --` / `clean` / `stash`，不要删除非自建文件，不提交（除非用户明确要求）。

## 1. 当前阶段：dev 对齐收口，四层验收本轮复核全绿，待用户验收

`feat/wpfui-migration`：把 WinForms 原版迁移成 WPF-UI/Fluent 版，业务与 UI 以本地 `dev` 分支
（tip `ce4cc88`）为行为基准，要求「一毛一样」。47 个 FeatureId 全部闭合，无占位/半成品标记。

**本轮（2026-11-06 15:0x）亲自复核的实测数字（同一时段连跑）**：

| 层 | 命令 | 结果 |
| --- | --- | --- |
| 构建 | `dotnet build DocumentTools.sln -v m` | 0 警告 / 0 错误 |
| 单测 | `dotnet test tests\DocumentTools.Tests\...csproj` | 通过 521 / 失败 0 / 跳过 3（Word opt-in 白名单）|
| 冒烟 | `dotnet run --project tools\FeatureSmoke` | 功能 47/47/47 + 启动落点 95 项失败 0 |
| 快照 | `dotnet run --project tools\UiSnapshot -- --out <临时目录>` | hidpi=16、interact 4/4、无 fatal/high/medium |
| 端到端 | `dotnet run --project tools\QaFunction` | 必测 16/16 + 扩展 42/42，退出码 0 |
| 兼容 | `dotnet run --project tools\QaCompat -- --all --report <临时> --work <临时>` | PASS 14 / FAIL 0 / 无法验证 2，退出码 0 |

> QaCompat 末尾会打印 3 条「疑似直出 Aspose 英文原文」的源码提示
> （`MergeDocumentOperation.cs:46-48`、`ImageConvertOperation.cs:55-57`、`SplitDocumentOperation.cs:50-52`）。
> **已核实为启发式假阳性**：这三个 `catch` 都调用了 `OperationErrors.Translate`，且其自身 PASS 输出已是
> 中文包装（如「合并 PDF 失败：当前为 Aspose 评估模式…请把文件拆分后再试。」）。FAIL=0 即证。

## 2. 已完成（勿重做）

- 47 功能全部闭合（`src\DocumentTools.Core\Features\BuiltInFeatures.cs` ↔ `src\DocumentTools.App\Features\Modules\`）。
- 考评 7 页全接 Core 真实现（MSS3/插入/报表/数据包含合并/圆圈修复/vote/single）。
- OCR 独立 `DocumentTools.Ocr` 可选组件（宿主按需从 `ocr\` 加载；缺失时页面禁用并提示，主程序体积不变）。
- 解耦审计：`Exam/` 之外（`Tools/` `SystemTools/` `Shell/` `Ui/`）本轮按 `docs\decoupling-audit.md` §0 口径普查完毕，
  无业务知识外泄点（详见该文件 §6.1）。
- 授权口径：本构建**不带** Aspose 授权，评估限制（集合 >4、产物水印句、条码解码尾部追加）为准；
  唯一权威说明见 `docs\feature-gap.md` §9 的 blockquote。

## 3. 仅剩的外部条件项（非缺陷，无法在本机闭环）

1. `exam.*` 未在**真实 VFSoft 库**验证（现证据来自 `ExamFixture` 合成库；本机 ACE 12.0 可用）。
2. `system.*` 写操作（暂停更新/箭头/粘滞键/键盘锁/UAC/静态 IP/DHCP）需**隔离机**回滚验证。
3. 实体**扫描仪**未联调（`twain.debug` 与考评圆圈页取图只验证了「无设备」分支）。
4. `docs\qa-clickthrough.md` 的**真人逐页点测**尚未执行（拖放/右键/取消/粘贴/Tab 走查）。

## 4. 命令与坑

- 开工：`dotnet build DocumentTools.sln -v q --nologo`（要 0 警告 0 错误）。
- 单测：`dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v q --nologo`（整集串行，约 60–70s）。
- 冒烟：`dotnet run --project tools\FeatureSmoke -v q --nologo`。
- 快照：`dotnet run --project tools\UiSnapshot -- --out <临时目录>`（全量默认写 `docs\qa-screens\`；子集只写 `partial-*.csv`，
  **不要**用子集覆盖 `_index.csv`）。
- 端到端：`dotnet run --project tools\QaFunction`（退出码 = 失败项数）。
- 兼容：`dotnet run --project tools\QaCompat -- --all`（**注意** `-v q --nologo` 不能放 `--` 之后，会被当作应用参数报「未知参数」）。
- 发布闸门（六门串跑）：`& .\tools\gate.ps1 -MaxSkips 3`（开跑前检测 `DocumentTools.exe`/`testhost` 占用并 ABORT——正确行为）。
- 并发坑：多线并行偶发 `MSB3027/MSB3021/CS0006` 文件占用，等 15–60 秒重试。
- 编码坑：含中文的 `.cs`/`.md` 只用 `write`/`edit`，**不要** `Get-Content`+`Set-Content`（会写坏中文）；`.ps1` 必须 UTF-8 带 BOM。

## 5. 权威文档优先级

`architecture.md` > `feature-ux-spec.md` > `feature-gap.md`（§9 是「有意偏离 dev」唯一权威清单）。
状态账本 `backlog-next.md` / `closeout-ledger.md` 可对账但不是规格；索引见 `docs\INDEX.md`。
