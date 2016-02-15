rm -r -f release
mkdir release
cd release
cp ../bin/Debug/*.sh .
cp ../bin/Debug/*.bat .
dmcs ../tools/aggregator/Aggregator.cs
mv ../tools/aggregator/Aggregator.exe .
cp ../tools/aggregator/places.txt .
xbuild ../nboard2.csproj
mv ../bin/Debug/nanodb.exe .
cp ../bin/Debug/Newtonsoft.Json.dll .
xbuild ../tools/nbpack/nbpack.csproj
mv ../tools/nbpack/bin/Debug/nbpack.exe .
cp -r ../bin/Debug/pages .
cp -r ../bin/Debug/images .
cp -r ../bin/Debug/scripts .
