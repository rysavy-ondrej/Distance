#!/bin/bash
echo $USER
# make some files executable
chmod a+x /distance/artifacts/distance
chmod a+x /distance/build.sh

# add distance bin folder to PATH variable
echo 'export PATH=/distance/artifacts:$PATH' >> $HOME/.bash_profile

# Restore packages
dotnet restore /distance/Distance.sln

# Workaround to missing mapping of wlibpcap library in Linux:
cp /usr/lib64/libpcap.so.1 $HOME/.nuget/packages/sharppcap/4.5.0/lib/netstandard2.0/libwpcap.so
