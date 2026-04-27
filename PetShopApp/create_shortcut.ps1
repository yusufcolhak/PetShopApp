$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("C:\Users\Yusuf\Desktop\PetShop.lnk")
$Shortcut.TargetPath = "C:\Users\Yusuf\Desktop\PROJE\PetShopApp\publish\PetShopApp.exe"
$Shortcut.WorkingDirectory = "C:\Users\Yusuf\Desktop\PROJE\PetShopApp\publish"
$Shortcut.Save()
Write-Host "Kısayol Masaüstüne Oluşturuldu! ✅"
