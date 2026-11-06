"""Generate DOCX fixtures for the function QA harness.

Run with the bundled Python runtime:
  python make_docx.py <testdata-root>

Produces:
  word/sample.docx   3 pages (page breaks) + heading + table + replace placeholders
  word/sample2.docx  1 page, used as the second input of word.merge
"""

import sys
from pathlib import Path

from docx import Document
from docx.enum.text import WD_BREAK
from docx.shared import Pt


def build_main(path: Path) -> None:
    document = Document()

    heading = document.add_heading("DocumentTools QA 夹具", level=0)
    heading.runs[0].font.size = Pt(20)

    document.add_paragraph("第一部分：中英文混排正文。")
    document.add_paragraph("The quick brown fox jumps over the lazy dog. 1234567890.")
    document.add_paragraph("占位符：{{NAME}} / {{DATE}}（供 word.replace 验证）。")

    table = document.add_table(rows=3, cols=3)
    table.style = "Table Grid"
    header = ("序号", "项目", "结果")
    for col, text in enumerate(header):
        table.cell(0, col).text = text
    rows = [
        ("1", "WORD 转 PDF", "待测"),
        ("2", "PDF 转 WORD", "待测"),
    ]
    for row_index, row in enumerate(rows, start=1):
        for col, text in enumerate(row):
            table.cell(row_index, col).text = text

    # Page 2
    paragraph = document.add_paragraph("第二部分：分页用于验证 pdf.split 的多页行为。")
    paragraph.runs[0].add_break(WD_BREAK.PAGE)
    document.add_paragraph("第二页内容：多页 PDF 拆分夹具。")

    # Page 3
    paragraph = document.add_paragraph("第三部分：再分一页。")
    paragraph.runs[0].add_break(WD_BREAK.PAGE)
    document.add_paragraph("第三页内容：确保 sample.pdf 至少 3 页。")

    document.save(str(path))


def build_second(path: Path) -> None:
    document = Document()
    document.add_heading("合并附录", level=1)
    document.add_paragraph("这是被合并的第二份文档（word.merge 使用）。")
    document.save(str(path))


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    word_dir = root / "word"
    word_dir.mkdir(parents=True, exist_ok=True)

    build_main(word_dir / "sample.docx")
    build_second(word_dir / "sample2.docx")

    for name in ("sample.docx", "sample2.docx"):
        target = word_dir / name
        print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
