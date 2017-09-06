#!/bin/bash

set -e

cd src
dotnet restore
dotnet publish --self-contained -c Release -r ubuntu.16.04-x64
dotnet publish --self-contained -c Release -r centos.7-x64
dotnet publish --self-contained -c Release -r debian.8-x64
dotnet publish --self-contained -c Release -r fedora.24-x64
dotnet publish --self-contained -c Release -r rhel.7-x64
dotnet publish --self-contained -c Release -r osx.10.12-x64
cd ..