dotnet --list-sdks

dotnet build Distance.sln

# copy output from projects to artifacts folder:
cp -Force DistanceEngine/bin/Debug/netcoreapp2.1/* artifacts

# copy output from projects to artifacts folder:
cp -Force Profiles/Diagnostics.Soho/bin/Debug/netstandard2.0/Diagnostics.Soho.dll artifacts/


# OS specific actions
if($IsLinux) {
    # Workaround to missing mapping of wlibpcap libary in Linux:
    cp -Force  /usr/lib64/libpcap.so.1 ~/.nuget/packages/sharppcap/4.5.0/lib/netstandard2.0/libwpcap.so
 }

