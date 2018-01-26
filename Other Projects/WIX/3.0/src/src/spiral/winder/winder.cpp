/*-----------------------------------------------------------------------------
 winder.cpp - Creates Self-Extracting Executables
 Owner(s): scotk

 Created: 03/26/03
-----------------------------------------------------------------------------*/

#include "precomp.h"

#define WINDER_ID_LEN 32
#define SECTION_DELIM 128
#define ELEMENT_DELIM 129
#define STRING_DELIM 130

struct WINDER_FILE     
{
    WCHAR wzId[WINDER_ID_LEN]; // this is the name by which the file appears in the cab (it must be unique)
    WCHAR wzExtractPath[MAX_PATH];
    WCHAR wzSourcePath[MAX_PATH];
    WINDER_FILE *pwfNextFile;
};

struct WINDER_PACKAGE
{
    WCHAR wzExeStub[MAX_PATH];
    WCHAR wzExe[MAX_PATH];
    WINDER_FILE *pwfFileList;
};

struct CUTOFF_DATA
{
    LONGLONG llExtractSize;
};

/*-----------------------------------------------------------------------------
 ShowHelp - displays help message

------------------------------------------------------------------- scotk --*/
void ShowHelp()
{
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "winder - creates self-extracting exes\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "winder.exe [-c <spx file>]");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -?  this help information");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -c  create exe");
}


/*-----------------------------------------------------------------------------
 ParseCommandLine - returns the command line arguments in useful variables

------------------------------------------------------------------- scotk --*/
HRESULT ParseCommandLine(
    IN int argc, 
    IN WCHAR* argv[], 
    OUT BOOL* pfCreate,
    OUT LPWSTR* pwzSpxFile
    )
{
    HRESULT hr = S_FALSE;
    WCHAR *pwzArg;

    *pfCreate = FALSE;
    *pwzSpxFile = NULL;

    // skip the first arguemnt which is the program name
    for (int i = 1; i < argc; i++)
    {
        pwzArg = argv[i];

        // if we have a command line switch
        if ('/' == *pwzArg || '-' == *pwzArg)
        {
            // handle the switch
            pwzArg++;
            switch (*pwzArg)
            {
                case '?':   // help
                    hr = S_FALSE;
                    ExitFunction();
                case 'c':   // create an exe
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify exe to list from");
                    *pwzSpxFile = argv[i];

                    *pfCreate = TRUE;
                    hr = S_OK;
                    break;
                default:    // unknown
                    ExitOnFailure1(hr = E_INVALIDARG, "Unknown command line switch: %S", pwzArg);
            }
        }
        else  // it must be the database
        {
            ExitOnFailure(hr = E_INVALIDARG, "Unknown parameter on command line");
        }
    }

LExit: 
    return hr;
}


HRESULT ProcessFileElement(
    IN IXMLDOMNode* pixnPort, 
    OUT WINDER_FILE** ppwf
    )
{
    Assert(pixnPort && ppwf);

    HRESULT hr;
    BSTR bstr;

    WINDER_FILE *wcf = static_cast<WINDER_FILE *>(MemAlloc(sizeof(WINDER_FILE), TRUE));

    // File.Id
    hr = XmlGetAttribute(pixnPort, L"Id", &bstr);
    if (S_FALSE == hr || 0 == ::SysStringLen(bstr))
        hr = E_UNEXPECTED;
    ExitOnFailure(hr, "failed to get File.Id");
    lstrcpynW(wcf->wzId, bstr, countof(wcf->wzId));
    ReleaseNullBSTR(bstr);

    // File.SourcePath
    hr = XmlGetAttribute(pixnPort, L"SourcePath", &bstr);
    if (S_FALSE == hr || 0 == ::SysStringLen(bstr))
        hr = E_UNEXPECTED;
    ExitOnFailure(hr, "failed to get File.SourcePath");
    lstrcpynW(wcf->wzSourcePath, bstr, countof(wcf->wzExtractPath));
    ReleaseNullBSTR(bstr);

    // File.ExtractPath
    hr = XmlGetAttribute(pixnPort, L"RelativeExtractPath", &bstr);
    if (S_FALSE == hr || 0 == ::SysStringLen(bstr))
        hr = E_UNEXPECTED;
    ExitOnFailure(hr, "failed to get File.RelativeExtractPath");
    lstrcpynW(wcf->wzExtractPath, bstr, countof(wcf->wzExtractPath));
    ReleaseNullBSTR(bstr);

    *ppwf = wcf;

LExit:
    if (FAILED(hr))
    {
        if (wcf)
            MemFree(wcf);
    }

    return hr;
}


