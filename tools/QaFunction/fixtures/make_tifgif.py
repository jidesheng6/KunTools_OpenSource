"""Generate the tiny PNG used by the TIF/GIF encoder checks (task-27).

Why such a plain image: the check converts it to .tif/.gif and then compares the
top-left pixel colour channel-by-channel (tolerance 8). Only four flat colours are
used so an 8bpp indexed GIF palette reproduces them exactly, and the assertion
measures the encoder, not the quantizer.

Run with the bundled interpreter:
  python tools\\QaFunction\\fixtures\\make_tifgif.py
"""

from pathlib import Path

from PIL import Image, ImageDraw

REPO_ROOT = Path(__file__).resolve().parents[3]
OUTPUT = REPO_ROOT / "testdata" / "image" / "tifgif_src.png"

WIDTH, HEIGHT = 96, 64

# Top-left quadrant colour is what the check reads back as SKColor(203, 52, 61).
QUADRANTS = [
    (0, 0, WIDTH // 2, HEIGHT // 2, (203, 52, 61)),
    (WIDTH // 2, 0, WIDTH, HEIGHT // 2, (60, 122, 200)),
    (0, HEIGHT // 2, WIDTH // 2, HEIGHT, (240, 200, 90)),
    (WIDTH // 2, HEIGHT // 2, WIDTH, HEIGHT, (34, 139, 87)),
]


def main() -> None:
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image = Image.new("RGB", (WIDTH, HEIGHT), QUADRANTS[0][4])
    draw = ImageDraw.Draw(image)
    for left, top, right, bottom, colour in QUADRANTS:
        draw.rectangle([left, top, right - 1, bottom - 1], fill=colour)

    # A thin dark frame keeps the image from being a degenerate 4-block grid while
    # staying inside the 4-colour palette budget.
    draw.rectangle([0, 0, WIDTH - 1, HEIGHT - 1], outline=(20, 20, 20))

    image.save(OUTPUT, format="PNG", optimize=True)

    size = OUTPUT.stat().st_size
    print(f"{OUTPUT.relative_to(REPO_ROOT)} = {size} B ({WIDTH}x{HEIGHT}, "
          f"corner=#{QUADRANTS[0][4][0]:02X}{QUADRANTS[0][4][1]:02X}{QUADRANTS[0][4][2]:02X})")
    if size > 20 * 1024:
        raise SystemExit(f"fixture too large: {size} B > 20480 B")


if __name__ == "__main__":
    main()
