/*-----------------------------------------------------------------------------
 cutoff.cpp - Spiral Self-Extractor Stub
 Owner(s): scotk

 Created: 12/31/02
-----------------------------------------------------------------------------*/

#include "precomp.h"

#define SECTION_DELIM 128
#define ELEMENT_DELIM 129
#define STRING_DELIM 130
#define CUTOFF_ID_LEN 32

struct CUTOFF_FILE     
{
    WCHAR wzId[CUTOFF_ID_LEN]; // this is the name by which the file appears in the cab (it must be unique)
    WCHAR wzExtractPath[MAX_PATH];
    CUTOFF_FILE *pcfNextFile;
};

struct CUTOFF_DATA
{
    LONGLONG llExtractedSize;
};

BOOL vfCanceled = FALSE;
HWND vhExtractDialog = NULL;
CUTOFF_DATA* vpcd;


/*-----------------------------------------------------------------------------
 ShowHelp - displays help message

------------------------------------------------------------------- scotk --*/
void ShowHelp()
{
    // TODO: replace this with a dialog
    /*ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "cutoff - Spiral Self-Extractor Stub\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "cutoff.exe [-q] [-e]");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -?  this help information");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -q  quiet extraction");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -e  extract only");*/
}


/*-----------------------------------------------------------------------------
 ParseCommandLine - returns the command line arguments in useful variables

------------------------------------------------------------------- scotk --*/
static HRESULT ParseCommandLine(
    IN int argc, 
    IN WCHAR* argv[], 
    OUT BOOL* pfQuiet,
    OUT BOOL* pfExtractOnly
    )
{
    HRESULT hr = S_OK;
    WCHAR *pwzArg;

    *pfQuiet = FALSE;
    *pfExtractOnly = FALSE;

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
                case 'q':   // quiet
                    *pfQuiet = TRUE;
                    hr = S_OK;
                    break;
                case 'e': // extract only
                    *pfExtractOnly = TRUE;
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


LPWSTR BreakDownData(LPWSTR* ppwzData, WCHAR wcDelim)
{
    Assert(ppwzData);

    if (0 == *ppwzData)
        return NULL;

    LPWSTR pwzReturn = *ppwzData;
    LPWSTR pwz = wcschr(pwzReturn, wcDelim);
    if (pwz)
    {
        *pwz = 0;
        *ppwzData = pwz + 1;
    }
    else
        *ppwzData = 0;

    return pwzReturn;
}


HRESULT ProcessFileElement(
    IN LPWSTR *ppwzFileElement,
    IN OUT CUTOFF_FILE** ppcfFileList
    )
{
    HRESULT hr = S_OK;
    LPWSTR pwz = NULL;
    CUTOFF_FILE *pcf = NULL;

    pcf = static_cast<CUTOFF_FILE*>(MemAlloc(sizeof(CUTOFF_FILE), TRUE));
    ExitOnNull(pcf, hr, E_OUTOFMEMORY, "failed to allocate cutoff file element");

    // process file id
    pwz = BreakDownData(ppwzFileElement, STRING_DELIM);
    ExitOnNull(pwz, hr, E_UNEXPECTED, "file element has no id");
    hr = StringCchCopyW(pcf->wzId, countof(pcf->wzId), pwz);
    ExitOnFailure1(hr, "failed to copy file id: %S", pwz);

    // process file extract path
    pwz = BreakDownData(ppwzFileElement, STRING_DELIM);
    ExitOnNull(pwz, hr, E_UNEXPECTED, "file element has no extract path");
    hr = StringCchCopyW(pcf->wzExtractPath, countof(pcf->wzExtractPath), pwz);
    ExitOnFailure1(hr, "failed to copy file extract path: %S", pwz);

    // the new file to the front of the list
    pcf->pcfNextFile = *ppcfFileList;
    *ppcfFileList = pcf;

    pcf = NULL; // don't want to free it since it was successfully added

LExit:
    if (FAILED(hr) && pcf)
        MemFree(pcf);

    return hr;
}


HRESULT ProcessData(
    IN LPWSTR *ppwzData,
    OUT CUTOFF_FILE** ppcfFileList
    )
{
    Assert(ppwzData && *ppwzData && **ppwzData && ppcfFileList);

    HRESULT hr = S_OK;
    LPWSTR pwzSection = NULL;
    LPWSTR pwzElement = NULL;
    DWORD dwCurrentSection = 0;

    while (NULL != (pwzSection = BreakDownData(ppwzData, SECTION_DELIM)))
    {
        while (NULL != (pwzElement = BreakDownData(&pwzSection, ELEMENT_DELIM)))
        {
            switch (dwCurrentSection)
            {
                case 0: // file section first
                    hr = ProcessFileElement(&pwzElement, ppcfFileList);
                    ExitOnFailure1(hr, "failed to process file element: %S", pwzElement);
                    break;
                default:
                    AssertSz(FALSE, "too many sections");
                    ExitOnFailure(hr = E_UNEXPECTED, "too many sections");
                    break;
            }
        }
        dwCurrentSection++;
    }

LExit:

    return hr;
}

HRESULT ExtractProgressCallback(BOOL fBeginFile, LPCWSTR wzFileId, LPVOID pvContext)
{
    HRESULT hr = S_OK;

    if (vfCanceled)
        ExitOnFailure(hr = E_ABORT, "user canceled extraction");

    if (fBeginFile)
    {
        //hr = ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Extracting %S...", wzFileId);
        //ExitOnFailure1(hr, "failed to print to console: %S", wzFileId);
    }
    else // We're finishing a file
    {
        // TODO: remember the file for later cleanup on failure/cancel
    }

LExit:
    return hr;
}

VOID ExtractWriteCallback(UINT cb)
{
    WORD wPercentDone = 0;
    static LONGLONG llTotalWritten = 0;

    llTotalWritten += cb;

    wPercentDone = static_cast<WORD>((llTotalWritten * 100) / vpcd->llExtractedSize);

    ::SendDlgItemMessageW( vhExtractDialog, IDC_GENERIC1, PBM_SETPOS, static_cast<WPARAM>(wPercentDone), 0);
}

VOID FreeFileList(
    IN CUTOFF_FILE* pcf
    )
{
    if (pcf->pcfNextFile)
        FreeFileList(pcf->pcfNextFile);

    MemFree(pcf);
}


/******************************************************************
 ExtractWork -shows extract dialog
 
 Note: May be called through CreateThread
********************************************************* scotk */

DWORD WINAPI ExtractWork(
    LPVOID pvContext
    )
{
    HRESULT hr = S_OK;
    LPWSTR pwzExtractDir = NULL;
    LPWSTR wz = NULL;
    CUTOFF_FILE *pcf = NULL;
    CUTOFF_FILE *pcfFileList = static_cast<CUTOFF_FILE*>(pvContext);

    // Update the progress dialog to "Extracting..." if we're showing it
    if (vhExtractDialog)
    {
        ::ShowWindow(::GetDlgItem(vhExtractDialog, IDC_EXTRACT_WAIT ), SW_HIDE);
        ::ShowWindow(::GetDlgItem(vhExtractDialog, IDC_EXTRACTINGFILE ), SW_SHOW);
    }

    // Extract the files
    for (pcf = pcfFileList; pcf; pcf = pcf->pcfNextFile)
    {
        wz = FileFromPath(pcf->wzExtractPath);
        hr = StrAllocString(&pwzExtractDir, pcf->wzExtractPath, wz - pcf->wzExtractPath);
        ExitOnFailure1(hr, "failed to allocate string for extract path of: %S", pcf->wzExtractPath);

        // Update the progress dialog if we're showing it
        if (vhExtractDialog)
            ::SetDlgItemTextW(vhExtractDialog, IDC_FILENAME, wz);

        hr = RexExtract(L"TODO2", pcf->wzId, pwzExtractDir, wz, ExtractProgressCallback, ExtractWriteCallback, NULL);
        ExitOnFailure(hr, "failed to extract cabinet files");
    }

LExit:
    // If we're showing UI, tell it we're done
    if (vhExtractDialog)
        ::SendMessageW(vhExtractDialog, IDM_EXTRACTDONE, SUCCEEDED(hr), NULL);

    ReleaseNullStr(pwzExtractDir);
    return hr;
}


BOOL CALLBACK ExtractDialogProc(
    HWND hwndDlg, 
    UINT uMsg, 
    WPARAM wParam, 
    LPARAM lParam
    )
{
    BOOL fResult = FALSE;
    static HANDLE hExtractThread = INVALID_HANDLE_VALUE;
    static DWORD dwExtractThreadId = 0;

    switch (uMsg) 
    {
    case WM_INITDIALOG:
        vhExtractDialog = hwndDlg;
        // show the file copy AVI
        Animate_Open(::GetDlgItem(hwndDlg, IDC_USER1), IDA_FILECOPY);
        Animate_Play(::GetDlgItem(hwndDlg, IDC_USER1), 0, -1, -1);
        hExtractThread = ::CreateThread(NULL, 0, ExtractWork, reinterpret_cast<LPVOID>(lParam), 0, &dwExtractThreadId);
        break;
    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDCANCEL:
            // set the canceled flag
            vfCanceled = TRUE;
            fResult = TRUE;
            break;
        }
        break;
    case IDM_EXTRACTDONE:
        //::TerminateThread(hExtractThread, 0); // TODO: don't do this
        ::EndDialog(hwndDlg, (BOOL) wParam);
        fResult = TRUE;
        break;
    case WM_DESTROY:
        // cleanup
        break;
    }

    return fResult;
}


