/*-----------------------------------------------------------------------------
 mandrel.cpp - Manipulates executable resources
 Owner(s): scotk

 Created: 12/31/02
-----------------------------------------------------------------------------*/

#include "precomp.h"

/*-----------------------------------------------------------------------------
 ShowHelp - displays help message

------------------------------------------------------------------- scotk --*/
void ShowHelp()
{
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "mandrel - manipulates exe resources\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "mandrel.exe [-l <file>] [-a <file> <resource file> <resource name>]");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   [-x <file> <xml file> <resource name>]\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   [-d <file> <resource name> <dump file>]\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -?  this help information");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -l  list the resources in an file");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -a  add an RCDATA resource to a file");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -x  add an XML file as an RCDATA resource to a file");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -d  dump resource to a file");
}


/*-----------------------------------------------------------------------------
 ParseCommandLine - returns the command line arguments in useful variables

------------------------------------------------------------------- scotk --*/
HRESULT ParseCommandLine(
    IN int argc, 
    IN WCHAR* argv[], 
    OUT BOOL* pfList,
    OUT BOOL* pfAdd,
    OUT BOOL* pfAddXml,
    OUT BOOL* pfDump,
    OUT LPWSTR* pwzFile,
    OUT LPWSTR* pwzRes,
    OUT LPWSTR* pwzResName
    )
{
    HRESULT hr = S_FALSE;
    WCHAR *pwzArg;

    *pfList = FALSE;
    *pfAdd = FALSE;
    *pfAddXml = FALSE;
    *pfDump = FALSE;
    *pwzFile = NULL;
    *pwzRes = NULL;
    *pwzResName = NULL;

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
                case 'l':   // list resources in exe
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify exe to list from");
                    *pwzFile = argv[i];

                    *pfList = TRUE;
                    hr = S_OK;
                    break;
                case 'a': // add a resource to an exe
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify exe to add to");
                    *pwzFile = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify file of resource to add");
                    *pwzRes = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify name of resource to add");
                    *pwzResName = argv[i];

                    *pfAdd = TRUE;
                    hr = S_OK;
                    break;
                case 'x': // add an XML resource to an exe
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify exe to add to");
                    *pwzFile = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify file of resource to add");
                    *pwzRes = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify name of resource to add");
                    *pwzResName = argv[i];

                    *pfAddXml = TRUE;
                    hr = S_OK;
                    break;
                case 'd': // dump a resource to a file
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify exe to dump from");
                    *pwzFile = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify name of resource to dump");
                    *pwzResName = argv[i];

                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify file to dump to");
                    *pwzRes = argv[i];

                    *pfDump = TRUE;
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

static BOOL EnumLangsCallback(
    HMODULE hModule,
    LPCWSTR lpType,
    LPCWSTR lpName,
    WORD wLang,
    LONG_PTR lParam
    )
{
    HRSRC hResInfo = NULL;
    WCHAR wzType[100];

    hResInfo = ::FindResourceExW(hModule, lpType, lpName, wLang);

    //
    // Start printing out all this info we have
    //
    if ((ULONG)lpName & 0xFFFF0000) 
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Name...: %S", lpName);
    else 
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Name...: %u", (USHORT)lpName);

    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "  Lang.: %d", MAKELCID(wLang, SORT_DEFAULT));
    
    if (hResInfo)
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "  Size.: %lu", SizeofResource(hModule, hResInfo));

    if ((ULONG)lpType & 0xFFFF0000) 
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "  Type.: %S\r\n", lpType);
    else 
    {
        switch((USHORT)lpType)
        {
        case RT_BITMAP:
            StringCchCopyW(wzType, countof(wzType), L"RT_BITMAP");
            break;
        case RT_DIALOG:
            StringCchCopyW(wzType, countof(wzType), L"RT_DIALOG");
            break;
        case RT_GROUP_ICON:
            StringCchCopyW(wzType, countof(wzType), L"RT_GROUP_ICON");
            break;
        case RT_ICON:
            StringCchCopyW(wzType, countof(wzType), L"RT_ICON");
            break;
        case RT_RCDATA:
            StringCchCopyW(wzType, countof(wzType), L"RT_RCDATA");
            break;
        case RT_STRING:
            StringCchCopyW(wzType, countof(wzType), L"RT_STRING");
            break;
        case RT_VERSION:
            StringCchCopyW(wzType, countof(wzType), L"RT_VERSION");
            break;
        default:
            StringCchPrintfW(wzType, countof(wzType), L"%u", (USHORT)lpType);
        }
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "  Type.: %S\r\n", wzType);
    }

    return TRUE;
}

