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
    Write-Host "Downloading single step test data (954 MB). This may take a few minutes..."
    $archive = Join-Path $tmpDir "65x02.zip"
    $sourceUrl = "https://github.com/SingleStepTests/65x02/archive/refs/heads/main.zip"
    
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $client = New-Object System.Net.WebClient
    $client.Headers["User-Agent"] = "CodyNET-SingleStep-Downloader"
    
    # Event-Handler für Fortschrittsanzeige
    Register-ObjectEvent -InputObject $client -EventName DownloadProgressChanged -Action {
        $received = [math]::Round($EventArgs.BytesReceived / 1MB, 2)
        $total = $EventArgs.TotalBytesToReceive
        
        if ($total -gt 0) {
            # Gesamtgröße bekannt
            $percent = $EventArgs.ProgressPercentage
            $totalMB = [math]::Round($total / 1MB, 2)
            Write-Progress -Activity "Downloading test data" `
                           -Status "$received MB / $totalMB MB" `
                           -PercentComplete $percent
        } else {
            # Gesamtgröße unbekannt (bei GitHub häufig der Fall)
            Write-Progress -Activity "Downloading test data" `
                           -Status "$received MB downloaded / 954 MB" `
                           -PercentComplete -1
        }
    } | Out-Null
    
    try {
        $client.DownloadFileAsync($sourceUrl, $archive)
        while ($client.IsBusy) {
            Start-Sleep -Milliseconds 100
        }
        Write-Progress -Activity "Downloading test data" -Completed
    } finally {
        # Event-Handler aufräumen
        Get-EventSubscriber | Where-Object { $_.SourceObject -eq $client } | Unregister-Event
        $client.Dispose()
    }
    
    Write-Host "Extracting archive..."
    Write-Progress -Activity "Extracting archive" -Status "In progress..."
    Expand-Archive -Path $archive -DestinationPath $tmpDir
    Write-Progress -Activity "Extracting archive" -Completed
    
    $sourceDir = Get-ChildItem -Path $tmpDir -Directory -Recurse | 
                 Where-Object { $_.FullName -match "wdc65c02\\v1$" } | 
                 Select-Object -First 1
    
    if (-not $sourceDir) {
        Write-Error "Could not locate wdc65c02/v1 in downloaded archive."
    }
    
    Write-Host "Copying files..."
    New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
    Copy-Item -Path (Join-Path $sourceDir.FullName "*") -Destination $targetDir -Recurse -Force
    
    Write-Host "Downloaded single step test data to $targetDir." -ForegroundColor Green
} finally {
    if (Test-Path $tmpDir) {
        Remove-Item -Path $tmpDir -Recurse -Force
    }
}