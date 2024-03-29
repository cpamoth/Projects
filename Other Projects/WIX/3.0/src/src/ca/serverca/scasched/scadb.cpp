//-------------------------------------------------------------------------------------------------
// <copyright file="scadb.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    DB functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

// sql queries
LPCWSTR vcsSqlDatabaseQuery = L"SELECT `SqlDb`, `Server`, `Instance`, `Database`, "
                              L"`Component_`, `User_`, `FileSpec_`, `FileSpec_Log`, `Attributes` "
                              L"FROM `SqlDatabase`";
enum eSqlDatabaseQuery { sdqSqlDb = 1, sdqServer, sdqInstance, sdqDatabase, 
                         sdqComponent, sdqUser, sdqDbFileSpec, sdqLogFileSpec, sdqAttributes };

LPCWSTR vcsSqlFileSpecQuery = L"SELECT `FileSpec`, `Name`, `Filename`, `Size`, "
                              L"`MaxSize`, `GrowthSize` FROM `SqlFileSpec` WHERE `FileSpec`=?";
enum eSqlFileSpecQuery { sfsqFileSpec = 1, sfsqName, sfsqFilename, sfsqSize, 
                         sfsqMaxSize, sfsqGrowth };


// prototypes for private helper functions
static HRESULT NewDb(
    __out SCA_DB** ppsd
    );

static SCA_DB* AddDbToList(
    __in SCA_DB* psdList,
    __in SCA_DB* psd
    );

static HRESULT SchedCreateDatabase(
    __in SCA_DB* psd
    );

static HRESULT SchedDropDatabase(
    __in LPCWSTR wzKey, LPCWSTR wzServer,
    __in LPCWSTR wzInstance,
    __in LPCWSTR wzDatabase,
    __in int iAttributes,
    __in BOOL fIntegratedAuth,
    __in LPCWSTR wzUser,
    __in LPCWSTR wzPassword
    );

static HRESULT GetFileSpec(
    __in MSIHANDLE hViewFileSpec,
    __in LPCWSTR wzKey,
    __in SQL_FILESPEC* psf
    );


