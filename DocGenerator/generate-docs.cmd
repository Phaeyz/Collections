setlocal
set libName=Phaeyz.Collections
set repoUrl=https://github.com/Phaeyz/Collections
dotnet run ..\%libName%\bin\Debug\net9.0\%libName%.dll ..\docs --source %repoUrl%/blob/main/%libName% --namespace %libName% --clean
endlocal