
# Specify the root folder
$rootFolder = "E:\code\temp_name\thom\Assets\Art\2D\ui\cursors"

# Specify the path to magick.exe
$magickPath = "magick.exe"

# Create a 'black' folder within the root folder
$destFolder = Join-Path $rootFolder "black"
if (-not (Test-Path -Path $destFolder )) {
    New-Item -Path $destFolder -ItemType Directory
}

# Get all PNG files in subfolders
$pngFiles = Get-ChildItem -Path $rootFolder -Recurse -Filter "*.png" | Where-Object { $_.Extension -eq ".png" }

# Loop through each PNG file and run the magick command
foreach ($pngFile in $pngFiles) {
    $outputFile = Join-Path $destFolder $pngFile.Name
    $command = "$magickPath `"$($pngFile.FullName)`" -fill black -colorize 100% `"$outputFile`""
    Invoke-Expression $command
    Write-Host "Processed: $($pngFile.FullName) => $($outputFile)"
}
