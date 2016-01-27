dmcs IncVer.cs
mono IncVer.exe
xbuild nboard.csproj /p:Configuration="Release" 
zip nboard_release.zip bin/Release/nboard.exe bin/Release/places.txt bin/Release/styles/*.css bin/Release/categories.txt bin/Release/containers/dummy.png bin/Release/key.txt bin/Release/README.txt
git add App.cs
git add nboard_release.zip
git add version
git commit -m 'Upload new release'
git push -u origin master
