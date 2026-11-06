"""Generate the name-list fixtures used by the name.vertical checks.

Three fixture groups, matching how the legacy WinForms page received input:

1. table_copy.docx + table_tabs.txt - a real Word table, plus exactly what copying that
   table yields (cells joined by TAB, rows by newline). The legacy splitter keys off TAB
   presence, so this is the branch worth pinning.
2. numbers_and_brackets.txt - newline list carrying digits and both half/full width brackets.
3. duplicates_and_two_chars.txt - duplicate names plus two-character names (double-align).

Usage:
    python make_name.py
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

from docx import Document

REPO = Path(__file__).resolve().parents[3]
OUT = REPO / "testdata" / "name"

TABLE_ROWS = [
    ["张三丰", "李四", "王小明", "○ 赵七"],
    ["钱八", "孙九 £", "周十", "吴十一"],
    ["郑十二", "冯十三", "陈十四", "褚十五"],
]

NUMBERS_AND_BRACKETS = [
    "张三1",
    "李四(组长)",
    "王五六（临时）",
    "赵七2",
    "钱八( )",
    "孙九",
    " 周十  ",
]

DUPLICATES_AND_TWO_CHARS = [
    "张三",
    "李四",
    "张三",
    "王小明",
    "李四",
    "周八",
    "周八",
    "郑十二",
]


def build_table_docx(path: Path) -> int:
    document = Document()
    document.add_heading("姓名列表（Word 表格）", level=1)
    table = document.add_table(rows=len(TABLE_ROWS), cols=len(TABLE_ROWS[0]))
    table.style = "Table Grid"
    for r, row in enumerate(TABLE_ROWS):
        for c, value in enumerate(row):
            table.cell(r, c).text = value
    path.parent.mkdir(parents=True, exist_ok=True)
    document.save(path)
    return len(TABLE_ROWS) * len(TABLE_ROWS[0])


def main() -> int:
    OUT.mkdir(parents=True, exist_ok=True)

    docx = OUT / "table_copy.docx"
    cells = build_table_docx(docx)

    # Exactly what Word puts on the clipboard for this table: TAB between cells, newline
    # between rows (a trailing newline included, as Word does).
    tabs = "\n".join("\t".join(row) for row in TABLE_ROWS) + "\n"
    tab_file = OUT / "table_tabs.txt"
    tab_file.write_text(tabs, encoding="utf-8")

    # Same table copied on Windows: CRLF row separators mixed with TABs. This is the shape
    # the legacy splitter really gets from the clipboard, and it pins the processing order
    # (line endings normalised first, then TAB -> newline) - a lone CR left over would leak
    # into the rendered name if that order were reversed.
    crlf = "\r\n".join("\t".join(row) for row in TABLE_ROWS) + "\r\n"
    crlf_file = OUT / "table_copy_crlf.txt"
    crlf_file.write_bytes(crlf.encode("utf-8"))

    numbers = OUT / "numbers_and_brackets.txt"
    numbers.write_text("\n".join(NUMBERS_AND_BRACKETS) + "\n", encoding="utf-8")

    duplicates = OUT / "duplicates_and_two_chars.txt"
    duplicates.write_text("\n".join(DUPLICATES_AND_TWO_CHARS) + "\n", encoding="utf-8")

    summary = {
        "table_copy.docx": {"bytes": docx.stat().st_size, "cells": cells, "rows": len(TABLE_ROWS)},
        "table_tabs.txt": {
            "bytes": tab_file.stat().st_size,
            "tabs": tabs.count("\t"),
            "lines": tabs.count("\n"),
            "note": "含 TAB，走 legacy 表格分支：先删换行，再把 TAB 视为分隔",
        },
        "table_copy_crlf.txt": {
            "bytes": crlf_file.stat().st_size,
            "tabs": crlf.count("\t"),
            "crlf": crlf.count("\r\n"),
            "note": "CRLF + TAB 混合：验证行尾先归一、再 TAB→换行",
        },
        "numbers_and_brackets.txt": {
            "bytes": numbers.stat().st_size,
            "entries": len(NUMBERS_AND_BRACKETS),
            "note": "数字 + 半角/全角括号 + 首尾空白",
        },
        "duplicates_and_two_chars.txt": {
            "bytes": duplicates.stat().st_size,
            "entries": len(DUPLICATES_AND_TWO_CHARS),
            "duplicate_names": ["张三", "李四", "周八"],
            "two_char_names": ["张三", "李四", "周八"],
        },
    }
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
