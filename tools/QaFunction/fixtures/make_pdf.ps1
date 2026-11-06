# Regenerate testdata/pdf/sample.pdf from testdata/word/sample.docx.
# Uses the LibreOffice Kit CLI bundled with the DSH runtime (no system soffice).
#
#   pwsh -File tools/QaFunction/fixtures/make_pdf.ps1
#
# The source DOCX must already exist (run make_docx.py first). The export keeps
# the source file untouched and writes a 3-page PDF (page breaks in the DOCX).

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$inputDocx = Join-Path $repoRoot 'testdata\word\sample.docx'
$outputPdf = Join-Path $repoRoot 'testdata\pdf\sample.pdf'

$node = 'D:\dsh\resources\runtime\primary-runtime\dependencies\node\bin\node.exe'
if (-not (Test-Path $node)) {
    $node = 'C:\Users\pw\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\node\bin\node.exe'
}

$cli = 'D:\dsh\resources\app.asar.unpacked\dsh\node_modules\@deepseek-ai\libreoffice-kit\lib\cli.js'
if (-not (Test-Path $cli)) {
    throw "LibreOffice Kit CLI not found at $cli"
}

if (-not (Test-Path $inputDocx)) {
    throw "Missing source DOCX: $inputDocx"
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputPdf) | Out-Null
& $node $cli convert --input $inputDocx --output $outputPdf
if ($LASTEXITCODE -ne 0) {
    throw "LibreOffice Kit convert failed with exit code $LASTEXITCODE"
}

$bytes = [System.IO.File]::ReadAllBytes($outputPdf)
$signature = [System.Text.Encoding]::ASCII.GetString($bytes[0..4])
if ($signature -ne '%PDF-') {
    throw "Output is not a PDF (signature: $signature)"
}

Write-Output ("{0} {1} bytes, signature {2}" -f $outputPdf, $bytes.Length, $signature)
