"""dev-parity fixtures (task-46).

Three fixtures that let the harness compare "what dev produces" against the new Core:

  word/six_pages.docx   6 pages, exactly 2 Word bookmarks (dev exports one PDF outline
                        per bookmark: wdExportCreateWordBookmarks).
  excel/three_sheets.xlsx  3 worksheets, one short table each (dev "每表一份 PDF").
  image/sample.webp     same pixels as sample.png, so WebP->image can be checked against
                        the source size instead of a hard-coded number.

Run with the bundled python:
  C:\\Users\\pw\\.dsh\\dsh-runtimes\\dsh-primary-runtime\\dependencies\\python\\python.exe ^
      tools\\QaFunction\\fixtures\\make_devparity.py
"""

from pathlib import Path

from docx import Document
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls
from docx.shared import Pt
from openpyxl import Workbook
from PIL import Image

ROOT = Path(__file__).resolve().parents[3] / "testdata"


def bookmark(paragraph, name: str, bid: int) -> None:
    """Wrap the paragraph text in a Word bookmark (w:bookmarkStart/End)."""
    start = parse_xml(f'<w:bookmarkStart {nsdecls("w")} w:id="{bid}" w:name="{name}"/>')
    end = parse_xml(f'<w:bookmarkEnd {nsdecls("w")} w:id="{bid}"/>')
    paragraph._p.insert(0, start)
    paragraph._p.append(end)


def make_six_pages() -> Path:
    doc = Document()
    doc.styles["Normal"].font.size = Pt(12)
    for page in range(1, 7):
        p = doc.add_paragraph(f"PAGE-{page} dev parity marker")
        if page in (2, 5):
            bookmark(p, f"PARITY_{page}", page)
        doc.add_paragraph(f"第 {page} 页正文：用于确认页数与书签无关。")
        if page != 6:
            doc.add_page_break()
    target = ROOT / "word" / "six_pages.docx"
    doc.save(target)
    return target


def make_three_sheets() -> Path:
    book = Workbook()
    first = True
    for name in ("成绩", "汇总", "明细"):
        sheet = book.active if first else book.create_sheet()
        sheet.title = name
        first = False
        sheet.append(["项目", "数值"])
        for row in range(1, 4):
            sheet.append([f"{name}-{row}", row * 10])
    target = ROOT / "excel" / "three_sheets.xlsx"
    book.save(target)
    return target


def make_webp() -> Path:
    source = ROOT / "image" / "sample.png"
    with Image.open(source) as image:
        target = ROOT / "image" / "sample.webp"
        image.convert("RGB").save(target, "WEBP", quality=80, method=4)
    return target


def main() -> None:
    for path in (make_six_pages(), make_three_sheets(), make_webp()):
        print(f"{path.relative_to(ROOT.parent)} {path.stat().st_size} B")


if __name__ == "__main__":
    main()
