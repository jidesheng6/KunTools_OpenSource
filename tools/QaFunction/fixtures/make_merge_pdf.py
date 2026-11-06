"""Generate the two 2-page PDF fixtures used by the pdf.merge check.

Aspose runs in evaluation mode here (no .lic), which caps a document at 4 elements.
A 3+3 page merge therefore cannot be exercised, so the merge check uses 2+2 = 4 pages:
that stays inside the cap while still proving pages from both inputs are concatenated.

DOCX inputs are built with python-docx; the PDFs come from LibreOffice Kit, exactly like
the other fixtures, so nothing is hand-written at the zip/PDF level.

Usage:
    python make_merge_pdf.py [--node NODE --cli CLI]
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

from docx import Document
from docx.enum.text import WD_BREAK

REPO = Path(__file__).resolve().parents[3]
TESTDATA = REPO / "testdata"

DEFAULT_NODE = Path(r"C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\node\bin\node.exe")
DEFAULT_CLI = Path(
    r"D:\dsh\resources\app.asar.unpacked\dsh\node_modules\@deepseek-ai\libreoffice-kit\lib\cli.js"
)


def build_docx(path: Path, title: str, marker: str) -> None:
    document = Document()
    document.add_heading(title, level=1)
    document.add_paragraph(f"本文件用于验证合并顺序，标记 {marker}。")

    document.add_paragraph()
    document.add_paragraph().add_run().add_break(WD_BREAK.PAGE)
    document.add_heading(f"{title} 第 2 页", level=2)
    document.add_paragraph(f"第 2 页内容，标记 {marker}-P2。")

    path.parent.mkdir(parents=True, exist_ok=True)
    document.save(path)


def export_pdf(source: Path, target: Path, node: Path, cli: Path) -> dict:
    target.parent.mkdir(parents=True, exist_ok=True)
    if target.exists():
        target.unlink()

    completed = subprocess.run(
        [str(node), str(cli), "convert", "--input", str(source), "--output", str(target)],
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    payload = (completed.stdout or "").strip() or (completed.stderr or "").strip()
    with_path = target.exists()
    ok = with_path and target.read_bytes()[:5] == b"%PDF-"
    return {
        "source": str(source.relative_to(REPO)),
        "target": str(target.relative_to(REPO)),
        "exit": completed.returncode,
        "created": with_path,
        "size": target.stat().st_size if with_path else 0,
        "ok": ok,
        "output": payload[-400:],
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--node", default=str(DEFAULT_NODE))
    parser.add_argument("--cli", default=str(DEFAULT_CLI))
    args = parser.parse_args()

    node, cli = Path(args.node), Path(args.cli)
    if not node.exists() or not cli.exists():
        print(json.dumps({"error": "node/cli 不存在", "node": str(node), "cli": str(cli)}, ensure_ascii=False))
        return 2

    results = []
    for suffix, marker in (("a", "MERGE-A"), ("b", "MERGE-B")):
        docx = TESTDATA / "word" / f"merge_{suffix}.docx"
        build_docx(docx, f"合并夹具 {suffix.upper()}", marker)
        results.append(export_pdf(docx, TESTDATA / "pdf" / f"merge_{suffix}.pdf", node, cli))

    print(json.dumps(results, ensure_ascii=False, indent=2))
    return 0 if all(item["ok"] for item in results) else 1


if __name__ == "__main__":
    sys.exit(main())
