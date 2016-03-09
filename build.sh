#backup dev local db:
mv release/downloaded.txt .
mv release/places.txt .
mv release/*.db .
mv release/index.json .
mv release/diff.list .
rm -r -f release
mkdir release
cd release
cp ../bin/Debug/run.sh .
#cp ../bin/Debug/*.sh .
#cp ../bin/Debug/*.bat .
#dmcs ../tools/aggregator/Aggregator.cs
#mv ../tools/aggregator/Aggregator.exe .
cp ../tools/aggregator/places.txt .
xbuild ../nanodb.csproj
mv ../bin/Debug/nanodb.exe .
cp ../bin/Debug/Newtonsoft.Json.dll .
cp ../README.md README.txt
#xbuild ../tools/nbpack/nbpack.csproj
#mv ../tools/nbpack/bin/Debug/nbpack.exe .
date > ../bin/Debug/pages/version.txt
cp -R ../bin/Debug/pages .
cp -R ../bin/Debug/images .
cp -R ../bin/Debug/scripts .
cp -R ../bin/Debug/styles .
cp -R ../bin/Debug/fonts .
cd ..
rm release2.zip
zip -r -9 release2.zip release/*
git add release2.zip
git add bin/Debug/pages/version.txt
git commit -m 'Upload 2.x release'
git push -u origin feature/2.0
#restore dev local db:
mv *.db release
mv index.json release
mv diff.list release
mv downloaded.txt release
mv places.txt release
