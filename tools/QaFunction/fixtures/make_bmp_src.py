"""Generate the tiny 3-colour PNG used by the bmpPixelBits checks.

Run with the bundled Python runtime:
  python make_bmp_src.py <testdata-root>

Produces:
  image/bmp_src.png  32x32, three flat colour blocks (24bpp source is lossless,
                     so 8/24/32 bpp BMP output can be compared on bit depth alone).
"""

import sys
from pathlib import Path

from PIL import Image, ImageDraw

SIZE = 32
COLOURS = ((203, 52, 61), (52, 122, 203), (240, 214, 96))


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    image_dir = root / "image"
    image_dir.mkdir(parents=True, exist_ok=True)

    image = Image.new("RGB", (SIZE, SIZE), COLOURS[0])
    draw = ImageDraw.Draw(image)
    draw.rectangle((0, 0, SIZE - 1, 10), fill=COLOURS[0])
    draw.rectangle((0, 11, SIZE - 1, 21), fill=COLOURS[1])
    draw.rectangle((0, 22, SIZE - 1, SIZE - 1), fill=COLOURS[2])

    target = image_dir / "bmp_src.png"
    image.save(target, format="PNG")
    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
