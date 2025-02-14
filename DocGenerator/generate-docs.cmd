setlocal
set lib=Phaeyz.Collections
set repoUrl=https://github.com/Phaeyz/Collections
dotnet run ..\%lib%\bin\Debug\net9.0\%lib%.dll ..\docs --source %repoUrl%/blob/main/%lib% --namespace %lib% --clean
endlocal