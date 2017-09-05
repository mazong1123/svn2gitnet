#!/bin/bash

set -e

# Unit tests
cd tests/unittests
dotnet test
cd ../../

# Preparing for integration test.
chmod +x ./publish.sh
./publish.sh
mkdir integrationtests
cp -r src/bin/Release/netcoreapp2.0 integrationtests

# Integration test
cd tests/integrationtests
dotnet test
cd ../../

# Clean up
rm -rf integrationtests
