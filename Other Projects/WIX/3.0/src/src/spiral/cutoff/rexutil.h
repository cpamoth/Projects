#pragma once
/****************************************************************************
 rexutil.h - Resource Cabinet Extract Utilities

 2003.03.24 - scotk - created
****************************************************************************/

#include <sys\stat.h>
#include <fdi.h>

#ifdef __cplusplus
extern "C" {
#endif

// defines
#define FILETABLESIZE 40

// structs
struct MEM_FILE 
{
	LPCBYTE vpStart;
	UINT  uiCurrent;
	UINT  uiLength;
};

typedef enum { NORMAL_FILE, MEMORY_FILE } FAKE_FILE_TYPE;

typedef HRESULT (*REX_CALLBACK_PROGRESS)(BOOL fBeginFile, LPCWSTR wzFileId, LPVOID pvContext);
typedef VOID (*REX_CALLBACK_WRITE)(UINT cb);


struct FAKE_FILE // used in internal file table
{
	BOOL fUsed;
	FAKE_FILE_TYPE fftType;
	MEM_FILE mfFile; // State for memory file
	HANDLE hFile; // Handle for disk  file
};

// functions
HRESULT RexInitialize(
	);
void RexUninitialize(
	);

HRESULT RexExtract(
	IN LPCWSTR wzResource,
	IN LPCWSTR wzExtractId,
	IN LPCWSTR wzExtractDir,
	IN LPCWSTR wzExtractName,
	IN REX_CALLBACK_PROGRESS pfnProgress,
	IN REX_CALLBACK_WRITE pfnWrite,
	IN LPVOID pvContext
	);

#ifdef __cplusplus
}
#endif

