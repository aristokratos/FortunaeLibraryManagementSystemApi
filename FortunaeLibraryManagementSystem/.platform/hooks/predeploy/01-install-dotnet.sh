#!/bin/bash
echo "Installing .NET Core Runtime..."
yum update -y
amazon-linux-extras enable dotnet6
yum install -y dotnet-sdk-6.0

