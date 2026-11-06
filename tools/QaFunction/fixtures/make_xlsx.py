"""Generate XLSX fixture for the function QA harness.

Run with the bundled Python runtime:
  python make_xlsx.py <testdata-root>

Produces:
  excel/sample.xlsx  two sheets, numbers, a formula and a replace placeholder
"""

import sys
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Font


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    excel_dir = root / "excel"
    excel_dir.mkdir(parents=True, exist_ok=True)

    workbook = Workbook()
    sheet = workbook.active
    sheet.title = "成绩"

    sheet.append(("姓名", "科目", "分数"))
    for cell in sheet[1]:
        cell.font = Font(bold=True)

    rows = [
        ("张三", "语文", 88),
        ("李四", "数学", 95),
        ("王五", "英语", 79),
        ("赵六", "物理", 62),
    ]
    for row in rows:
        sheet.append(row)

    sheet.append(("平均", "", "=ROUND(AVERAGE(C2:C5),2)"))
    sheet.column_dimensions["A"].width = 14
    sheet.column_dimensions["B"].width = 14
    sheet.column_dimensions["C"].width = 12

    summary = workbook.create_sheet("汇总")
    summary.append(("项目", "值"))
    summary.append(("占位符", "{{NAME}}"))
    summary.append(("说明", "第二个工作表用于验证多 sheet 导出"))
    summary.column_dimensions["A"].width = 14
    summary.column_dimensions["B"].width = 32

    target = excel_dir / "sample.xlsx"
    workbook.save(str(target))
    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
