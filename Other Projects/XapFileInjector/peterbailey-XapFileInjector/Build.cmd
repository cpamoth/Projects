echo Building...
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe" /v:q /p:PreBuildEvent= /p:Configuration=Release XapFileInjector.sln


echo Running Tests...
"Tools\NUnit\nunit-console-x86.exe" "XapFileInjector.Test\bin\release\XapFileInjector.Test.dll" /nologo /xml=Output\test-report.xml /framework=net-4.0

echo "Combining parts into single executable...
"Tools\ILMerge\ILMerge.exe" /out:.\Output\XapConfigInjector.exe XapFileInjector\bin\Release\XapFileInjector.exe XapFileInjector\bin\Release\Ionic.Zip.dll XapFileInjector\bin\Release\NDesk.Options.dll

echo Tidying up...
del ".\Output\*.pdb"

