#!/bin/bash

SCRIPT=`realpath $0`
SCRIPTPATH=`dirname $SCRIPT`

pushd "${SCRIPTPATH}/../Chronos/" >/dev/null 2>&1
dotnet publish -r linux-x64 -c distribution
popd >/dev/null 2>&1

cp "${SCRIPTPATH}/../Build/Debug/net6.0/Chronos.System.dll" "${SCRIPTPATH}/../Build/Release/net6.0/linux-x64/native/Chronos.System.dll" >/dev/null 2>&1
