"""Generate image fixtures for the function QA harness.

Run with the bundled Python runtime:
  python make_images.py <testdata-root>

Produces:
  image/sample.png          plain raster for image.pdf / image.compress
  image/sample.jpg          JPEG variant (image.compress 转 JPG)
  exam/circle_sheet.png     考评表仿制页：规则圆圈网格，供 exam.circle 模板匹配
  exam/circle_sheet.json    模板矩形与期望匹配数（与图片同源生成，避免手抄错位）
"""

import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

CANVAS = (800, 600)
CIRCLE_CANVAS = (900, 1200)
RADIUS = 26
COLUMNS = 5
ROWS = 5
ORIGIN_X = 140
ORIGIN_Y = 240
STEP_X = 150
STEP_Y = 180


def load_font(size: int):
    candidates = [
        r"C:\Windows\Fonts\msyh.ttc",
        r"C:\Windows\Fonts\simhei.ttf",
        r"C:\Windows\Fonts\simsun.ttc",
    ]
    for candidate in candidates:
        if Path(candidate).exists():
            try:
                return ImageFont.truetype(candidate, size)
            except OSError:
                continue
    return ImageFont.load_default()


def build_sample_png(path: Path) -> None:
    image = Image.new("RGB", CANVAS, (255, 255, 255))
    draw = ImageDraw.Draw(image)
    draw.rectangle((0, 0, CANVAS[0] - 1, 90), fill=(31, 78, 121))
    draw.text((30, 30), "DocumentTools QA", font=load_font(34), fill=(255, 255, 255))
    for index, color in enumerate([(41, 128, 185), (39, 174, 96), (243, 156, 18)]):
        draw.rectangle((40 + index * 240, 140, 240 + index * 240, 330), fill=color)
    for line in range(4):
        draw.line((40, 380 + line * 45, 760, 380 + line * 45), fill=(120, 120, 120), width=3)
    draw.text((40, 540), "图片转 PDF / 压缩图片 夹具", font=load_font(26), fill=(20, 20, 20))
    image.save(path, format="PNG", optimize=True)


def build_circle_sheet(png_path: Path, json_path: Path) -> dict:
    image = Image.new("RGB", CIRCLE_CANVAS, (255, 255, 255))
    draw = ImageDraw.Draw(image)
    font = load_font(22)
    draw.text((60, 60), "考评表圆圈标记（QA 夹具）", font=load_font(32), fill=(0, 0, 0))
    draw.text((60, 130), "姓名：____________    编号：____________", font=font, fill=(0, 0, 0))

    centers = []
    for row in range(ROWS):
        for column in range(COLUMNS):
            cx = ORIGIN_X + column * STEP_X
            cy = ORIGIN_Y + row * STEP_Y
            draw.ellipse(
                (cx - RADIUS, cy - RADIUS, cx + RADIUS, cy + RADIUS),
                outline=(0, 0, 160),
                width=4,
                fill=(235, 242, 255),
            )
            centers.append((cx, cy))
            draw.text((cx - 52, cy + RADIUS + 10), f"{row + 1}-{column + 1}", font=font, fill=(60, 60, 60))

    for line in range(3):
        draw.line((60, 1090 + line * 30, 840, 1090 + line * 30), fill=(140, 140, 140), width=2)

    image.save(png_path, format="PNG", optimize=True)

    first = centers[0]
    payload = {
        "image": "exam/circle_sheet.png",
        "width": CIRCLE_CANVAS[0],
        "height": CIRCLE_CANVAS[1],
        "radius": RADIUS,
        "expectedMatches": len(centers),
        "template": {
            "left": first[0] - RADIUS,
            "top": first[1] - RADIUS,
            "width": RADIUS * 2,
            "height": RADIUS * 2,
        },
        "centers": centers,
    }
    json_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
    return payload


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    image_dir = root / "image"
    exam_dir = root / "exam"
    image_dir.mkdir(parents=True, exist_ok=True)
    exam_dir.mkdir(parents=True, exist_ok=True)

    png = image_dir / "sample.png"
    build_sample_png(png)
    image = Image.open(png).convert("RGB")
    jpg = image_dir / "sample.jpg"
    image.save(jpg, format="JPEG", quality=88, optimize=True)

    sheet = exam_dir / "circle_sheet.png"
    payload = build_circle_sheet(sheet, exam_dir / "circle_sheet.json")

    for target in (png, jpg, sheet, exam_dir / "circle_sheet.json"):
        print(f"{target} {target.stat().st_size} bytes")
    print(f"expectedMatches={payload['expectedMatches']} template={payload['template']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
