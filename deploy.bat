csc IncVer.cs
IncVer.exe
msbuild nboard.csproj /p:Configuration=Release
jar -cMf nboard_release.zip bin/Release/js/weppy.js bin/Release/allowed.txt bin/Release/js/jquery.min.js bin/Release/js/jquery-ui.min.css bin/Release/js/jquery-ui.min.js bin/Release/spamfilter.txt bin/Release/nboard.exe bin/Release/places.txt bin/Release/styles/*.css bin/Release/categories.txt bin/Release/containers/dummy.png bin/Release/key.txt bin/Release/README.txt
git add App.cs
git add nboard_release.zip
git add version
git commit -m "Upload new release"
git push -u origin master
