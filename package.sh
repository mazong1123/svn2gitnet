#!/bin/bash

set -e

tar -zcvf svn2gitnet-ubuntu-x64.tar.gz ./src/bin/Release/netcoreapp2.0/ubuntu.16.04-x64/publish
tar -zcvf svn2gitnet-centos-x64.tar.gz ./src/bin/Release/netcoreapp2.0/centos.7-x64/publish
tar -zcvf svn2gitnet-debian-x64.tar.gz ./src/bin/Release/netcoreapp2.0/debian.8-x64/publish
tar -zcvf svn2gitnet-fedora-x64.tar.gz ./src/bin/Release/netcoreapp2.0/fedora.24-x64/publish
tar -zcvf svn2gitnet-rhel-x64.tar.gz ./src/bin/Release/netcoreapp2.0/rhel.7-x64/publish
tar -zcvf svn2gitnet-osx-x64.tar.gz ./src/bin/Release/netcoreapp2.0/osx.10.12-x64/publish