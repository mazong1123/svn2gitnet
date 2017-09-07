#!/bin/bash

set -e

tar -zcvf ubuntu.16.04-x64.tar.gz ./src/bin/Release/netcoreapp2.0/ubuntu.16.04-x64/publish
tar -zcvf centos.7-x64.tar.gz ./src/bin/Release/netcoreapp2.0/centos.7-x64/publish
tar -zcvf debian.8-x64.tar.gz ./src/bin/Release/netcoreapp2.0/debian.8-x64/publish
tar -zcvf fedora.24-x64.tar.gz ./src/bin/Release/netcoreapp2.0/fedora.24-x64/publish
tar -zcvf rhel.7-x64.tar.gz ./src/bin/Release/netcoreapp2.0/rhel.7-x64/publish
tar -zcvf osx.10.12-x64.tar.gz ./src/bin/Release/netcoreapp2.0/osx.10.12-x64/publish