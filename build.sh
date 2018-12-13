# build all projects using dotnet:
dotnet --list-sdks
dotnet build Distance.sln

# copy output from projects to artifacts folder:
cp -a DistanceEngine/bin/Debug/netcoreapp2.0/* artifacts
