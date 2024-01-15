# Specify the directory where the PNG files are located
$directoryPath = "D:\Downloads\Character Icons(1)"

# Get all PNG files in the specified directory
$pngFiles = Get-ChildItem -Path $directoryPath -Filter *.png

# Iterate through each PNG file and remove "_Icon" from the filename
foreach ($file in $pngFiles) {
    $newFileName = $file.Name -replace "_Icon", ""
    $newFilePath = Join-Path -Path $directoryPath -ChildPath $newFileName

    # Rename the file
    Rename-Item -Path $file.FullName -NewName $newFileName -Force
}

Write-Host "Renaming completed successfully."