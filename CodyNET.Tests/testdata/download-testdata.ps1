$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$targetDir = Join-Path $rootDir "wdc65c02/v1"

if (Test-Path $targetDir) {
    $jsonFiles = Get-ChildItem -Path $targetDir -Filter "*.json" -ErrorAction SilentlyContinue
    if ($jsonFiles.Count -gt 0) {
        Write-Host "Single step test data already present at $targetDir."
        exit 0
    }
}

$tmpDir = Join-Path $env:TEMP ([System.Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tmpDir | Out-Null

try {
    Write-Host "Downloading single step test data (954 MB) . This may take a few minutes..."
    $archive = Join-Path $tmpDir "65x02.zip"
    $sourceUrl = "https://github.com/SingleStepTests/65x02/archive/refs/heads/main.zip"

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $client = New-Object System.Net.WebClient
    $client.Headers["User-Agent"] = "CodyNET-SingleStep-Downloader"

    try {
        $client.DownloadFile($sourceUrl, $archive)
    }
    finally {
        $client.Dispose()
    }

    Expand-Archive -Path $archive -DestinationPath $tmpDir

    $sourceDir = Get-ChildItem -Path $tmpDir -Directory -Recurse | Where-Object {
        $_.FullName -match "wdc65c02\\v1$"
    } | Select-Object -First 1

    if (-not $sourceDir) {
        Write-Error "Could not locate wdc65c02/v1 in downloaded archive."
    }

    New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
    Copy-Item -Path (Join-Path $sourceDir.FullName "*") -Destination $targetDir -Recurse -Force

    Write-Host "Downloaded single step test data to $targetDir."
}
finally {
    if (Test-Path $tmpDir) {
        Remove-Item -Path $tmpDir -Recurse -Force
    }
}
