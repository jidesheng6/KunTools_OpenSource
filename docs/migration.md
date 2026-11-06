# Migration notes

Branch `feat/wpfui-migration` starts from `dev` and rebuilds the product shell on **WPF-UI** (WinUI3 / Fluent look via left-nav + `FluentWindow`).

## Status

| Area | Status |
|------|--------|
| Open-source folder layout | Done |
| Fluent left-nav + ContentHost | Done |
| Feature module + DI + Aspose.Hook | Done |
| UX spec `docs/feature-ux-spec.md` | Done |
| Gap matrix `docs/feature-gap.md` | Done |
| Convert bottom-bar layout + partitions | Done |
| word/pdf/excel main Options → Core | Done |
| Watermark/split/compress option key align | Partial |
| System settings (no form size/anchor) + mojibake fix | Done |
| Image compress / WEBP / ICO Core | Next |
| Replace / DataMatrix / Exam / Twain depth | Next |

## Encoding

Chinese UI strings in `.cs` must stay **UTF-8**. If `???` appears in Settings, rewrite the file with UTF-8 BOM (see `docs/feature-gap.md` §7).

## UI rule

Only legacy multi-file ListView tabs get `BorderedFileList`. No fake file shells on barcode / idcard / exam / system tools / etc.

## Licensing

Do not publish Aspose.Hook or pirate licenses. Pin Aspose package versions.