@echo off

pushd %~dp0\..\Build\Release\netcoreapp3.1\win-x64\native\

Chronos -t Diagnostic %~dp0\..\Build\Debug\netcoreapp3.1\Chronos.Tests.dll %*

popd