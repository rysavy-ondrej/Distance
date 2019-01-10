# build all projects using dotnet:
dotnet --list-sdks

dotnet build Distance.sln

# copy output from projects to artifacts folder:
cp -a DistanceEngine/bin/Debug/netcoreapp2.1/* artifacts

# copy output from projects to artifacts folder:
cp -a Profiles/Diagnostics.Soho/bin/Debug/netstandard2.0/Diagnostics.Soho.dll artifacts/


# Workaround to missing mapping of wlibpcap libary in Linux:
cp /usr/lib64/libpcap.so.1 ~/.nuget/packages/sharppcap/4.5.0/lib/netstandard2.0/libwpcap.so