HRESULT ScaDbsRead(
    __inout SCA_DB** ppsdList,
    __in SCA_ACTION saAction
    )
{
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;
    PMSIHANDLE hView;
    PMSIHANDLE hRec;
    PMSIHANDLE hViewFileSpec = NULL;

    LPWSTR pwzData = NULL;
    LPWSTR pwzId = NULL;
    LPWSTR pwzComponent = NULL;

    SCA_DB* psd = NULL;

    if (S_OK != WcaTableExists(L"SqlDatabase"))
    {
        WcaLog(LOGMSG_VERBOSE, "Skipping ScaCreateDatabase() - SqlDatabase table not present");
        ExitFunction1(hr = S_FALSE);
    }

    if (S_OK == WcaTableExists(L"SqlFileSpec"))
    {
        hr = WcaOpenView(vcsSqlFileSpecQuery, &hViewFileSpec);
        ExitOnFailure(hr, "failed to open view on SqlFileSpec table");
    }

    // loop through all the sql databases
    hr = WcaOpenExecuteView(vcsSqlDatabaseQuery, &hView);
    ExitOnFailure(hr, "Failed to open view on SqlDatabase table");
    while (S_OK == (hr = WcaFetchRecord(hView, &hRec)))
    {
        BOOL fHasComponent = FALSE;
        INSTALLSTATE isInstalled = INSTALLSTATE_UNKNOWN;
        INSTALLSTATE isAction = INSTALLSTATE_UNKNOWN;

        hr = WcaGetRecordString(hRec, sdqSqlDb, &pwzId);
        ExitOnFailure(hr, "Failed to get SqlDatabase.SqlDb");

        hr = WcaGetRecordString(hRec, sdqComponent, &pwzComponent);
        ExitOnFailure1(hr, "Failed to get Component for database: '%S'", psd->wzKey);
        if (pwzComponent && *pwzComponent)
        {
            fHasComponent = TRUE;

            er = ::MsiGetComponentStateW(WcaGetInstallHandle(), pwzComponent, &isInstalled, &isAction);
            hr = HRESULT_FROM_WIN32(er);
            ExitOnFailure1(hr, "Failed to get state for component: %S", pwzComponent);

            // If we're doing install but the Component is not being installed or we're doing
            // uninstall but the Component is not being uninstalled, skip it.
            if ((WcaIsInstalling(isInstalled, isAction) && SCA_ACTION_INSTALL != saAction) ||
                (WcaIsUninstalling(isInstalled, isAction) && SCA_ACTION_UNINSTALL != saAction))
            {
                continue;
            }
        }

        hr  = NewDb(&psd);
        ExitOnFailure1(hr, "Failed to allocate memory for new database: %D", pwzId);

        hr = ::StringCchCopyW(psd->wzKey, countof(psd->wzKey), pwzId);
        ExitOnFailure1(hr, "Failed to copy SqlDatabase.SqlDbL: %S", pwzId);

        hr = ::StringCchCopyW(psd->wzComponent, countof(psd->wzComponent), pwzComponent);
        ExitOnFailure1(hr, "Failed to copy SqlDatabase.Component_: %S", pwzComponent);

        psd->fHasComponent = fHasComponent;
        psd->isInstalled = isInstalled;
        psd->isAction = isAction;

        hr = WcaGetRecordFormattedString(hRec, sdqServer, &pwzData);
        ExitOnFailure1(hr, "Failed to get Server for database: '%S'", psd->wzKey);
        StringCchCopyW(psd->wzServer, countof(psd->wzServer), pwzData);

        hr = WcaGetRecordFormattedString(hRec, sdqInstance, &pwzData);
        ExitOnFailure1(hr, "Failed to get Instance for database: '%S'", psd->wzKey);
        StringCchCopyW(psd->wzInstance, countof(psd->wzInstance), pwzData);

        hr = WcaGetRecordFormattedString(hRec, sdqDatabase, &pwzData);
        ExitOnFailure1(hr, "Failed to get Database for database: '%S'", psd->wzKey);
        StringCchCopyW(psd->wzDatabase, countof(psd->wzDatabase), pwzData);

        hr = WcaGetRecordInteger(hRec, sdqAttributes, &psd->iAttributes);
        ExitOnFailure(hr, "Failed to get SqlDatabase.Attributes");

        hr = WcaGetRecordFormattedString(hRec, sdqUser, &pwzData);
        ExitOnFailure1(hr, "Failed to get User record for database: '%S'", psd->wzKey);

        // if a user was specified
        if (*pwzData)
        {
            psd->fUseIntegratedAuth = FALSE;
            hr = ScaGetUser(pwzData, &psd->scau);
            ExitOnFailure1(hr, "Failed to get user information for database: '%S'", psd->wzKey);
        }
        else
        {
            psd->fUseIntegratedAuth = TRUE;
            // integrated authorization doesn't have a User record
        }

        hr = WcaGetRecordString(hRec, sdqDbFileSpec, &pwzData);
        ExitOnFailure1(hr, "Failed to get Database FileSpec for database: '%S'", psd->wzKey);

        // if a database filespec was specified
        if (*pwzData)
        {
            hr = GetFileSpec(hViewFileSpec, pwzData, &psd->sfDb);
            ExitOnFailure1(hr, "failed to get FileSpec for: %S", pwzData);
            if (S_OK == hr)
            {
                psd->fHasDbSpec = TRUE;
            }
        }

        hr = WcaGetRecordString(hRec, sdqLogFileSpec, &pwzData);
        ExitOnFailure1(hr, "Failed to get Log FileSpec for database: '%S'", psd->wzKey);

        // if a log filespec was specified
        if (*pwzData)
        {
            hr = GetFileSpec(hViewFileSpec, pwzData, &psd->sfLog);
            ExitOnFailure1(hr, "failed to get FileSpec for: %S", pwzData);
            if (S_OK == hr)
            {
                psd->fHasLogSpec = TRUE;
            }
        }

        *ppsdList = AddDbToList(*ppsdList, psd);
        psd = NULL; // set the db NULL so it doesn't accidentally get freed below
    }

    if (E_NOMOREITEMS == hr)
    {
        hr = S_OK;
    }
    ExitOnFailure(hr, "Failure occured while processing SqlDatabase table");

LExit:
    if (psd)
    {
        ScaDbsFreeList(psd);
    }

    ReleaseStr(pwzComponent);
    ReleaseStr(pwzId);
    ReleaseStr(pwzData);
    return hr;
}


