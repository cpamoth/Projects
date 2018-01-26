/****************************************************************************
 rexutil.cpp - Resource Cabinet Extract Utilities

 2003.03.24 - scotk - created
****************************************************************************/

#include "precomp.h"

//
// static globals
//
static HMODULE vhCabinetDll = NULL;
static HFDI vhfdi = NULL;
static ERF verf;

static FAKE_FILE vrgffFileTable[FILETABLESIZE];
static DWORD vcbRes;
static LPCBYTE vpbRes;
static CHAR vszResource[MAX_PATH];
static REX_CALLBACK_WRITE vpfnWrite = NULL;

static HRESULT vhrLastError = S_OK;

//
// structs
//
struct REX_CALLBACK_STRUCT
{
    BOOL fStopExtracting;   // flag set when no more files are needed
    LPCWSTR pwzExtract;         // file to extract ("*" means extract all)
    LPCWSTR pwzExtractDir;      // directory to extract files to
    LPCWSTR pwzExtractName; // name of file (pwzExtract can't be "*")

    // possible user data
    REX_CALLBACK_PROGRESS pfnProgress;
    LPVOID pvContext;
};

//
// prototypes
//
static LPVOID DIAMONDAPI RexAlloc(DWORD dwSize);
static void DIAMONDAPI RexFree(LPVOID pvData);
static INT_PTR FAR DIAMONDAPI RexOpen(__no_count char FAR *pszFile, int oflag, int pmode);
static UINT FAR DIAMONDAPI RexRead(INT_PTR hf, void FAR *pv, UINT cb);
static UINT FAR DIAMONDAPI RexWrite(INT_PTR hf, void FAR *pv, UINT cb);
static int FAR DIAMONDAPI RexClose(INT_PTR hf);
static long FAR DIAMONDAPI RexSeek(INT_PTR hf, long dist, int seektype);
static INT_PTR DIAMONDAPI RexCallback(FDINOTIFICATIONTYPE iNotification, FDINOTIFICATION *pFDINotify);


/********************************************************************
 RexInitialize - initializes internal static variables

*********************************************************** scotk */
extern "C" HRESULT RexInitialize(
    )
{
    Assert(!vhfdi);

    HRESULT hr = S_OK;

    vhfdi = ::FDICreate(RexAlloc, RexFree, RexOpen, RexRead, RexWrite, RexClose, RexSeek, cpuUNKNOWN, &verf);
    if (!vhfdi)
        ExitOnFailure(hr = E_FAIL, "failed to initialize cabinet.dll"); // TODO: put verf info in trace message here

    ::ZeroMemory(vrgffFileTable, sizeof(vrgffFileTable));

LExit:
    if (FAILED(hr))
    {
        ::FDIDestroy(vhfdi);
        vhfdi = NULL;
    }
    
    return hr;
}


/********************************************************************
 RexUninitialize - initializes internal static variables

*********************************************************** scotk */
extern "C" void RexUninitialize(
    )
{
    if (vhfdi)
    {
        ::FDIDestroy(vhfdi);
        vhfdi = NULL;
    }
}


/********************************************************************
 RexExtract - extracts one or all files from a resource cabinet

 NOTE: wzExtractId can be a single file id or "*" to extract all files
       wzExttractDir must be normalized (end in a "\")
       wzExtractName is ignored if wzExtractId is "*"
*********************************************************** scotk */
extern "C" HRESULT RexExtract(
    IN LPCWSTR wzResource,
    IN LPCWSTR wzExtractId,
    IN LPCWSTR wzExtractDir,
    IN LPCWSTR wzExtractName,
    IN REX_CALLBACK_PROGRESS pfnProgress,
    IN REX_CALLBACK_WRITE pfnWrite,
    IN LPVOID pvContext
    )
{
    Assert(vhfdi);
    HRESULT hr = S_OK;
    BOOL fResult;

    HRSRC hResInfo = NULL;
    HANDLE hRes = NULL;

    REX_CALLBACK_STRUCT rcs;

    // remember the write callback
    vpfnWrite = pfnWrite;

    //
    // load the cabinet resource
    //
    hResInfo = ::FindResourceW(NULL, wzResource, /*RT_RCDATA*/MAKEINTRESOURCEW(10));
    ExitOnNull(hResInfo, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load resource info");

    hRes = ::LoadResource(NULL, hResInfo);
    ExitOnNull(hRes, hr, HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND), "failed to load resource");

    vcbRes = ::SizeofResource(NULL, hResInfo);
    vpbRes = (LPBYTE)::LockResource(hRes);

    // TODO: Call FDIIsCabinet to confirm resource is a cabinet before trying to extract from it

    //
    // convert the resource name to multi-byte
    //
    if (!::WideCharToMultiByte(CP_ACP, 0, wzResource, -1, vszResource, countof(vszResource), NULL, NULL))
        ExitOnLastError1(hr, "failed to convert cabinet resource name to ASCII: %S", wzResource);

    //
    // iterate through files in cabinet extracting them to the callback function
    //
    rcs.fStopExtracting = FALSE;
    rcs.pwzExtract = wzExtractId;
    rcs.pwzExtractDir = wzExtractDir;
    rcs.pwzExtractName = wzExtractName;
    rcs.pfnProgress = pfnProgress;
    rcs.pvContext = pvContext;

    fResult = ::FDICopy(vhfdi, vszResource, "", 0, RexCallback, NULL, static_cast<void*>(&rcs));
    if (!fResult && !rcs.fStopExtracting)   // if something went wrong and it wasn't us just stopping the extraction, then return a failure
        hr = vhrLastError;  // TODO: put verf info in trace message here

LExit:
    return hr;
}


