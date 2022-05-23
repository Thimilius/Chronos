@echo off

pushd %~dp0\..\Chronos\
dotnet publish -r win-x64 -c distribution
popd

copy "%~dp0\..\Build\Debug\net6.0\Chronos.System.dll" "%~dp0\..\Build\Release\net6.0\win-x64\native\Chronos.System.dll"