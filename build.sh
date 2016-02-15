rm -r -f release
mkdir release
cd release
dmcs ../tools/aggregator/Aggregator.cs
mv ../tools/aggregator/Aggregator.exe .
xbuild ../nboard2.csproj
mv ../bin/Debug/nanodb.exe .
cp ../bin/Debug/Newtonsoft.Json.dll .
xbuild ../tools/nbpack/nbpack.csproj
mv ../tools/nbpack/bin/Debug/nbpack.exe .

