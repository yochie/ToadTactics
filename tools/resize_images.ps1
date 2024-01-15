# Specify the directory where the original images are located
$originalDirectoryPath = "D:\Downloads\Character Icons(1)"

# Specify the directory to store resized images
$resizedDirectoryPath = "D:\Downloads\Character Icons(1)\resized"

# Specify the new resolution (width x height)
$newWidth = 512		
$newHeight = 512

# Create the resized images directory if it doesn't exist
if (-not (Test-Path $resizedDirectoryPath)) {
    New-Item -ItemType Directory -Path $resizedDirectoryPath | Out-Null
}

# Get all PNG files in the original directory
$pngFiles = Get-ChildItem -Path $originalDirectoryPath -Filter *.png

# Load System.Drawing assembly
Add-Type -AssemblyName System.Drawing

# Iterate through each PNG file and resize
foreach ($pngFile in $pngFiles) {
    # Load the image
    $image = [System.Drawing.Image]::FromFile($pngFile.FullName)

    # Create a new bitmap with the desired resolution
    $newImage = New-Object System.Drawing.Bitmap $newWidth, $newHeight

    # Create a graphics object from the new bitmap
    $graphics = [System.Drawing.Graphics]::FromImage($newImage)

    # Draw the original image onto the new bitmap with the new resolution
    $graphics.DrawImage($image, 0, 0, $newWidth, $newHeight)

    # Generate the path for the resized image
    $resizedImagePath = Join-Path -Path $resizedDirectoryPath -ChildPath $pngFile.Name

    # Save the resized image as PNG
    $newImage.Save($resizedImagePath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Dispose of the image and graphics objects to free up resources
    $image.Dispose()
    $graphics.Dispose()

    Write-Host "Resized and saved: $($pngFile.Name)"
}

Write-Host "PNG image resizing completed successfully."
