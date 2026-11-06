# Icons

导航 / 首页 / 标题栏使用 **WPF-UI Fluent `SymbolIcon`**（见 `DocumentTools.App.Ui.FeatureIcons`），**不再复制** WinForms `Resources/*.png`。

仅保留应用壳图标：

| 文件 | 用途 |
|------|------|
| `app.ico`（可选） | 可执行文件 / 任务栏图标；通过 csproj `ApplicationIcon` 引用 |

若需要自定义品牌图标，把 `app.ico` 放本目录即可；功能列表图标改 `FeatureIcons.cs` 映射，勿再塞 PNG 资源。
