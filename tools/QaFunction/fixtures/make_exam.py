"""Generate the exam.single fixtures: a DOCX template holding an image whose alt text is
"照片", plus the replacement photo that must end up inside the produced document.

Usage:
    python make_exam.py
"""

from __future__ import annotations

import json
import sys
import tempfile
from pathlib import Path

from docx import Document
from docx.shared import Cm
from PIL import Image

REPO = Path(__file__).resolve().parents[3]
OUT = REPO / "testdata" / "exam"

PLACEHOLDER_SIZE = (200, 260)
PHOTO_SIZE = (240, 300)


def solid(path: Path, size: tuple[int, int], color: tuple[int, int, int]) -> None:
    Image.new("RGB", size, color).save(path, "PNG")


def main() -> int:
    OUT.mkdir(parents=True, exist_ok=True)

    photo = OUT / "person_photo.png"
    solid(photo, PHOTO_SIZE, (32, 160, 96))

    with tempfile.TemporaryDirectory() as tmp:
        placeholder = Path(tmp) / "placeholder.png"
        solid(placeholder, PLACEHOLDER_SIZE, (40, 80, 200))

        document = Document()
        document.add_heading("考评表换照片（QA 夹具）", level=1)
        document.add_paragraph("下面这张图带着替代文字「照片」，用于验证按 AltText 替换图片。")
        run = document.add_paragraph().add_run()
        run.add_picture(str(placeholder), width=Cm(3.2))

        shapes = document.inline_shapes
        if not shapes:
            raise SystemExit("夹具生成失败：文档里没有内联图片")
        shapes[0]._inline.docPr.set("descr", "照片")
        shapes[0]._inline.docPr.set("name", "person-photo")

        document.add_paragraph("第二段用于确认正文未被破坏。")
        template = OUT / "person_template.docx"
        document.save(template)

    summary = {
        "person_template.docx": {
            "bytes": (OUT / "person_template.docx").stat().st_size,
            "altText": "照片",
            "placeholder": list(PLACEHOLDER_SIZE),
        },
        "person_photo.png": {
            "bytes": photo.stat().st_size,
            "size": list(PHOTO_SIZE),
            "note": "尺寸与占位图不同，替换后可从 media 尺寸判定",
        },
    }
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
