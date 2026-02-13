#!/bin/bash
# Workaround for ARM64 vstest host detection bug on distro-packaged .NET 8 SDK.
# Uses the xunit console runner directly instead of dotnet test.
set -e
cd "$(dirname "$0")"
export DOTNET_GCHeapHardLimit=0x10000000
export DOTNET_ROLL_FORWARD=LatestMajor

dotnet build --nologo -q 2>/dev/null
dotnet exec ~/.nuget/packages/xunit.runner.console/2.9.0/tools/net6.0/xunit.console.dll bin/Debug/net8.0/api.tests.dll "$@"
