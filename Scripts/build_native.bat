@echo off

pushd %~dp0\..\Chronos\
dotnet publish -r win-x64 -c distribution
popd

copy "%~dp0\..\Build\Debug\netcoreapp3.1\Chronos.System.dll" "%~dp0\..\Build\Release\netcoreapp3.1\win-x64\native\Chronos.System.dll"