HRESULT LoadFromXml(
    IN IXMLDOMNode* pixnSpiral, 
    OUT WINDER_PACKAGE* pwp
    )
{
    Assert(pixnSpiral && pwp);

    HRESULT hr = S_OK;

    IXMLDOMNodeList* pixnl = NULL;
    IXMLDOMNode* pixn = NULL;

    WINDER_FILE *pwf = NULL;

    //
    // loop through the file elements
    //
    hr = pixnSpiral->selectNodes(L"File", &pixnl);
    if (S_FALSE == hr)
        ExitFunction();
    ExitOnFailure(hr, "failed to get Files from Spiral XML");

    while(S_OK == (hr = pixnl->nextNode(&pixn)))
    {
        hr = ProcessFileElement(pixn, &pwf);
        ExitOnFailure(hr, "failed to process Spiral File");

        pwf->pwfNextFile = pwp->pwfFileList;
        pwp->pwfFileList = pwf;
    }
    Assert(S_FALSE == hr);

    hr = S_OK;
LExit:
    return hr;
}


HRESULT ProcessXml(
    IN LPCWSTR wzXmlFile,
    OUT WINDER_PACKAGE *pwp
    )
{
    Assert(wzXmlFile && *wzXmlFile && pwp);

    HRESULT hr = S_OK;
    IXMLDOMDocument* pixd = NULL;
    IXMLDOMElement* pixeSpiral = NULL;

    BSTR bstr = NULL;

    hr = XmlLoadDocumentFromFile(wzXmlFile, &pixd);
    ExitOnFailure(hr, "failed to load document");

    //
    // ensure the root of the document is good
    //
    hr = pixd->get_documentElement(&pixeSpiral);
    ExitOnFailure(hr, "failed to get root Spiral element");

    hr = pixeSpiral->get_baseName(&bstr);
    ExitOnFailure(hr, "failed to get name of root Spiral element");

    if (0 != lstrcmpW(L"Spiral", bstr))
        ExitOnFailure1(hr = E_INVALIDARG, "invalid root element name: %S", bstr);
    ReleaseNullBSTR(bstr);

    //
    // get the package name
    //
    hr = XmlGetAttribute(pixeSpiral, L"Name", &bstr);
    if (S_OK == hr)
    {
        hr = StringCchCopyW(pwp->wzExe, countof(pwp->wzExe), bstr);
        ExitOnFailure1(hr, "failed to copy package name: %S", bstr);
        ReleaseNullBSTR(bstr);
    }
    else if (S_FALSE == hr)
        ConsoleExitOnFailure(hr = E_UNEXPECTED, CONSOLE_COLOR_RED, "Spiral.Name attribute not found");
    ExitOnFailure(hr, "error getting Spiral.Name attribute");

    //
    // get the stub name
    //
    hr = XmlGetAttribute(pixeSpiral, L"Stub", &bstr);
    if (S_OK == hr)
    {
        hr = StringCchCopyW(pwp->wzExeStub, countof(pwp->wzExeStub), bstr);
        ExitOnFailure1(hr, "failed to copy package stub name: %S", bstr);
    }
    else if (S_FALSE == hr)
    {
        // since they didn't specify a stub, assume cutoff.exe is sitting right next to us
        hr = StringCchCopyW(pwp->wzExeStub, countof(pwp->wzExeStub), L"cutoff.exe");
        ExitOnFailure(hr, "failed to copy default package stub name");
    }
    ExitOnFailure(hr, "invalid Jukebox.Overwrite attribute");

    //
    // process all the spiral element
    //
    hr = LoadFromXml(pixeSpiral, pwp);
    ExitOnFailure1(hr, "failed to process XML: %S", wzXmlFile);

LExit:
    ReleaseBSTR(bstr);
    ReleaseObject(pixeSpiral);
    ReleaseObject(pixd);

    return hr;
}


