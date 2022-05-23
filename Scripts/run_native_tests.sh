#!/bin/bash

SCRIPT=`realpath $0`
SCRIPTPATH=`dirname $SCRIPT`

pushd "${SCRIPTPATH}/../Build/Release/net6.0/linux-x64/native/" >/dev/null 2>&1

./Chronos -t Diagnostic "${SCRIPTPATH}/../Build/Debug/net6.0/Chronos.Tests.dll"

popd >/dev/null 2>&1