SCA_DB* ScaDbsFindDatabase(
    __in LPCWSTR wzSqlDb,
    __in SCA_DB* psdList
    )
{
    SCA_DB* psd = NULL;

    for (psd = psdList; psd; psd = psd->psdNext)
    {
        if (0 == lstrcmpW(wzSqlDb, psd->wzKey))
        {
            break;
        }
    }

    return psd;
}


HRESULT ScaDbsInstall(
    __in SCA_DB* psdList
    )
{
    HRESULT hr = S_FALSE; // assume nothing will be done
    SCA_DB* psd = NULL;

    for (psd = psdList; psd; psd = psd->psdNext)
    {
        if (psd->fHasComponent)
        {
            // if installing this component
            if (((psd->iAttributes & SCADB_CREATE_ON_INSTALL) && WcaIsInstalling(psd->isInstalled, psd->isAction) && !WcaIsReInstalling(psd->isInstalled, psd->isAction)) ||
                ((psd->iAttributes & SCADB_CREATE_ON_REINSTALL) && WcaIsReInstalling(psd->isInstalled, psd->isAction)))
            {
                hr = SchedCreateDatabase(psd);
                ExitOnFailure1(hr, "Failed to ensure database %S exists", psd->wzKey);
            }
            else if (((psd->iAttributes & SCADB_DROP_ON_INSTALL) && WcaIsInstalling(psd->isInstalled, psd->isAction) && !WcaIsReInstalling(psd->isInstalled, psd->isAction)) ||
                ((psd->iAttributes & SCADB_DROP_ON_REINSTALL) && WcaIsReInstalling(psd->isInstalled, psd->isAction)))
            {
                hr = SchedDropDatabase(psd->wzKey, psd->wzServer, psd->wzInstance, psd->wzDatabase, psd->iAttributes, psd->fUseIntegratedAuth, psd->scau.wzName, psd->scau.wzPassword);
                ExitOnFailure1(hr, "Failed to drop database %S", psd->wzKey);
            }
        }
    }

LExit:
    return hr;
}


HRESULT ScaDbsUninstall(
    __in SCA_DB* psdList
    )
{
    HRESULT hr = S_FALSE; // assume nothing will be done
    SCA_DB* psd = NULL;

    for (psd = psdList; psd; psd = psd->psdNext)
    {
        if (psd->fHasComponent)
        {
            // if uninstalling this component
            if ((psd->iAttributes & SCADB_DROP_ON_UNINSTALL) && WcaIsUninstalling(psd->isInstalled, psd->isAction))
            {
                hr = SchedDropDatabase(psd->wzKey, psd->wzServer, psd->wzInstance, psd->wzDatabase, psd->iAttributes, psd->fUseIntegratedAuth, psd->scau.wzName, psd->scau.wzPassword);
                ExitOnFailure1(hr, "Failed to drop database %S", psd->wzKey);
            }
            else if ((psd->iAttributes & SCADB_CREATE_ON_UNINSTALL) && WcaIsUninstalling(psd->isInstalled, psd->isAction))
            {
                hr = SchedCreateDatabase(psd);
                ExitOnFailure1(hr, "Failed to ensure database %S exists", psd->wzKey);
            }
        }
    }

LExit:
    return hr;
}


void ScaDbsFreeList(
    __in SCA_DB* psdList
    )
{
    SCA_DB* psdDelete = psdList;
    while (psdList)
    {
        psdDelete = psdList;
        psdList = psdList->psdNext;

        MemFree(psdDelete);
    }
}


// private helper functions

static HRESULT NewDb(
    __out SCA_DB** ppsd
    )
{
    HRESULT hr = S_OK;
    SCA_DB* psd = (SCA_DB*)MemAlloc(sizeof(SCA_DB), TRUE);
    ExitOnNull(psd, hr, E_OUTOFMEMORY, "failed to allocate memory for new database element");

    *ppsd = psd;

LExit:
    return hr;
}


static SCA_DB* AddDbToList(
    __in SCA_DB* psdList,
    __in SCA_DB* psd
    )
{
    if (psdList)
    {
        SCA_DB* psdT = psdList;
        while (psdT->psdNext)
        {
            psdT  = psdT->psdNext;
        }

        psdT->psdNext = psd;
    }
    else
    {
        psdList = psd;
    }

    return psdList;
}


