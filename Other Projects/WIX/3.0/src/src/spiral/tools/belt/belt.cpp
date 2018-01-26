/*-----------------------------------------------------------------------------
 belt.cpp - Creats Cabs
 Owner(s): scotk

 Created: 1/27/03
-----------------------------------------------------------------------------*/

#include "precomp.h"

/*-----------------------------------------------------------------------------
 ShowHelp - displays help message

------------------------------------------------------------------- scotk --*/
void ShowHelp()
{
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "belt - creates cabs\r\n");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "belt.exe [-c <cab> [compression type] <file list>]");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -c  create a cab from a file list");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "       compression type values: NONE HIGH MEDIUM LOW MSZIP");
    ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "   -?  this help information");
}


/*-----------------------------------------------------------------------------
 ParseCommandLine - returns the command line arguments in useful variables

 NOTE: cFiles is is used to pass the size of the array rgpwzFiles and to pass back the number
 of file names passed in as args
------------------------------------------------------------------- scotk --*/
HRESULT ParseCommandLine(
    IN DWORD argc, 
    IN WCHAR* argv[], 
    OUT BOOL* pfCreate,
    OUT LPWSTR* pwzCab,
    OUT LPWSTR* rgpwzFiles,
    IN OUT DWORD *cFiles,
    OUT COMPRESSION_TYPE* pct
    )
{
    HRESULT hr = S_FALSE;
    WCHAR *pwzArg;
    
    *pfCreate = FALSE;

    for(DWORD i = 0; i < *cFiles; i++)
        rgpwzFiles[i] = NULL;

    // skip the first arguemnt which is the program name
    for (i = 1; i < argc; i++)
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
                case 'c':   // create a cab
                    // move i but fail if it walked off the end of the list
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify cab name to create");
                    *pwzCab = argv[i];
                    if (++i == argc)
                        ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    // check if they specified a compression type
                    if (0 == lstrcmpW(argv[i], L"NONE"))
                    {
                        *pct = COMPRESSION_TYPE_NONE;
                        if (++i == argc)
                            ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    }
                    else if (0 == lstrcmpW(argv[i], L"HIGH"))
                    {
                        *pct = COMPRESSION_TYPE_HIGH;
                        if (++i == argc)
                            ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    }
                    else if (0 == lstrcmpW(argv[i], L"MEDIUM"))
                    {
                        *pct = COMPRESSION_TYPE_MEDIUM;
                        if (++i == argc)
                            ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    }
                    else if (0 == lstrcmpW(argv[i], L"LOW"))
                    {
                        *pct = COMPRESSION_TYPE_LOW;
                        if (++i == argc)
                            ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    }
                    else if (0 == lstrcmpW(argv[i], L"MSZIP"))
                    {
                        *pct = COMPRESSION_TYPE_MSZIP;
                        if (++i == argc)
                            ExitOnFailure(hr = E_INVALIDARG, "Did not specify at least one file to include in cab");
                    }
                    *cFiles = 0;
                    rgpwzFiles[*cFiles] = argv[i];
                    while (++i < argc)
                        rgpwzFiles[++(*cFiles)] = argv[i];

                    *pfCreate = TRUE;
                    hr = S_OK;
                    break;
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


/****************************************************************************
 main - entry point for resourcerer

****************************************************************** scotk **/
extern "C" HRESULT __cdecl main()
{
    HRESULT hr = S_OK;
    UINT er = 0;
    BOOL fRet = TRUE;

    DWORD argc;
    LPWSTR* pwzArgv;

    BOOL fCreate = FALSE;
    LPWSTR wzCab = NULL;
    LPWSTR rgwzFiles[30];
    DWORD cFiles = countof(rgwzFiles);
    COMPRESSION_TYPE ct = COMPRESSION_TYPE_MSZIP;
    HANDLE hCabC = INVALID_HANDLE_VALUE;

    hr = ConsoleInitialize();
    ExitOnFailure(hr, "failed to initialize console");

    //
    // process the command line
    //
    pwzArgv = ::CommandLineToArgvW(::GetCommandLineW(), reinterpret_cast<int *>(&argc));
    if (NULL == pwzArgv)
        ConsoleExitOnFailure(hr = E_OUTOFMEMORY, CONSOLE_COLOR_RED, "failed to get command line");

    hr = ParseCommandLine(argc, pwzArgv, &fCreate, &wzCab, rgwzFiles, &cFiles, &ct);
    ConsoleExitOnFailure(hr, CONSOLE_COLOR_RED, "failed to parse command line");

    if (S_FALSE == hr)
    {
        ShowHelp();
        ExitFunction();
    }

    //
    // if we have a cab to create
    //
    if (fCreate)
    {
        hr = CabCBegin(wzCab, NULL, NULL, NULL, ct, &hCabC);
        ExitOnFailure(hr, "failed to begin cabinet creation");
        ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "Creating Cab %S with files:", wzCab);
        for (DWORD i = 0; i <= cFiles; i++)
        {
            ConsoleWriteLine(CONSOLE_COLOR_NORMAL, "  %S", rgwzFiles[i]);
            hr = CabCAddFile(rgwzFiles[i], NULL, hCabC);
            ExitOnFailure1(hr, "failed to add to cabinet file: %S", rgwzFiles[i]);
        }
        hr = CabCFinish(hCabC);
        ExitOnFailure(hr, "failed to finish cabinet");
        hCabC = INVALID_HANDLE_VALUE;
    }

LExit:

    if (INVALID_HANDLE_VALUE != hCabC)
        CabCFinish(hCabC);

    if (FAILED(hr))
        ConsoleWriteLine(CONSOLE_COLOR_RED, "err: 0x%x", hr);

    ConsoleUninitialize();

    return hr;
}


void __cdecl mainStartup()
{
    HRESULT hr = main();
    ::ExitProcess(hr);
}

