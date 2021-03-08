#!/bin/bash

SCRIPT=`realpath $0`
SCRIPTPATH=`dirname $SCRIPT`

pushd "${SCRIPTPATH}/../Build/Release/netcoreapp3.1/linux-x64/native/" >/dev/null 2>&1

./Chronos -t Diagnostic "${SCRIPTPATH}/../Build/Debug/netcoreapp3.1/Sandbox.dll"

popd >/dev/null 2>&1