static HRESULT SchedCreateDatabase(
    __in SCA_DB* psd
    )
{
    HRESULT hr = S_OK;
    WCHAR* pwzCustomActionData = NULL;

    hr = WcaWriteStringToCaData(psd->wzKey, &pwzCustomActionData);
    ExitOnFailure(hr, "failed to add DBKey to CustomActionData");

    hr = WcaWriteStringToCaData(psd->wzServer, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server name to CustomActionData");

    hr = WcaWriteStringToCaData(psd->wzInstance, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server instance to CustomActionData");

    hr = WcaWriteStringToCaData(psd->wzDatabase, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add database name to CustomActionData");

    hr = WcaWriteIntegerToCaData(psd->iAttributes, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add Sql attributes to CustomActionData");

    hr = WcaWriteStringToCaData(psd->fUseIntegratedAuth ? L"1" : L"0", &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add if integrated connection to CustomActionData");

    hr = WcaWriteStringToCaData(psd->scau.wzName, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server user to CustomActionData");

    hr = WcaWriteStringToCaData(psd->scau.wzPassword, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add user password to CustomActionData");

    // Check to see if the database exists, if it does not then schedule a rollback
    // so we clean up after ourselves if the creation of the database fails or is
    // aborted.  It is intereting to note that we can do this check here because the
    // deferred CustomActions are Impersonated.  That means this scheduling action and
    // the execution actions all run with the same user context, so it is safe to
    // to do the check.
    hr = SqlDatabaseExists(psd->wzServer, psd->wzInstance, psd->wzDatabase, psd->fUseIntegratedAuth, psd->scau.wzName, psd->scau.wzPassword, NULL);
    if (S_FALSE == hr)
    {
        hr = WcaDoDeferredAction(L"RollbackCreateDatabase", pwzCustomActionData, COST_SQL_CREATEDB);
        ExitOnFailure(hr, "Failed to schedule RollbackCreateDatabase action");
    }

    // database filespec
    if (psd->fHasDbSpec)
    {
        hr = WcaWriteStringToCaData(L"1", &pwzCustomActionData);
        ExitOnFailure(hr, "failed to specify that do have db.filespec to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfDb.wzName, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add FileSpec.Name to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfDb.wzFilename, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add FileSpec.Filename to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfDb.wzSize, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.Size to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfDb.wzMaxSize, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.MaxSize to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfDb.wzGrow, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.GrowthSize to CustomActionData");
    }
    else
    {
        hr = WcaWriteStringToCaData(L"0", &pwzCustomActionData);
        ExitOnFailure(hr, "failed to specify that do not have db.filespec to CustomActionData");
    }

    // log filespec
    if (psd->fHasLogSpec)
    {
        hr = WcaWriteStringToCaData(L"1", &pwzCustomActionData);
        ExitOnFailure(hr, "failed to specify that do have log.filespec to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfLog.wzName, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add FileSpec.Name to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfLog.wzFilename, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add FileSpec.Filename to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfLog.wzSize, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.Size to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfLog.wzMaxSize, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.MaxSize to CustomActionData");

        hr = WcaWriteStringToCaData(psd->sfLog.wzGrow, &pwzCustomActionData);
        ExitOnFailure(hr, "Failed to add FileSpec.GrowthSize to CustomActionData");
    }
    else
    {
        hr = WcaWriteStringToCaData(L"0", &pwzCustomActionData);
        ExitOnFailure(hr, "failed to specify that do not have log.filespec to CustomActionData");
    }

    // schedule the CreateDatabase action
    hr = WcaDoDeferredAction(L"CreateDatabase", pwzCustomActionData, COST_SQL_CREATEDB);
    ExitOnFailure(hr, "Failed to schedule CreateDatabase action");

LExit:
    ReleaseStr(pwzCustomActionData);
    return hr;
}


HRESULT SchedDropDatabase(
    __in LPCWSTR wzKey,
    __in LPCWSTR wzServer,
    __in LPCWSTR wzInstance,
    __in LPCWSTR wzDatabase,
    __in int iAttributes,
    __in BOOL fIntegratedAuth,
    __in LPCWSTR wzUser,
    __in LPCWSTR wzPassword
    )
{
    HRESULT hr = S_OK;
    WCHAR* pwzCustomActionData = NULL;

    hr = WcaWriteStringToCaData(wzKey, &pwzCustomActionData);
    ExitOnFailure(hr, "failed to add DBKey to CustomActionData");

    hr = WcaWriteStringToCaData(wzServer, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server name to CustomActionData");

    hr = WcaWriteStringToCaData(wzInstance, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server instance to CustomActionData");

    hr = WcaWriteStringToCaData(wzDatabase, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add database name to CustomActionData");

    hr = WcaWriteIntegerToCaData(iAttributes, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server name to CustomActionData");

    hr = WcaWriteStringToCaData(fIntegratedAuth ? L"1" : L"0", &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server name to CustomActionData");

    hr = WcaWriteStringToCaData(wzUser, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add server user to CustomActionData");

    hr = WcaWriteStringToCaData(wzPassword, &pwzCustomActionData);
    ExitOnFailure(hr, "Failed to add user password to CustomActionData");

    hr = WcaDoDeferredAction(L"DropDatabase", pwzCustomActionData, COST_SQL_DROPDB);
    ExitOnFailure(hr, "Failed to schedule DropDatabase action");

LExit:
    ReleaseStr(pwzCustomActionData);
    return hr;
}


HRESULT GetFileSpec(
    __in MSIHANDLE hViewFileSpec,
    __in LPCWSTR wzKey,
    __in SQL_FILESPEC* psf
    )
{
    HRESULT hr = S_OK;
    PMSIHANDLE hRecFileSpec, hRec;
    LPWSTR pwzData = NULL;

    // create a record to do the fetch
    hRecFileSpec = ::MsiCreateRecord(1);
    if (!hRecFileSpec)
    {
        ExitOnFailure1(hr = E_UNEXPECTED, "failed to create record for filespec: %S", wzKey);
    }
    hr = WcaSetRecordString(hRecFileSpec, 1, wzKey);
    ExitOnFailure1(hr, "failed to set record string for filespec: %S", wzKey);

    // get the FileSpec record
    hr = WcaExecuteView(hViewFileSpec, hRecFileSpec);
    ExitOnFailure1(hr, "failed to execute view on SqlFileSpec table for filespec: %S", wzKey);
    hr = WcaFetchSingleRecord(hViewFileSpec, &hRec);
    ExitOnFailure1(hr, "failed to get record for filespec: %S", wzKey);

    // read the data out of the filespec record
    hr = WcaGetRecordFormattedString(hRec, sfsqName, &pwzData);
    ExitOnFailure1(hr, "Failed to get SqlFileSpec.Name for filespec: %S", wzKey);
    StringCchCopyW(psf->wzName, countof(psf->wzName), pwzData);

    hr = WcaGetRecordFormattedString(hRec, sfsqFilename, &pwzData);
    ExitOnFailure1(hr, "Failed to get SqlFileSpec.Filename for filespec: %S", wzKey);
    if (*pwzData)
    {
        StringCchCopyW(psf->wzFilename, countof(psf->wzFilename), pwzData);
    }
    else   // if there is no file, skip this FILESPEC
    {
        WcaLog(LOGMSG_VERBOSE, "No filename specified, skipping FileSpec: %S", psf->wzName);
        ExitFunction1(hr = S_FALSE);
    }

    hr = WcaGetRecordFormattedString(hRec, sfsqSize, &pwzData);
    ExitOnFailure1(hr, "Failed to get SqlFileSpec.Size for filespec: %S", wzKey);
    if (*pwzData)
    {
        StringCchCopyW(psf->wzSize, countof(psf->wzSize), pwzData);
    }
    else
    {
        psf->wzSize[0] = 0;
    }

    hr = WcaGetRecordFormattedString(hRec, sfsqMaxSize, &pwzData);
    ExitOnFailure1(hr, "Failed to get SqlFileSpec.MaxSize for filespec: %S", wzKey);
    if (*pwzData)
    {
        StringCchCopyW(psf->wzMaxSize, countof(psf->wzMaxSize), pwzData);
    }
    else
    {
        psf->wzMaxSize[0] = 0;
    }

    hr = WcaGetRecordFormattedString(hRec, sfsqGrowth, &pwzData);
    ExitOnFailure1(hr, "Failed to get SqlFileSpec.GrowthSize for filespec: %S", wzKey);
    if (*pwzData)
    {
        StringCchCopyW(psf->wzGrow, countof(psf->wzGrow), pwzData);
    }
    else
    {
        psf->wzGrow[0] = 0;
    }

    hr = S_OK;
LExit:
    ReleaseStr(pwzData);
    return hr;
}
