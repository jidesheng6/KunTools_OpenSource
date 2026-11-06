# Embedded assets（原 WinForms Resources 释放项）

本目录文件会复制到应用程序输出目录的 `embedded/`。

原项目通过 `Util.GetEmbedResourceObj` 从 `DocumentTools/Properties/Resources.resx` 读取。  
WPF 版改为**旁路文件**，避免把破解许可等敏感/侵权资源打进开源仓库。

## 需要的文件

| 键（代码） | 文件名 | 来源（legacy Resources 名 / 文件） |
|------------|--------|--------------------------------------|
| `registry` | `registry.reg` | `注册` → `DocumentTools/Resources/注册.reg` |
| `kmsaio` | `kmsaio.cmd` | `KMSAIO` → `Resources/KMSAIO.cmd` |
| `renamer` | `ReNamerPro73.exe` | `ReNamerPro73` → `Resources/ReNamerPro73.exe` |
| `renamer.rules` | `renamer-rules.rnp` | `移除副本规则_部分自定义规则` |
| `link.netfx35` | `link-netfx35.txt` | `Link_*NetFx35*`（一行 URL；缺省则打开可选功能） |
| `vote.report.doc` | `VoteReportDoc.doc` | 选举报告单 Word 模板（可提交） |

> Resources.resx 使用 `ResXFileRef`，导出工具会解析相对路径并复制真实文件。  
> `VoteReportDoc.doc` 可随仓库分发；KMS/ReNamer 等二进制勿提交。

## 从旧工程导出

在仓库根目录执行（需本机有 .NET SDK）：

```powershell
dotnet run --project tools/ExportEmbeddedAssets -- `
  --resx DocumentTools/Properties/Resources.resx `
  --out assets/embedded
```

导出的二进制请**不要提交到公共仓库**（可加入 `.gitignore`）。