static BOOL EnumNamesCallback(
    HMODULE hModule,
    LPCWSTR lpType,
    LPCWSTR lpName,
    LONG_PTR lParam
    )
{
    // Enumerate the LCIDs for this Name
    return ::EnumResourceLanguagesW(hModule, lpType, lpName, (ENUMRESLANGPROCW)EnumLangsCallback, NULL);
}

static BOOL EnumTypesCallback(
    HMODULE hModule,
    LPCWSTR lpType,
    LONG_PTR lParam
    )
{
    // Enumerate the names for this type
    return ::EnumResourceNamesW(hModule, lpType, (ENUMRESNAMEPROCW)EnumNamesCallback, NULL);
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

    BOOL fList = FALSE;
    BOOL fAdd = FALSE;
    BOOL fAddXml = FALSE;
    BOOL fDump = FALSE;
    LPWSTR wzFile = NULL;
    LPWSTR wzRes = NULL;
    LPWSTR wzResName = NULL;
    LPWSTR pwz = NULL;
    BSTR bstr = NULL;

    HMODULE hModule = NULL;
    HANDLE hUpdateRes = NULL;

    LPBYTE pbData = NULL;
    DWORD cbData = 0;
    LPCBYTE pbRes = NULL;
    DWORD cbRes = 0;
    HANDLE hFile = INVALID_HANDLE_VALUE;
    HANDLE hRes = NULL;
    HRSRC hResInfo = NULL;

    IXMLDOMDocument *pixd = NULL;

    hr = XmlInitialize();
    ExitOnFailure(hr, "failed to initialize XML");

    hr = ConsoleInitialize();
    ExitOnFailure(hr, "failed to initialize console");

    //
    // process the command line
    //
    pwzArgv = ::CommandLineToArgvW(::GetCommandLineW(), &argc);
    if (NULL == pwzArgv)
        ConsoleExitOnFailure(hr = E_OUTOFMEMORY, CONSOLE_COLOR_RED, "failed to get command line");

    hr = ParseCommandLine(argc, pwzArgv, &fList, &fAdd, &fAddXml, &fDump, &wzFile, &wzRes, &wzResName);
    ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to parse command line");

    if (S_FALSE == hr)
    {
        ShowHelp();
        ExitFunction();
    }

    // prevent the system from showing resolution dialogs
    ::SetErrorMode(SEM_FAILCRITICALERRORS);

    //
    // if we have a file to list the resources out of
    //
    if (fList)
    {
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Resources in %S\r\n", wzFile);

        hModule = ::LoadLibraryExW(wzFile, NULL,LOAD_LIBRARY_AS_DATAFILE| DONT_RESOLVE_DLL_REFERENCES);
        ExitOnNull(hModule, hr, HRESULT_FROM_WIN32(ERROR_BAD_EXE_FORMAT), "failed to load exe for resource reading");

        fRet = ::EnumResourceTypesW(hModule, (ENUMRESTYPEPROCW)EnumTypesCallback, NULL);
        if (!fRet)
            ExitOnLastError(hr, "failed to enumerate resource types");
    }

    //
    // if we have a resource to add
    //
    if (fAdd)
    {
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "\r\nAdding data from %S\r\n  to %S\r\n  as resource '%S'\r\n", wzRes, wzFile, wzResName);

        // open the source for the resource
        hFile = ::CreateFileW(wzRes, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
        if (INVALID_HANDLE_VALUE == hFile)
            ExitOnLastError1(hr, "failed to open file for reading: %S", wzRes);

        // allocate a buffer for the resource
        cbData = ::GetFileSize(hFile, NULL);
        pbData = (BYTE*)MemAlloc(cbData, TRUE);
        ExitOnNull1(pbData, hr, E_OUTOFMEMORY, "failed to allocate buffer for reading file: %S", wzRes);

        if (!::ReadFile(hFile, pbData, cbData, &cbData, NULL))
            ExitOnLastError1(hr, "failed to read from file %S", wzRes);

        // Open the file to which you want to add the resource 
        hUpdateRes = ::BeginUpdateResourceW(wzFile, FALSE); 
        ExitOnNull1(hUpdateRes, hr, E_INVALIDARG, "failed to open %S for updating", wzFile);
 
        // Add the resource to the update list 
        fRet = ::UpdateResourceW(hUpdateRes, /*RT_RCDATA*/MAKEINTRESOURCEW(10), wzResName, MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), pbData, cbData);
        if (fRet == FALSE) 
            ExitOnLastError1(hr, "could not add resource to %S", wzFile);  
 
        // Write changes to file then close it
        if (!::EndUpdateResourceW(hUpdateRes, FALSE)) 
            ExitOnLastError1(hr, "could not write changes to %S", wzFile); 
    }

    //
    // if we have an XML resource to add
    //
    if (fAddXml)
    {
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "\r\nAdding data from %S\r\n  to %S\r\n  as resource '%S'\r\n", wzRes, wzFile, wzResName);

        // Load the XML file
        hr = XmlLoadDocumentFromFile(wzRes, &pixd);
        ExitOnFailure1(hr, "failed to load xml file: %S", wzRes);

        pixd->get_xml(&bstr);

        // allocate a buffer for the resource
        cbData = ( lstrlenW(const_cast<LPCWSTR>(bstr)) + 1 ) * sizeof(WCHAR);
        pbData = (BYTE*)MemAlloc(cbData, TRUE);
        ExitOnNull1(pbData, hr, E_OUTOFMEMORY, "failed to allocate buffer for reading file: %S", wzRes);

        hr = StringCchCopyW(reinterpret_cast<LPWSTR>(pbData), cbData / sizeof(WCHAR), const_cast<LPCWSTR>(bstr));
        ExitOnFailure(hr, "failed to copy XML string to byte buffer");

        // Open the file to which you want to add the resource 
        hUpdateRes = ::BeginUpdateResourceW(wzFile, FALSE); 
        ExitOnNull1(hUpdateRes, hr, E_INVALIDARG, "failed to open %S for updating", wzFile);
 
        // Add the resource to the update list 
        fRet = ::UpdateResourceW(hUpdateRes, /*RT_RCDATA*/MAKEINTRESOURCEW(10), wzResName, MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), pbData, cbData);
        if (fRet == FALSE) 
            ExitOnLastError1(hr, "could not add resource to %S", wzFile);  
 
        // Write changes to file then close it
        if (!::EndUpdateResourceW(hUpdateRes, FALSE)) 
            ExitOnLastError1(hr, "could not write changes to %S", wzFile); 
    }

    //
    // if we are dumping a resource
    //
    if (fDump)
    {
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "\r\nDumping data from resource '%S'\r\n  in file %S\r\n  to file %S\r\n", wzResName, wzFile, wzRes);

        hModule = ::LoadLibraryExW(wzFile, NULL,LOAD_LIBRARY_AS_DATAFILE| DONT_RESOLVE_DLL_REFERENCES);
        ExitOnNull(hModule, hr, HRESULT_FROM_WIN32(ERROR_BAD_EXE_FORMAT), "failed to load exe for resource reading");

        hResInfo = ::FindResourceW(hModule, wzResName, /*RT_RCDATA*/MAKEINTRESOURCEW(10));
        ExitOnNull(hResInfo, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load resource info");

        hRes = ::LoadResource(hModule, hResInfo);
        ExitOnNull(hRes, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load resource");

        cbRes = ::SizeofResource(hModule, hResInfo);
        pbRes = (LPBYTE)::LockResource(hRes);

        // TODO: Change this to write to STDOUT instead of to a file
        hFile = ::CreateFileW(wzRes, GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
        if (INVALID_HANDLE_VALUE == hFile)
            ExitOnLastError1(hr, "failed to open file for writing: %S", wzRes);

        if (!::WriteFile(hFile, pbRes, cbRes, &cbData, NULL))  
            ExitOnLastError1(hr, "failed to write to file %S", wzRes);
    }

LExit:

    if (FAILED(hr))
        ConsoleWriteLine(CONSOLE_COLOR_RED, "err: 0x%x", hr);

    if (hModule)
        ::FreeLibrary(hModule);
    if (pbData)
        MemFree(pbData);
    if (INVALID_HANDLE_VALUE != hFile)
        ::CloseHandle(hFile);
    ReleaseObject(pixd);

    ConsoleUninitialize();
    XmlUninitialize();

    return hr;
}


void __cdecl mainStartup()
{
    HRESULT hr = main();
    ::ExitProcess(hr);
}

