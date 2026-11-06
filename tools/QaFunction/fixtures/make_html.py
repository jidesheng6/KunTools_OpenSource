"""Generate the HTML fixture for the function QA harness.

Run with the bundled Python runtime:
  python make_html.py <testdata-root>

Produces:
  html/sample.html  local page with Chinese text, a table and inline CSS
"""

import sys
from pathlib import Path

HTML = """<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="utf-8" />
<title>DocumentTools QA 夹具</title>
<style>
  body { font-family: "Microsoft YaHei", "Segoe UI", sans-serif; margin: 32px; color: #1f2933; }
  h1 { font-size: 26px; color: #1f4e79; border-bottom: 3px solid #1f4e79; padding-bottom: 6px; }
  table { border-collapse: collapse; width: 100%; margin-top: 16px; }
  th, td { border: 1px solid #8a94a6; padding: 8px 10px; text-align: left; }
  th { background: #dfe8f3; }
  .note { background: #fff8e1; border-left: 4px solid #f0b400; padding: 10px 14px; margin-top: 18px; }
</style>
</head>
<body>
  <h1>HTML 转 PDF 夹具</h1>
  <p>本页由 <code>tools/QaFunction/fixtures/make_html.py</code> 生成，用于验证 HtmlToPdfService 的本地文件渲染。</p>
  <table>
    <thead>
      <tr><th>功能</th><th>输入</th><th>期望</th></tr>
    </thead>
    <tbody>
      <tr><td>html.pdf</td><td>本页</td><td>生成单页 PDF，含中文与表格</td></tr>
      <tr><td>html.pdf</td><td>带 CSS 的页面</td><td>保留背景色与边框</td></tr>
    </tbody>
  </table>
  <div class="note">注意：中文渲染依赖本机字体；若输出为方框，需在报告里标注为字体缺失而非转换失败。</div>
</body>
</html>
"""


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    html_dir = root / "html"
    html_dir.mkdir(parents=True, exist_ok=True)
    target = html_dir / "sample.html"
    target.write_text(HTML, encoding="utf-8")
    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