/******************************************************************
 ExtractDialog -shows extract dialog
 
********************************************************* scotk */
DWORD WINAPI ExtractDialog(
    CUTOFF_FILE *pcfFileList
    )
{
    HRESULT hr = S_OK;
    DWORD dwResult = 0;
    HGLOBAL hg = 0;

    dwResult  = ::DialogBoxParamW(NULL, MAKEINTRESOURCEW(IDD_EXTRACT), NULL, ExtractDialogProc, reinterpret_cast<LPARAM>(pcfFileList));
/*
    vhExtractDialog = ::CreateDialogParamW(NULL, MAKEINTRESOURCEW(IDD_EXTRACT), NULL, DialogProc, reinterpret_cast<LPARAM>(pcfFileList));
    if (!vhExtractDialog)
        ExitOnLastError(hr, "failed to create extract dialog");

    ::ShowWindow(vhExtractDialog, SW_SHOW);

    while(!vfCanceled)
    {
        if (-1 != ::GetMessageW(&msg, NULL, 0, 0) && WM_QUIT != msg.message)
        {
            if (!::IsDialogMessage(vhExtractDialog, &msg))
            {
                ::TranslateMessage(&msg);
                ::DispatchMessageW(&msg);
            }
         }
        else
            ExitOnLastError(hr, "failed to get message in extract dialog message loop");
    }

    if (!::DestroyWindow(vhExtractDialog))
        ExitOnLastError(hr, "failed to destroy extract dialog");

LExit:*/
    return hr;
}


