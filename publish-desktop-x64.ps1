dotnet restore
dotnet build .\Storm.sln -c Release --no-restore --nologo
dotnet publish .\StormDesktop\StormDesktop.csproj -c Release -r win-x64 /p:PublishSingleFile=true --no-self-contained --no-restore