HRESULT CreateManifest(
    OUT LPWSTR *ppwzManifest,
    IN WINDER_PACKAGE* pwp,
    IN OUT CUTOFF_DATA* pcd
    )
{
    HRESULT hr = S_OK;
    WINDER_FILE *pwf = NULL;
    WCHAR wzStringDelim[2] = { STRING_DELIM, 0 };
    WCHAR wzElementDelim[2] = { ELEMENT_DELIM, 0 };
    WCHAR wzSectionDelim[2] = { SECTION_DELIM, 0 };
    LONGLONG llFileSize = 0;

    //
    // create file section
    //
    for (pwf = pwp->pwfFileList; pwf; pwf = pwf->pwfNextFile)
    {
        hr = FileSize(pwf->wzSourcePath, &llFileSize);
        ExitOnFailure1(hr, "failed to get size of file: %S", pwf->wzSourcePath);
        pcd->llExtractSize += llFileSize;

        hr = StrAllocConcat(ppwzManifest, pwf->wzId, 0);
        ExitOnFailure1(hr, "failed to concat file id: %S", pwf->wzId);

        hr = StrAllocConcat(ppwzManifest, wzStringDelim, 0);
        ExitOnFailure(hr, "failed to concat string delimeter");

        hr = StrAllocConcat(ppwzManifest, pwf->wzExtractPath, 0);
        ExitOnFailure1(hr, "failed to concat file extract path: %S", pwf->wzExtractPath);

        if (pwf->pwfNextFile)
        {
            hr = StrAllocConcat(ppwzManifest, wzElementDelim, 0);
            ExitOnFailure(hr, "failed to concat element delimeter");
        }
    }

LExit:
    return hr;
}