HRESULT LoadResourceByName(
    IN LPCWSTR wzName,
    IN LPCWSTR wzType,
    OUT LPBYTE * ppb,
    IN OUT DWORD * pcb
    )
{
    HRESULT hr = S_OK;
    HANDLE hRes = NULL;
    HRSRC hResInfo = NULL;
    DWORD cb = 0;
    
    hResInfo = ::FindResourceW(NULL, wzName, wzType);
    ExitOnNull(hResInfo, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load extract manifest resource info");

    hRes = ::LoadResource(NULL, hResInfo);
    ExitOnNull(hRes, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load resource");

    *ppb = static_cast<LPBYTE>(::LockResource(hRes));
    if (pcb)
        *pcb = ::SizeofResource(NULL, hResInfo);

LExit:
    return hr;
}



/****************************************************************************
 WinMain - entry point for cutoff

****************************************************************** scotk **/
INT WINAPI WinMain(
    HINSTANCE hInstance, 
    HINSTANCE hPrevInstance, 
    LPSTR szCmdLine, 
    int nCmdShow
    )
{
    HRESULT hr = S_OK;
    UINT er = 0;
    BOOL fRet = TRUE;

    int argc;
    LPWSTR* pwzArgv;

    BOOL fQuiet = FALSE;
    BOOL fExtractOnly = FALSE;

    LPBYTE pbRes = NULL;
    DWORD cbRes = 0;
    HANDLE hRes = NULL;
    HRSRC hResInfo = NULL;
    IXMLDOMDocument* pixd = NULL;
    IXMLDOMNode* pixn = NULL; 
    VARIANT_BOOL vbSuccess = 0;
    LPWSTR pwzData = NULL;
    CUTOFF_FILE *pcfFileList = NULL;
    CUTOFF_FILE *pcf = NULL;

    hr = RexInitialize();
    ExitOnFailure(hr, "failed to initialize resource extraction");

    //
    // process the command line
    //
    pwzArgv = ::CommandLineToArgvW(::GetCommandLineW(), &argc);
    if (NULL == pwzArgv)
        ConsoleExitOnFailure(hr = E_OUTOFMEMORY, CONSOLE_COLOR_RED, "failed to get command line");

    hr = ParseCommandLine(argc, pwzArgv, &fQuiet, &fExtractOnly);
    ExitOnFailure(hr, "failed to parse command line");

    if (S_FALSE == hr)
    {
        ShowHelp();
        ExitFunction();
    }

    // prevent the system from showing resolution dialogs
    ::SetErrorMode(SEM_FAILCRITICALERRORS);

    //
    // Load and process file extract manifest
    //
    hr = LoadResourceByName(L"TODO", /*RT_RCDATA*/MAKEINTRESOURCEW(10), &pbRes, &cbRes);
    ExitOnFailure(hr, "failed to load extract manifest resource");

    // TODO: Copy may not be necessary if it doesn't affect resource and pbRes is null terminated
    hr = StrAllocString(&pwzData, reinterpret_cast<LPCWSTR>(pbRes), cbRes / sizeof(WCHAR));
    ExitOnFailure(hr, "failed to allocate string for copying XML resource");

    hr = ProcessData(&pwzData, &pcfFileList);
    ExitOnFailure(hr, "failed to process XML");

    //
    // Load the cutoff binary data
    //
    hr = LoadResourceByName(L"TODO3", /*RT_RCDATA*/MAKEINTRESOURCEW(10), reinterpret_cast<LPBYTE*>(&vpcd), NULL);
    ExitOnFailure(hr, "failed to cutoff binary resource data");

    //
    // Extract quitely... or not
    //
    if (fQuiet)
    {
        hr = ExtractWork(static_cast<LPVOID>(pcfFileList));
        ExitOnFailure(hr, "failed to do extract work");
    }
    else
    {
        hr = ExtractDialog(pcfFileList);
        ExitOnFailure(hr, "failed to show extract dialog");
    }

LExit:

    /*if (FAILED(hr))
        ConsoleWriteLine(CONSOLE_COLOR_RED, "err: 0x%x", hr);*/

    if (pcfFileList)
        FreeFileList(pcfFileList);

    ReleaseNullStr(pwzData);
    ReleaseObject(pixd);

    RexUninitialize();

    return hr;
}