/****************************************************************************
 default extract routines

****************************************************************************/
static LPVOID DIAMONDAPI RexAlloc(DWORD dwSize)
{ 
    return MemAlloc(dwSize, FALSE);
}


static void DIAMONDAPI RexFree(LPVOID pvData)
{ 
    MemFree(pvData);
}


static INT_PTR FAR DIAMONDAPI RexOpen(__no_count char FAR *pszFile, int oflag, int pmode)
{
    HRESULT hr = S_OK;
    HANDLE hFile = INVALID_HANDLE_VALUE;

    // if FDI asks for some crazy mode (in low memory situation it could ask for a scratch file) fail
    if ((oflag != (/*_O_BINARY*/ 0x8000 | /*_O_RDONLY*/ 0x0000)) || (pmode != (_S_IREAD | _S_IWRITE)))
        ExitOnFailure(hr = E_OUTOFMEMORY, "FDI asked for to create a scratch file, which is crazy madness");

    // find an empty spot in the fake file table
    for (int i=0; i < FILETABLESIZE; i++)
        if (!vrgffFileTable[i].fUsed)
            break;

    // we should never run out of space in the fake file table
    if (FILETABLESIZE <= i)
        ExitOnFailure(hr = E_OUTOFMEMORY, "File table exceeded");

    if (0 == lstrcmpA(vszResource, pszFile))
    {

        vrgffFileTable[i].fUsed = TRUE;
        vrgffFileTable[i].fftType = MEMORY_FILE;
        vrgffFileTable[i].mfFile.vpStart = static_cast<LPCBYTE>(vpbRes);
        vrgffFileTable[i].mfFile.uiCurrent = 0;
        vrgffFileTable[i].mfFile.uiLength = vcbRes;
    }
    else   // it's a real file
    {
        hFile = ::CreateFileA(pszFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (INVALID_HANDLE_VALUE == hFile)
            ExitOnLastError1(hr, "failed to open file: %s", pszFile);

        vrgffFileTable[i].fUsed = TRUE;
        vrgffFileTable[i].fftType = NORMAL_FILE;
        vrgffFileTable[i].hFile = hFile;
    }

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return FAILED(hr) ? -1 : i;
}


static UINT FAR DIAMONDAPI RexRead(INT_PTR hf, void FAR *pv, UINT cb)
{
    Assert(vrgffFileTable[hf].fUsed);

    HRESULT hr = S_OK;
    DWORD cbRead = 0;
    DWORD cbAvailable = 0;

    if (MEMORY_FILE == vrgffFileTable[hf].fftType)
    {
        // ensure that we don't read past the length of the resource
        cbAvailable = vrgffFileTable[hf].mfFile.uiLength - vrgffFileTable[hf].mfFile.uiCurrent;
        cbRead = cb < cbAvailable? cb : cbAvailable;

        Assert(cbRead >= 0);
        memcpy(pv, static_cast<const void *>(vrgffFileTable[hf].mfFile.vpStart + vrgffFileTable[hf].mfFile.uiCurrent), cbRead);

        vrgffFileTable[hf].mfFile.uiCurrent += cbRead;
    }
    else // NORMAL_FILE
    {
        Assert(vrgffFileTable[hf].hFile && vrgffFileTable[hf].hFile != INVALID_HANDLE_VALUE);

        if (!::ReadFile(vrgffFileTable[hf].hFile, pv, cb, &cbRead, NULL))
            ExitOnLastError(hr, "failed to read during cabinet extraction");
    }

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return FAILED(hr) ? -1 : cbRead;
}


static UINT FAR DIAMONDAPI RexWrite(INT_PTR hf, void FAR *pv, UINT cb)
{
    Assert(vrgffFileTable[hf].fUsed);
    Assert(vrgffFileTable[hf].fftType == NORMAL_FILE); // we should never be writing to a memory file

    HRESULT hr = S_OK;
    DWORD cbWrite = 0;

    Assert(vrgffFileTable[hf].hFile && vrgffFileTable[hf].hFile != INVALID_HANDLE_VALUE);
    if (!::WriteFile(reinterpret_cast<HANDLE>(vrgffFileTable[hf].hFile), pv, cb, &cbWrite, NULL))
        ExitOnLastError(hr, "failed to write during cabinet extraction");

    // call the writer callback if defined
    if (vpfnWrite)
        vpfnWrite(cb);

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return FAILED(hr) ? -1 : cbWrite;
}


static long FAR DIAMONDAPI RexSeek(INT_PTR hf, long dist, int seektype)
{
    Assert(vrgffFileTable[hf].fUsed);

    HRESULT hr = S_OK;
    DWORD dwMoveMethod;
    LONG lMove = 0;

    switch (seektype)
    {
    case 0:   // SEEK_SET
        dwMoveMethod = FILE_BEGIN;
        break;
    case 1:   /// SEEK_CUR
        dwMoveMethod = FILE_CURRENT;
        break;
    case 2:   // SEEK_END
        dwMoveMethod = FILE_END;
        break;
    default :
        dwMoveMethod = 0;
        ExitOnFailure1(hr = E_UNEXPECTED, "unexpected seektype in FDISeek(): %d", seektype);
    }

    if (MEMORY_FILE == vrgffFileTable[hf].fftType)
    {
        if (FILE_BEGIN == dwMoveMethod)
            vrgffFileTable[hf].mfFile.uiCurrent = dist;
        else if (FILE_CURRENT == dwMoveMethod)
            vrgffFileTable[hf].mfFile.uiCurrent += dist;
        else // FILE_END
            vrgffFileTable[hf].mfFile.uiCurrent = vrgffFileTable[hf].mfFile.uiLength + dist;

        lMove = vrgffFileTable[hf].mfFile.uiCurrent;
    }
    else // NORMAL_FILE
    {
        Assert(vrgffFileTable[hf].hFile && vrgffFileTable[hf].hFile != INVALID_HANDLE_VALUE);

        // SetFilePointer returns -1 if it fails (this will cause FDI to quit with an FDIERROR_USER_ABORT error. 
        // (Unless this happens while working on a cabinet, in which case FDI returns FDIERROR_CORRUPT_CABINET)
        lMove = ::SetFilePointer(vrgffFileTable[hf].hFile, dist, NULL, dwMoveMethod);
        if (0xFFFFFFFF == lMove)
            ExitOnLastError1(hr, "failed to move file pointer %d bytes", dist);
    }

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return FAILED(hr) ? -1 : lMove;
}


int FAR DIAMONDAPI RexClose(INT_PTR hf)
{
    Assert(vrgffFileTable[hf].fUsed);

    HRESULT hr = S_OK;

    if (MEMORY_FILE == vrgffFileTable[hf].fftType)
    {
        vrgffFileTable[hf].mfFile.vpStart = NULL;
        vrgffFileTable[hf].mfFile.uiCurrent = 0;
        vrgffFileTable[hf].mfFile.uiLength = 0;
    }
    else
    {
        Assert(vrgffFileTable[hf].hFile && vrgffFileTable[hf].hFile != INVALID_HANDLE_VALUE);

        if (!::CloseHandle(vrgffFileTable[hf].hFile))
            ExitOnLastError(hr, "failed to close file during cabinet extraction");

        vrgffFileTable[hf].hFile = INVALID_HANDLE_VALUE;
    }

    vrgffFileTable[hf].fUsed = FALSE;

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return FAILED(hr) ? -1 : 0;
}


static INT_PTR DIAMONDAPI RexCallback(FDINOTIFICATIONTYPE iNotification, FDINOTIFICATION *pFDINotify)
{
    Assert(pFDINotify->pv);

    HRESULT hr = S_OK;
    int ipResult = 0;   // result to return on success
    HANDLE hFile = INVALID_HANDLE_VALUE;

    REX_CALLBACK_STRUCT* prcs = static_cast<REX_CALLBACK_STRUCT*>(pFDINotify->pv);
    LPCSTR sz;
    WCHAR wz[MAX_PATH];
    FILETIME ft;

    switch(iNotification)
    {
    case fdintCOPY_FILE:  // begin extracting a resource from cabinet
        Assert(pFDINotify->psz1);

        if (prcs->fStopExtracting)
            ExitFunction1(hr = S_FALSE);   // no more extracting

        // convert params to useful variables
        sz = static_cast<LPCSTR>(pFDINotify->psz1);
        if (!::MultiByteToWideChar(CP_ACP, 0, sz, -1, wz, countof(wz)))
            ExitOnLastError1(hr, "failed to convert cabinet file id to unicode: %s", sz);

        if (prcs->pfnProgress)
        {
            hr = prcs->pfnProgress(TRUE, wz, prcs->pvContext);
            if (S_OK != hr)
                ExitFunction();
        }

        if (L'*' == *prcs->pwzExtract || 0 == lstrcmpW(prcs->pwzExtract, wz))
        {
            // get the created date for the resource in the cabinet
            if (!::DosDateTimeToFileTime(pFDINotify->date, pFDINotify->time, &ft))
                ExitOnLastError1(hr, "failed to get time for resource: %S", wz);
            
            WCHAR wzPath[MAX_PATH];

            hr = DirEnsureExists(prcs->pwzExtractDir, NULL);
            ExitOnFailure1(hr, "failed to ensure directory: %S", prcs->pwzExtractDir);

            hr = StringCchCopyW(wzPath, countof(wzPath), prcs->pwzExtractDir);
            ExitOnFailure2(hr, "failed to copy in extract directory: %S for file: %S", prcs->pwzExtractDir, wz);

            if (L'*' == *prcs->pwzExtract)
            {
                hr = StringCchCatW(wzPath, countof(wzPath), wz);
                ExitOnFailure2(hr, "failed to concat onto path: %S file: %S", wzPath, wz);
            }
            else
            {
                Assert(*prcs->pwzExtractName);

                hr = StringCchCatW(wzPath, countof(wzPath), prcs->pwzExtractName);
                ExitOnFailure2(hr, "failed to concat onto path: %S file: %S", wzPath, prcs->pwzExtractName);
            }

            // find an empty spot in the fake file table
            for (int i=0; i < FILETABLESIZE; i++)
                if (!vrgffFileTable[i].fUsed)
                    break;

            // we should never run out of space in the fake file table
            if (FILETABLESIZE <= i)
                ExitOnFailure(hr = E_OUTOFMEMORY, "File table exceeded");

            // open the file
            hFile = ::CreateFileW(wzPath, GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
            if (INVALID_HANDLE_VALUE == hFile)
                ExitOnLastError1(hr, "failed to open file: %S", wzPath);

            vrgffFileTable[i].fUsed = TRUE;
            vrgffFileTable[i].fftType = NORMAL_FILE;
            vrgffFileTable[i].hFile = hFile;

            ipResult = i;

            ::SetFileTime(vrgffFileTable[i].hFile, &ft, &ft, &ft);   // try to set the file time (who cares if it fails)

            if (::SetFilePointer(vrgffFileTable[i].hFile, pFDINotify->cb, NULL, FILE_BEGIN))   // try to set the end of the file (don't worry if this fails)
            {
                if (::SetEndOfFile(vrgffFileTable[i].hFile))
                {
                    ::SetFilePointer(vrgffFileTable[i].hFile, 0, NULL, FILE_BEGIN);  // reset the file pointer
                }
            }
        }
        else  // resource wasn't requested, skip it
        {
            hr = S_OK;
            ipResult = 0;
        }

        break;
    case fdintCLOSE_FILE_INFO:  // resource extraction complete
        Assert(pFDINotify->hf && pFDINotify->psz1);

        // convert params to useful variables
        sz = static_cast<LPCSTR>(pFDINotify->psz1);
        if (!::MultiByteToWideChar(CP_ACP, 0, sz, -1, wz, countof(wz)))
            ExitOnLastError1(hr, "failed to convert cabinet file id to unicode: %s", sz);

        RexClose(pFDINotify->hf);

        if (prcs->pfnProgress)
            hr = prcs->pfnProgress(FALSE, wz, prcs->pvContext);

        if (S_OK == hr && L'*' == *prcs->pwzExtract)   // if everything is okay and we're extracting all files, keep going
            ipResult = TRUE;
        else   // something went wrong or we only needed to extract one file
        {
            hr = S_OK;
            ipResult = FALSE;
            prcs->fStopExtracting = TRUE;
        }

        break;
    case fdintPARTIAL_FILE:   // no action needed for these messages, fall through
    case fdintNEXT_CABINET:
    case fdintENUMERATE:
    case fdintCABINET_INFO:
        break;
    default:
        AssertSz(FALSE, "RexCallback() - unknown FDI notification command");
    };

LExit:
    if (FAILED(hr))
        vhrLastError = hr;

    return (S_OK == hr) ? ipResult : -1;
}

