$Env:DISTANCE_PROFILE +=".\Profiles\Diagnostics.Soho\bin\Debug\netstandard2.0"
dotnet .\DistanceEngine\bin\Debug\netcoreapp2.1\DistanceEngine.dll run -profile Diagnostics.Soho $args