/****************************************************************************
 main - entry point for resourcerer

****************************************************************** scotk **/
extern "C" HRESULT __cdecl main()
{
    HRESULT hr = S_OK;
    UINT er = 0;
    BOOL fRet = TRUE;

    int argc;
    LPWSTR* pwzArgv;

    BOOL fCreate = FALSE;
    LPWSTR wzSpxFile = NULL;
    WINDER_PACKAGE *pwp = NULL;
    WINDER_FILE *pwf = NULL;
    CUTOFF_DATA cd;

    HMODULE hModule = NULL;
    HANDLE hUpdateRes = NULL;
    LPWSTR pwzCabPath = NULL;
    LPWSTR pwzCabDir = NULL;
    LPCWSTR wzCab = NULL;

    HANDLE hCab = NULL;

    LPBYTE pbData = NULL;
    DWORD cbData = 0;

    LPCBYTE pbRes = NULL;
    DWORD cbRes = 0;
    HANDLE hFile = INVALID_HANDLE_VALUE;
    HANDLE hRes = NULL;
    HRSRC hResInfo = NULL;

    LPWSTR pwzManifest = NULL;

    ::ZeroMemory(&cd, sizeof(CUTOFF_DATA));

    hr = ConsoleInitialize();
    ExitOnFailure(hr, "failed to initialize console");

    hr = XmlInitialize();
    ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to initialize XML");

    pwp = reinterpret_cast<WINDER_PACKAGE*>(MemAlloc(sizeof(WINDER_PACKAGE), TRUE));
    if (!pwp)
        ConsoleExitOnLastError(hr, CONSOLE_COLOR_RED, "failed to allocate winder package structure");
    
    //
    // process the command line
    //
    pwzArgv = ::CommandLineToArgvW(::GetCommandLineW(), &argc);
    if (NULL == pwzArgv)
        ConsoleExitOnFailure(hr = E_OUTOFMEMORY, CONSOLE_COLOR_RED, "failed to get command line");

    hr = ParseCommandLine(argc, pwzArgv, &fCreate, &wzSpxFile);
    ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to parse command line");

    if (S_FALSE == hr)
    {
        ShowHelp();
        ExitFunction();
    }

    // prevent the system from showing resolution dialogs
    ::SetErrorMode(SEM_FAILCRITICALERRORS);

    //
    // if we have an exe to create
    //
    if (fCreate)
    {
        hr = ProcessXml(wzSpxFile, pwp);
        ConsoleExitOnFailure1(hr, CONSOLE_COLOR_RED, "failed to process spiral xml file: %S", wzSpxFile);

        hr = FileCreateTemp(L"winder", L"cab", &pwzCabPath, NULL);
        ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to find a place to make cab");

        wzCab = FileFromPath(pwzCabPath);
        hr = StrAllocString(&pwzCabDir, pwzCabPath, wzCab - pwzCabPath);
        ConsoleExitOnFailure1(hr, CONSOLE_COLOR_RED, "failed to allocate string for extract path of: %S", pwzCabPath);

        //
        // create the cab
        //
        hr = CabCBegin(wzCab, pwzCabDir, 0, 0, COMPRESSION_TYPE_MSZIP, &hCab);
        ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to begin cabinet creation");

        for (pwf = pwp->pwfFileList; pwf; pwf = pwf->pwfNextFile)
        {
            ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Adding to package file: %S", pwf->wzSourcePath);
            hr = CabCAddFile(pwf->wzSourcePath, pwf->wzId, hCab);
            ConsoleExitOnFailure1(hr, CONSOLE_COLOR_RED, "failed to add to cabinet file: %S", pwf->wzSourcePath);
        }
        hr = CabCFinish(hCab);
        ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to finish cabinet");

        // Copy the stub
        if (!::CopyFileW(pwp->wzExeStub, pwp->wzExe, FALSE))
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "failed to make copy of exe stub: %S", pwp->wzExeStub);
        
        // Open the stub for updating
        hUpdateRes = ::BeginUpdateResourceW(pwp->wzExe, FALSE); 
        ConsoleExitOnNull1(hUpdateRes, hr, E_INVALIDARG, CONSOLE_COLOR_RED, "failed to open %S for resource updating", pwp->wzExe);

        //
        // Add the cab resource
        //
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Adding compressed files to extractor stub.");
        hFile = ::CreateFileW(pwzCabPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
        if (INVALID_HANDLE_VALUE == hFile)
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "failed to open file for reading: %S", pwzCabPath);

        // allocate a buffer for the cab resource
        cbData = ::GetFileSize(hFile, NULL);
        pbData = (BYTE*)MemAlloc(cbData, TRUE);
        ConsoleExitOnNull1(pbData, hr, E_OUTOFMEMORY, CONSOLE_COLOR_RED, "failed to allocate buffer for reading cab file: %S", pwzCabPath);

        if (!::ReadFile(hFile, pbData, cbData, &cbData, NULL))
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "failed to read from file %S", pwzCabPath);
        fRet = ::UpdateResourceW(hUpdateRes, /*RT_RCDATA*/MAKEINTRESOURCEW(10), L"TODO2", MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), pbData, cbData);
        if (fRet == FALSE) 
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "could not add cab resource to %S", pwp->wzExe);  

        //
        // Add the manifest resource
        //
        hr = CreateManifest(&pwzManifest, pwp, &cd);
        ConsoleExitOnFailure1(hr, CONSOLE_COLOR_RED, "failed to create manifest for project: %S", pwp->wzExe);

        fRet = ::UpdateResourceW(hUpdateRes, /*RT_RCDATA*/MAKEINTRESOURCEW(10), L"TODO", MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), reinterpret_cast<LPBYTE>(pwzManifest), ((lstrlenW(pwzManifest) + 1) * sizeof(WCHAR)));
        if (fRet == FALSE) 
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "could not add manifest resource to %S", pwp->wzExe);  

        //
        // Add the cutoff data resource
        //
        fRet = ::UpdateResourceW(hUpdateRes, /*RT_RCDATA*/MAKEINTRESOURCEW(10), L"TODO3", MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), reinterpret_cast<LPBYTE>(&cd), sizeof(CUTOFF_DATA));
        if (fRet == FALSE)
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "could not add binary data resource to %S", pwp->wzExe);

        // Write changes to file then close it
        if (!::EndUpdateResourceW(hUpdateRes, FALSE)) 
            ConsoleExitOnLastError1(hr, CONSOLE_COLOR_RED, "could not write changes to %S", pwp->wzExe);
    }

LExit:

    if (FAILED(hr))
        ConsoleWriteLine(CONSOLE_COLOR_RED, "err: 0x%x", hr);

    if (pwp)
        MemFree(pwp);

    ReleaseNullStr(pwzManifest);
    ReleaseNullStr(pwzCabPath);
    ReleaseNullStr(pwzCabDir);
    ConsoleUninitialize();
    
    if (hModule)
        ::FreeLibrary(hModule);
    if (pbData)
        MemFree(pbData);
    if (INVALID_HANDLE_VALUE != hFile)
        ::CloseHandle(hFile);

    return hr;
}


void __cdecl mainStartup()
{
    HRESULT hr = main();
    ::ExitProcess(hr);
}

