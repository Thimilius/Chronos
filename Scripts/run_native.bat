@echo off

pushd %~dp0\..\Build\Release\net6.0\win-x64\native\

Chronos --trace=Diagnostic %~dp0\..\Build\Debug\net6.0\Sandbox.dll %*

popd