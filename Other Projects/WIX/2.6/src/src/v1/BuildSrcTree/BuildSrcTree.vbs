'<script Language='VBScript'> -- provides syntax highlighting in VId
' 
' BuildSrcTree.vbs
'
'   Reads directory and file information from components specified in a wiX file (in XML), 
' copy each file from "src" location to the correct location, which is the "source" location referenced
' by MSI during setup.  Corresponding directory structure will be created
'
' By: Charles Wu (cwu)  07-31-02

Option Explicit

Const NODE_ELEMENT                = 1

Dim g_sSrcRoot
Dim g_sTargetRoot
Dim g_bPrompt, g_bVerbose, g_bCleanBuild
Dim g_cFile, g_cbFile
Dim g_sInputFile
Dim g_bModule
Dim g_oFSO, g_oShell

g_sSrcroot = ""
g_sTargetRoot = ""
g_sInputFile = ""
g_bPrompt = false
g_bModule = false
g_bVerbose = false
g_bCleanBuild = false
g_cFile = 0
g_cbFile = 0

set g_oFSO = WScript.CreateObject("Scripting.FileSystemObject")
set g_oShell = Wscript.CreateObject("Wscript.Shell")

'
' =========== Main Execution Point ========
'
ProcessCommandArguments

'
' open the XML file 
'
Dim xmlDoc : Set xmlDoc = WScript.CreateObject("Microsoft.XMLDOM")

xmlDoc.preserveWhiteSpace = False
xmlDoc.async = False

Dim bLoaded :  bLoaded = xmlDoc.load( g_sInputFile )
If Not bLoaded Then
    Dim pe :  Set pe = xmlDoc.parseError
    Dim sErr : sErr = "Failed to load XML file: " & pe.url & vbCrLf & "   " & pe.errorCode & " - " & pe.reason & vbCrLf & "   Line:" & pe.line & ", Character: " & pe.linepos
    Fail pe.line, pe.errorCode, sErr
End If

'
' read the XML file
'
Dim root
Set root = xmlDoc.documentElement
If root.baseName = "Product" Then
    g_bModule = false
Else
    g_bModule = true
End If

'
' search for any <Directory> elements
'
Dim dir, dirs
set dirs = root.selectNodes( "Directory" )

For each dir in dirs   
    Dim sTargetPath
    sTargetPath = g_sTargetRoot
    ProcessDirectoryElement dir, sTargetPath
Next

WScript.Echo g_cbFile & " bytes in " & g_cFile & " files were copied"
WScript.Quit 0

'====================================
' Process a directory element
'====================================
Sub ProcessDirectoryElement( dir, sTargetPath )
    Dim attribute, value, name, longName, sourceName, longSource, child, dirName, s, sNewTargetPath

    dirName = ElementText( dir )

    For Each attribute In dir.Attributes
        value = attribute.value
        Select Case(attribute.name)
        Case "Name"       : name       = value
        Case "LongName"   : longName   = value
        Case "SourceName" : sourceName = value
        Case "LongSource" : longSource = value
        End Select
    Next

    If Len( longSource ) = 0 And Len( SourceName ) <> 0 Then longSource = SourceName
    If Len( longName ) = 0 And Len( Name ) <> 0 Then longName = Name

    If Len( longSource ) = 0 Then longSource = longName

    If Len( longSource ) = 0 Then
        Fail 0, 1011, "Directory element must specify a name"
    End If

    if sTargetPath = g_sTargetRoot Then
        if dirName <> "TARGETDIR" Then
            Fail 0, 1010, "First directory element in the input file must be for TARGETDIR"
        End If

        ' name of first directory element must be SourceDir
        If longSource <> "SourceDir" Then
            Fail 0, 1012, "First directory element in the input file must has name = 'SourceDir'"
        End If

        if Len( g_sTargetRoot ) <> 0 Then
            sNewTargetPath = g_sTargetRoot & "\."
        else
            sNewTargetPath = "."
        End If
    Else
        ' concat. the source & destination path
        sNewTargetPath = sTargetPath & "\" & longSource

        If g_bVerbose Then
            WScript.Echo "Found directory " & sNewTargetPath
        End If

        If g_bCleanBuild and g_oFSO.FolderExists( sNewTargetPath ) Then
            if g_bPrompt Then
                WScript.StdOut.Write "About to remove " & sNewTargetPath & " and all it's subdirectories. Ok to continue? (Y/N) "
                s = WScript.StdIn.ReadLine
                if AscW( UCase( s ) ) <> AscW( "Y" ) Then
                    Fail 0, 1013, "Terminated by user." 
                End If
            End If

            g_oFSO.DeleteFolder sNewTargetPath, true
            If g_bVerbose Then
                WScript.Echo "Directory " & sNewTargetPath & " and all of its subdirectories were deleted"
            End If
        End If

        If not g_oFSO.FolderExists( sNewTargetPath ) Then
            if g_bPrompt Then
                WScript.StdOut.Write "About to create " & sNewTargetPath & ". Ok to continue? (Y/N) "
                s = WScript.StdIn.ReadLine
                if AscW( UCase( s ) ) <> AscW( "Y" ) Then
                    Fail 0, 1013, "Terminated by user." 
                End If
            End If

            g_oFSO.CreateFolder( sNewTargetPath )

            If g_bVerbose Then
                WScript.Echo "Created directory " & sNewTargetPath
            End If
        End If
    End If

    For Each child In dir.childNodes
        Select Case (GetElementName(child))
        Case "Directory" : ProcessDirectoryElement child, sNewTargetPath
        Case "Component" : ProcessComponentElement child, sNewTargetPath
        End Select
    Next

