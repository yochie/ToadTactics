
# Specify the root folder
$rootFolder = "D:\Downloads\toConvert"

# Specify the path to magick.exe
$magickPath = "magick.exe"

# Create a 'white' folder within the root folder
$whiteFolder = Join-Path $rootFolder "white"
if (-not (Test-Path -Path $whiteFolder)) {
    New-Item -Path $whiteFolder -ItemType Directory
}

# Get all PNG files in subfolders
$pngFiles = Get-ChildItem -Path $rootFolder -Recurse -Filter "*.png" | Where-Object { $_.Extension -eq ".png" }

# Loop through each PNG file and run the magick command
foreach ($pngFile in $pngFiles) {
    $outputFile = Join-Path $whiteFolder $pngFile.Name
    $command = "$magickPath `"$($pngFile.FullName)`" -fill white -colorize 100% `"$outputFile`""
    Invoke-Expression $command
    Write-Host "Processed: $($pngFile.FullName) => $($outputFile)"
}
