"""Generate the EPUB fixture for the function QA harness.

Run with the bundled Python runtime:
  python make_epub.py <testdata-root>

Produces:
  epub/sample.epub  minimal valid EPUB 3 (mimetype + container + package + one chapter)

EPUB is not an Office format and LibreOffice Kit refuses docx -> epub, so this
script assembles the container directly from the EPUB 3 spec (uncompressed
mimetype entry first, then OCF/OPF/XHTML members).
"""

import sys
import zipfile
from pathlib import Path

MIMETYPE = "application/epub+zip"

CONTAINER = """<?xml version="1.0" encoding="UTF-8"?>
<container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
  <rootfiles>
    <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
  </rootfiles>
</container>
"""

OPF = """<?xml version="1.0" encoding="UTF-8"?>
<package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="bookid">
  <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
    <dc:identifier id="bookid">urn:uuid:documenttools-qa-fixture</dc:identifier>
    <dc:title>DocumentTools QA 夹具</dc:title>
    <dc:language>zh-CN</dc:language>
    <dc:creator>DocumentTools</dc:creator>
  </metadata>
  <manifest>
    <item id="chapter1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
    <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
  </manifest>
  <spine>
    <itemref idref="chapter1"/>
  </spine>
</package>
"""

CHAPTER = """<?xml version="1.0" encoding="UTF-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" lang="zh-CN">
<head><title>第一章</title></head>
<body>
  <h1>EPUB 转 WORD / EPUB 转 PDF 夹具</h1>
  <p>本文件由 tools/QaFunction/fixtures/make_epub.py 生成，用于验证 EpubToWord / EpubToPdf。</p>
  <p>第二段：中文与 English mixed content 1234567890。</p>
</body>
</html>
"""

NAV = """<?xml version="1.0" encoding="UTF-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" lang="zh-CN">
<head><title>目录</title></head>
<body>
  <nav epub:type="toc" id="toc">
    <h1>目录</h1>
    <ol><li><a href="chapter1.xhtml">第一章</a></li></ol>
  </nav>
</body>
</html>
"""


def main() -> int:
    root = Path(sys.argv[1]).resolve()
    epub_dir = root / "epub"
    epub_dir.mkdir(parents=True, exist_ok=True)
    target = epub_dir / "sample.epub"

    with zipfile.ZipFile(target, "w") as archive:
        # OCF requires the mimetype entry to be first and stored without compression.
        info = zipfile.ZipInfo("mimetype")
        info.compress_type = zipfile.ZIP_STORED
        archive.writestr(info, MIMETYPE)
        archive.writestr("META-INF/container.xml", CONTAINER, zipfile.ZIP_DEFLATED)
        archive.writestr("OEBPS/content.opf", OPF, zipfile.ZIP_DEFLATED)
        archive.writestr("OEBPS/chapter1.xhtml", CHAPTER, zipfile.ZIP_DEFLATED)
        archive.writestr("OEBPS/nav.xhtml", NAV, zipfile.ZIP_DEFLATED)

    print(f"{target} {target.stat().st_size} bytes")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
