"""Generate the PPTX-with-notes fixture used by the ppt.pdf exportNotes checks.

Run with the bundled Python runtime:
  python make_pptx_notes.py <testdata-root>

Produces:
  ppt/notes.pptx  two 16:9 slides, each with an ASCII speaker-note marker so the
                  PDF text layer can prove whether notes were exported at all.
"""

import sys
from pathlib import Path

from pptx import Presentation
from pptx.util import Inches, Pt

MARKER_A = "NOTES-MARKER-ALPHA"
MARKER_B = "NOTES-MARKER-BETA"


def add_slide(presentation, title_text, body_text, notes_text):
    slide = presentation.slides.add_slide(presentation.slide_layouts[6])

    title = slide.shapes.add_textbox(Inches(0.6), Inches(0.5), Inches(12), Inches(1))
    run = title.text_frame.paragraphs[0].add_run()
    run.text = title_text
    run.font.size = Pt(32)

    body = slide.shapes.add_textbox(Inches(0.8), Inches(1.8), Inches(11.5), Inches(2))
    body.text_frame.text = body_text

    # Speaker notes live in notesSlide; they are only visible when the exporter
    # is told to lay notes out (exportNotes != None).
    slide.notes_slide.notes_text_frame.text = notes_text
    return slide


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    ppt_dir = root / "ppt"
    ppt_dir.mkdir(parents=True, exist_ok=True)

    presentation = Presentation()
    presentation.slide_width = Inches(13.333)
    presentation.slide_height = Inches(7.5)

    add_slide(presentation, "备注导出夹具 第 1 页", "正文第一页，不含备注标记。", MARKER_A)
    add_slide(presentation, "备注导出夹具 第 2 页", "正文第二页，不含备注标记。", MARKER_B)

    target = ppt_dir / "notes.pptx"
    presentation.save(str(target))
    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
