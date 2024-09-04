#!/bin/bash
set -e

./deployment/build.sh
dotnet test \
	-p:CollectCoverage=true \
	-p:BuildInParallel=true \
	-p:ContinuousIntegrationBuild=false \
	-p:DebugType=portable \
	-p:CopyLocalLockFileAssemblies=true \
	-m:8 \
	--configuration Release \
	--collect:"XPlat Code Coverage" \
	--settings ./tests/coverlet.runsettings \
	--logger:"junit;LogFilePath=TestResults/test-result.junit.xml" \
	--no-build
