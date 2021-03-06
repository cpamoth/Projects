REGEDIT4

####################################################################################################
# File Associations
####################################################################################################

[HKEY_CLASSES_ROOT\.wixproj]
@="WindowsInstallerXml.wixprojfile.3.0"
"Content Type"="text/plain"
[HKEY_CLASSES_ROOT\.wixproj\OpenWithList\devenv.exe]
[HKEY_CLASSES_ROOT\.wixproj\OpenWithProgids]
"WindowsInstallerXml.wixprojfile.3.0"=""
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixprojfile.3.0]
@="WiX Project File"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixprojfile.3.0\DefaultIcon]
@="%DLLPATH%,0"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixprojfile.3.0\shell\Open]
@="&Open in Visual Studio %VS_VERSION_YEAR%"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixprojfile.3.0\shell\Open\command]
@="\"%DEVENVPATH%\" \"%1\""

[HKEY_CLASSES_ROOT\.wxs]
@="WindowsInstallerXml.wxsfile.3.0"
"Content Type"="text/xml"
"PerceivedType"="text"
[HKEY_CLASSES_ROOT\.wxs\OpenWithList\devenv.exe]
[HKEY_CLASSES_ROOT\.wxs\OpenWithProgids]
"WindowsInstallerXml.wxsfile.3.0"=""
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxsfile.3.0]
@="WiX Source File"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxsfile.3.0\DefaultIcon]
@="%DLLPATH%,1"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxsfile.3.0\shell\Open]
@="&Open in Visual Studio %VS_VERSION_YEAR%"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxsfile.3.0\shell\Open\command]
@="\"%DEVENVPATH%\" \"%1\""

[HKEY_CLASSES_ROOT\.wxi]
@="WindowsInstallerXml.wxifile.3.0"
"Content Type"="text/xml"
"PerceivedType"="text"
[HKEY_CLASSES_ROOT\.wxi\OpenWithList\devenv.exe]
[HKEY_CLASSES_ROOT\.wxi\OpenWithProgids]
"WindowsInstallerXml.wxifile.3.0"=""
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxifile.3.0]
@="WiX Include File"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxifile.3.0\DefaultIcon]
@="%DLLPATH%,2"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxifile.3.0\shell\Open]
@="&Open in Visual Studio %VS_VERSION_YEAR%"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxifile.3.0\shell\Open\command]
@="\"%DEVENVPATH%\" \"%1\""

[HKEY_CLASSES_ROOT\.wxl]
@="WindowsInstallerXml.wxlfile.3.0"
"Content Type"="text/xml"
"PerceivedType"="text"
[HKEY_CLASSES_ROOT\.wxl\OpenWithList\devenv.exe]
[HKEY_CLASSES_ROOT\.wxl\OpenWithProgids]
"WindowsInstallerXml.wxlfile.3.0"=""
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxlfile.3.0]
@="WiX Localization File"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxlfile.3.0\DefaultIcon]
@="%DLLPATH%,3"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxlfile.3.0\shell\Open]
@="&Open in Visual Studio %VS_VERSION_YEAR%"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wxlfile.3.0\shell\Open\command]
@="\"%DEVENVPATH%\" \"%1\""

[HKEY_CLASSES_ROOT\.wixlib]
@="WindowsInstallerXml.wixlibfile.3.0"
"Content Type"="text/xml"
"PerceivedType"="text"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixlibfile.3.0]
@="WiX Library File"
[HKEY_CLASSES_ROOT\WindowsInstallerXml.wixlibfile.3.0\DefaultIcon]
@="%DLLPATH%,4"

####################################################################################################
# Visual Studio Registration
####################################################################################################

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\Editors\%XML_EDITOR_GUID%\Extensions]
"wxs"=dword:00000028
"wxi"=dword:00000028
"wxl"=dword:00000028

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\InstalledProducts\WiX]
"Package"="%PACKAGE_GUID%"
"ToolsDirectory"="%WIXTOOLSDIR%"
"UseInterface"=dword:00000001

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\NewProjectTemplates\TemplateDirs\%PACKAGE_GUID%\/1]
@="WiX"
"SortPriority"=dword:0000001e
"TemplatesDir"="%PROJECTTEMPLATESDIR%"

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\Packages\%PACKAGE_GUID%]
@="WiX Project Package"
"Assembly"=""
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.WixPackage"
"CodeBase"="%DLLPATH%"
"CompanyName"="Microsoft"
"ID"=dword:00000096
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"MinEdition"="Standard"
"ProductName"="Votive"
"ProductVersion"="3.0"

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\Projects\%PROJECT_GUID%]
@="WixProjectFactory"
"DefaultProjectExtension"="wixproj"
"DisplayName"="WiX"
"DisplayProjectFileExtensions"="#100"
"ItemTemplatesDir"="%ITEMTEMPLATESDIR%"
"Language(VsTemplate)"="WiX"
"Package"="%PACKAGE_GUID%"
"ProjectTemplatesDir"="%PROJECTTEMPLATESDIR%"
"PossibleProjectExtensions"="wixproj"

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\%VS_VERSION%Exp\Projects\%PROJECT_GUID%\Filters\WiX Files]
@="#101"
"CommonOpenFilesFilter"=dword:00000001
"SortPriority"=dword:000003e8

####################################################################################################
# Property Pages
####################################################################################################

[HKEY_LOCAL_MACHINE\Software\Microsoft\VisualStudio\8.0Exp\CLSID\{3C50BD5E-0E85-4306-A0A8-5B39CCB07DA0}]
@="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixBuildPropertyPage"
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixBuildPropertyPage"
"CodeBase"="%DLLPATH%"
"ThreadingModel"="Both"

[HKEY_LOCAL_MACHINE\Software\Microsoft\VisualStudio\8.0Exp\CLSID\{6D7F1842-14C0-4697-9AE6-0B777D1F5C65}]
@="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixCompilerPropertyPage"
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixCompilerPropertyPage"
"CodeBase"="%DLLPATH%"
"ThreadingModel"="Both"

[HKEY_LOCAL_MACHINE\Software\Microsoft\VisualStudio\8.0Exp\CLSID\{1D7B7FA7-4D01-4112-8972-F287E9DC206A}]
@="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixLinkerPropertyPage"
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixLinkerPropertyPage"
"CodeBase"="%DLLPATH%"
"ThreadingModel"="Both"

[HKEY_LOCAL_MACHINE\Software\Microsoft\VisualStudio\8.0Exp\CLSID\{6CE92892-70C4-4385-87F4-627E1E04CA66}]
@="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixLibrarianPropertyPage"
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixLibrarianPropertyPage"
"CodeBase"="%DLLPATH%"
"ThreadingModel"="Both"

[HKEY_LOCAL_MACHINE\Software\Microsoft\VisualStudio\8.0Exp\CLSID\{A71983CF-33B9-4241-9B5A-80091BCE57DA}]
@="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixBuildEventsPropertyPage"
"InprocServer32"="%SYSDIR%\\mscoree.dll"
"Class"="Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages.WixBuildEventsPropertyPage"
"CodeBase"="%DLLPATH%"
"ThreadingModel"="Both"
