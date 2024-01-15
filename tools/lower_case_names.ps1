# Specify the directory where the files are located
$directoryPath = "D:\Downloads\Character Icons(1)"

# Get all files in the specified directory
$files = Get-ChildItem -Path $directoryPath

# Iterate through each file and convert the filename to lowercase
foreach ($file in $files) {
    $newFileName = $file.Name.ToLower()
    $newFilePath = Join-Path -Path $directoryPath -ChildPath $newFileName

    # Rename the file
    Rename-Item -Path $file.FullName -NewName $newFileName -Force
}

Write-Host "File name conversion to lowercase completed successfully."