End Sub

'====================================
' Process a component element
'====================================
Sub ProcessComponentElement( elem, sTargetPath )
    Dim child

    'only interested in File elements
    For Each child In elem.childNodes
        If GetElementName(child) = "File" Then
            ProcessFileElement child, sTargetPath
        End If
    Next
End Sub

'====================================
' Process a File element
'====================================
Sub ProcessFileElement( elem, sTargetPath )
    Dim child, attribute, value, src, name, longName, sourceName, longSource, compressed, sSrcFile, sDstFile, fileSrc, fileDst, ch1, ch2

    For Each attribute In elem.Attributes
        value = attribute.value
        Select Case(attribute.name)
        Case "src"        : src = value
        Case "Name"       : name       = value
        Case "LongName"   : longName   = value
        Case "SourceName" : sourceName = value
        Case "LongSource" : longSource = value
        Case "Compressed" : compressed = value
        End Select
    Next

    If Len( longSource ) = 0 And Len( SourceName ) <> 0 Then longSource = SourceName
    If Len( longName ) = 0 And Len( Name ) <> 0 Then longName = Name

    If Len( longSource ) = 0 Then longSource = longName

    If Len( longSource ) = 0 Then
        Fail 0, 1030, "File element must specify a name"
    End If

    if Len( src ) = 0 Then
        Fail 0, 1031, "File element must specify a source"
    End If

    if Len( compressed ) <> 0 and compressed <> "no" Then
        Exit Sub
    End If

    sSrcFile = g_oShell.ExpandEnvironmentStrings( src )

    If Len( g_sSrcRoot ) <> 0 Then
        ' see if src is a relative path
        ch1 = Mid( sSrcFile, 1, 1 )
        ch2 = Mid( sSrcFile, 2, 1 )

        If  ch1 = "." and ( ch2 = "." or ch2 = "\" ) or not ( ch1 = "\" and ch2 = "\" or ch2 = ":" ) Then
            sSrcFile = g_sSrcRoot & "\" & sSrcFile
        End If
    End If

    If g_bVerbose Then
        WScript.Echo "Found source file " & sSrcFile
    End If

    If not g_oFSO.FileExists( sSrcFile ) Then
        Fail 0, 1032, sSrcFile & " does not exists." 
    End If

    sDstFile = sTargetPath & "\" & longSource

    'make sure target file is not write-protected
    if g_oFSO.FileExists( sDstFile ) Then
        set fileDst = g_oFSO.GetFile( sDstFile ) 
        fileDst.attributes = 0
    End If

    g_oFSO.CopyFile sSrcFile, sDstFile, true 

    If g_bVerbose Then
        WScript.Echo "Copied to target file " & sDstFile
    End If

    set fileSrc = g_oFSO.GetFile( sSrcFile ) 
    set fileDst = g_oFSO.GetFile( sDstFile ) 
    fileDst.attributes = fileSrc.attributes

    g_cbFile = g_cbFile + fileSrc.Size
    g_cFile = g_cFile + 1
End Sub

'====================================
' Process command line arguments
'====================================
Sub ProcessCommandArguments
    Dim oArgs : set oArgs = WScript.Arguments

    If oArgs.Count = 0 Then
        ShowUsage
        WScript.Quit 0
    End If

    Dim ix, arg, chFlag

    For ix = 0 to oArgs.Count - 1
        arg = oArgs.Item( ix ) 
        chFlag = AscW( arg )
        If chFlag = AscW( "-" ) or chFlag = AscW( "/" ) Then
    		chFlag = UCase(Right(arg, Len(arg)-1))
            Select Case chFlag
                case "?" 
                     ShowUsage
                     WScript.Quit 0

                case "T"
                    If ix >= oArgs.Count - 1 Then
                        Fail 0, 1000, "Missing parameters to the -" & chFlag & " argument"
                    End If
                    ix = ix + 1
                    g_sTargetRoot = oArgs.Item(ix)

                case "S"
                    If ix >= oArgs.Count - 1 Then
                        Fail 0, 1001, "Missing parameters to the -" & chFlag & " argument"
                    End If
                    ix = ix + 1
                    g_sSrcRoot = g_oShell.ExpandEnvironmentStrings( oArgs.Item(ix) )

                case "P"
                    g_bPrompt = true

                case "V"
                    g_bVerbose = true

                case "C"
                    g_bCleanBuild = true

            End Select
        Else
            If Len( g_sInputFile ) <> 0 Then
                Fail 0, 1002, "input XML file was specified more than once"
            End If
            g_sInputFile = arg
        End If
    Next

    If Len( g_sInputFile ) = 0 Then
        Fail 0, 1003, "Need to specify an input XML file"
    End If
End Sub

'====================================
' Show help and usage information
'====================================
Sub ShowUsage
    WScript.Echo "BuildSrcTree - a tool for Windows Installer XML (wiX)"
    WScript.Echo "      It traverses all <Directory> and <File> elements within and copy each"
    WScript.Echo "file from location specified in 'src' to target location specified in the"
    WScript.Echo "<Directory> element."
    WScript.Echo ""
    WScript.Echo "BuildSrcTree.vbs [-?] [-t root_dir] [-p] xml_file"
    WScript.Echo ""
    WScript.Echo "-?            This help information"
    WScript.Echo "-p            Prompt before creating a new directory or removing directory"
    WScript.Echo "xml_file      A XML file using wiX schema (eg. .wxp or .wxm files)"
    WScript.Echo "-t root_dir   root directory where files are copied to. Default is ."
    WScript.Echo "-v            Verbose mode."
    WScript.Echo "-c            Clean build, remove target directories first."
    WScript.Echo ""
    WScript.Echo "Example"
    WScript.Echo "  BuildSrcTree.vbs -s c:\Depot -t ""c:\output\Program Files"" foo.wxp"
    WScript.Echo "      Above command reads foo.wsp and looks for <File> elements withint a "
    WScript.Echo "<Directory> element. It copies each file from c:\Depot\... to "
    WScript.Echo "c:\output\Program Files\..."
End Sub

Function GetElementName(node)
    GetElementName = Empty
    If node.nodeType = NODE_ELEMENT Then GetElementName = node.nodeName
End Function

Function ElementText(node)
    If node Is Nothing Then Fail 0, 2050, "passed dead node to ElementText"
    Dim child : Set child = node.selectSingleNode("text()")
    If child Is Nothing Then Fail 0, 2051, "Missing text value for element: " & node.nodeName
    ElementText = child.text
End Function

'====================================
' Unexpected element or attribute was found
'====================================
Sub Unexpected(child, parent)
    Fail 0, 5000, "Unexpected " & child.nodeTypeString & " node: " & child.nodeName & ", parent = " & parent.nodeName
End Sub

'====================================
' Show error message and terminate
'====================================
Sub Fail(nLine, nErr, sMsg)
	Dim sLine
	If 0 < nLine Then sLine = "(" & nLine & ")"
	WScript.Echo "BuildSrcTree.vbs" & sLine & " : ERROR " & nErr & ": " & sMsg
	WScript.Quit nErr
End Sub   ' Fail

