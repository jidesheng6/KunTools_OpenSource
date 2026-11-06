"""Generate PPTX fixture for the function QA harness.

Run with the bundled Python runtime:
  python make_pptx.py <testdata-root>

Produces:
  ppt/sample.pptx  two 16:9 slides with title, bullets and a native chart
"""

import sys
from pathlib import Path

from pptx import Presentation
from pptx.chart.data import CategoryChartData
from pptx.enum.chart import XL_CHART_TYPE
from pptx.util import Inches, Pt


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    ppt_dir = root / "ppt"
    ppt_dir.mkdir(parents=True, exist_ok=True)

    presentation = Presentation()
    presentation.slide_width = Inches(13.333)
    presentation.slide_height = Inches(7.5)

    blank = presentation.slide_layouts[6]

    slide = presentation.slides.add_slide(blank)
    title = slide.shapes.add_textbox(Inches(0.6), Inches(0.5), Inches(12), Inches(1))
    run = title.text_frame.paragraphs[0].add_run()
    run.text = "QA 夹具演示文稿"
    run.font.size = Pt(32)

    body = slide.shapes.add_textbox(Inches(0.8), Inches(1.8), Inches(11.5), Inches(2.4))
    frame = body.text_frame
    frame.text = "第一页要点："
    for line in ("WORD/PDF/EXCEL/PPT 转 PDF", "PDF 压缩与水印", "条码与圆圈修复"):
        paragraph = frame.add_paragraph()
        paragraph.text = "• " + line
        paragraph.font.size = Pt(20)

    data = CategoryChartData()
    data.categories = ["Q1", "Q2", "Q3"]
    data.add_series("通过项", [3, 5, 8])
    slide.shapes.add_chart(
        XL_CHART_TYPE.COLUMN_CLUSTERED, Inches(0.8), Inches(4.2),
        Inches(11.5), Inches(2.8), data)

    second = presentation.slides.add_slide(blank)
    title2 = second.shapes.add_textbox(Inches(0.6), Inches(0.5), Inches(12), Inches(1))
    run2 = title2.text_frame.paragraphs[0].add_run()
    run2.text = "第二页"
    run2.font.size = Pt(32)
    body2 = second.shapes.add_textbox(Inches(0.8), Inches(1.8), Inches(11.5), Inches(2))
    body2.text_frame.text = "用于验证 ppt.pdf 的多页输出。"

    target = ppt_dir / "sample.pptx"
    presentation.save(str(target))
    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
