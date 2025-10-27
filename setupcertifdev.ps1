# PowerShell script to set up development certificates for Docker
# Run this script as Administrator

Write-Host "Setting up ASP.NET Core development certificates for Docker..." -ForegroundColor Green

# Generate development certificate
Write-Host "Generating development certificate..." -ForegroundColor Yellow
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Export certificate for Docker
$certPassword = "DevCertPassword123!"
$certPath = "$env:USERPROFILE\.aspnet\https"

Write-Host "Creating certificate directory: $certPath" -ForegroundColor Yellow
if (!(Test-Path $certPath)) {
    New-Item -Path $certPath -ItemType Directory -Force
}

Write-Host "Exporting certificate to: $certPath\aspnetapp.pfx" -ForegroundColor Yellow
dotnet dev-certs https -ep "$certPath\aspnetapp.pfx" -p $certPassword

# Create .env file with certificate password
$envContent = @"
# Certificate password for Docker development
CERT_PASSWORD=$certPassword
"@

Write-Host "Creating .env file with certificate password..." -ForegroundColor Yellow
$envContent | Out-File -FilePath ".env" -Encoding UTF8

Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "Certificate password: $certPassword" -ForegroundColor Cyan
Write-Host "You can now run: docker-compose up -d" -ForegroundColor Cyan