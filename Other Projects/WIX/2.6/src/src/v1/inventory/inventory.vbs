'== inventory.vbs v1.0 == contact: t-davidl ==
'==================================

Option Explicit
'On Error Resume Next

Dim strOutput, strQuality, strRoot, strServer, strShare, strProvider, strNumber
Dim fso : Set fso = WScript.CreateObject("Scripting.FileSystemObject")


'----------------------------------------------------------------------------------------------------------
'-- Main --
if not ParseCommandLine then WScript.Quit 1
if not BundleRedistPackManifests then WScript.Quit 1
WScript.Echo "Created: " & strOutput & vbCrLf
WScript.Quit 0


'----------------------------------------------------------------------------------------------------------
'- ParseCommandLine - parses the command line
function ParseCommandLine
	Dim i, strArg
	ParseCommandLine = True
	if 1 > WScript.Arguments.Count or 7 < WScript.Arguments.Count then
		ShowHelp
		ParseCommandLine = False
	else
		' set the defaults
		strOutput = "manifest.xml"
		strQuality = "daily"
		strRoot = "."

		for i = 0 To  WScript.Arguments.Count - 1
			strArg = WScript.Arguments(i)

			if "-" = Left(strArg, 1) Or "/" = Left(strArg, 1) Then
				Select Case LCase(Mid(strArg, 2))
				Case "o"
					i = i + 1
					strOutput = LCase(WScript.Arguments(i))
				Case "q"
					i = i + 1
					strQuality = LCase(WScript.Arguments(i))
				Case "r"
					i = i + 1
					strRoot = LCase(WScript.Arguments(i))
				Case Else
					WScript.Echo "Unexpected switch: " & Mid(strArg, 2)
					ParseCommandLine = False 
					Exit Function
				End Select
			Else
				If Not IsEmpty(strServer) Then 
					WScript.Echo "Can only specify release server once" 
					ParseCommandLine = False 
					Exit Function
				End If

				strNumber = Right(strArg, Len(strArg) - InStrRev(strArg, "\"))
				strArg = Left(strArg, InStrRev(strArg, "\") - 1)
				strProvider = Right(strArg, Len(strArg) - InStrRev(strArg, "\"))
				strArg = Left(strArg, InStrRev(strArg, "\") - 1)
				strShare = Right(strArg, Len(strArg) - InStrRev(strArg, "\"))
				strServer = Left(strArg, InStrRev(strArg, "\") - 1)
			End If
		Next

		if strServer = "" or strShare = "" or strProvider = "" or strNumber = "" then ParseCommandLine = False

		if "daily" <> strQuality and "alpha" <> strQuality and "beta" <> strQuality and "rc" <> strQuality and "rtm" <> strQuality then
			WScript.Echo "Quality must be 'daily', 'alpha', 'beta', 'rc', or 'rtm'"
			ParseCommandLine = False
		End If
		If Not fso.FolderExists(strRoot) Then
			WScript.Echo "Cannot find root folder: " & strRoot
			ParseCommandLine = False
		end if
	end if
end function


'----------------------------------------------------------------------------------------------------------
'- ShowHelp - displays the help text
sub ShowHelp
	WScript.Echo "inventory.vbs [-o output] [-r root] \\server\share\provider\number" & vbCrLf & vbCrLf & _
	"  o  - output file ([default: 'manifest.xml'])" & vbCrLf & _
	"  q  - quality of the release (daily, alpha, beta, rc, rtm [default: 'daily'])" & vbCrLf & _
	"  r  - root folder to start inventory from [default: '.\']" & vbCrLf  & _
	"  \\server\share\provider\number - the server, share, provider, release number" & vbCrLf
end sub


'----------------------------------------------------------------------------------------------------------
'- BundleRedistPackManifests - bundles all the .redist files in the current dir tree into a manifest.xml file
function BundleRedistPackManifests
	BundleRedistPackManifests = True
	Dim folderDir
	Dim xmlDoc : Set xmlDoc = WScript.CreateObject("Msxml2.DOMDocument")
	Dim xmlEltRelease : Set xmlEltRelease = xmlDoc.createElement("Release")
	xmlDoc.appendChild(xmlEltRelease)
	xmlDoc.documentElement = xmlEltRelease

	xmlEltRelease.setAttribute "Server", strServer
	xmlEltRelease.setAttribute "Share", strShare
	xmlEltRelease.setAttribute "Provider", strProvider
	xmlEltRelease.setAttribute "Number", strNumber
	xmlEltRelease.setAttribute "Quality", strQuality

	Set folderDir = fso.getFolder(strRoot)
	if BundleRedistPackManifests then BundleRedistPackManifests = BundleRedistPacksInDir(xmlEltRelease, folderDir)

	if BundleRedistPackManifests then xmlDoc.save(strOutput)
end function


'----------------------------------------------------------------------------------------------------------
'- BundleRedistPacksInDir - adds all the .redist files in the current dir tree as children of xmlEltRelease
function BundleRedistPacksInDir(xmlEltRelease, folderDir)
	BundleRedistPacksInDir = True
	Dim fileRedistPack, subfolderDir

	for each fileRedistPack in folderDir.Files
		if BundleRedistPacksInDir then
			if (0 <> InStr(fileRedistPack.Name, ".redist")) then BundleRedistPacksInDir = AddRedistPack(xmlEltRelease, fileRedistPack.Path)
		end if
	next

	for each subfolderDir in folderDir.SubFolders
		if BundleRedistPacksInDir then
			BundleRedistPacksInDir = BundleRedistPacksInDir(xmlEltRelease, subfolderDir)
		end if
	next
end function


'----------------------------------------------------------------------------------------------------------
'- AddRedistPack - adds the .redist file as a child of xmlEltRelease
function AddRedistPack(xmlEltRelease, strFilePath)
	AddRedistPack = True
	Dim xmlDoc : Set xmlDoc = WScript.CreateObject("Msxml2.DOMDocument")
	xmlDoc.async = False
	xmlDoc.load strFilePath

	Dim xmlEltRedistPack : Set xmlEltRedistPack = xmlDoc.selectSingleNode("/RedistPack")
	Dim xmlTxtPackName : Set xmlTxtPackName = xmlDoc.createTextNode(Mid(strFilePath, InStrRev(strFilePath, "\") + 1, InStr(strFilePath, ".redist") - InStrRev(strFilePath, "\") - 1))
	Dim xmlEltSource : Set xmlEltSource = xmlDoc.createElement("Source")
	Dim xmlTxtSource : Set xmlTxtSource = xmlDoc.createTextNode(fso.GetFile(Left(strFilePath, InStr(strFilePath, ".redist") - 1)).Path)
	xmlEltRedistPack.appendChild xmlTxtPackName
	xmlEltSource.appendChild xmlTxtSource
	xmlEltRedistPack.appendChild xmlEltSource
	xmlEltRelease.appendChild xmlEltRedistPack
end function
