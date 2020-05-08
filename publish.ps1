dotnet clean
dotnet build -c Release
dotnet test -c Release

if ($LASTEXITCODE -eq 0)
{
	Write-Host "-- all tests passed, publishing..."
	dotnet publish -c Release
}
else
{
	Write-Host "!! tests failed!"
}