//-------------------------------------------------------------------------------------------------
// <copyright file="XmlConfig.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Code to configure XML files.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

#define XMLCONFIG_ELEMENT 0x00000001
#define XMLCONFIG_VALUE 0x00000002
#define XMLCONFIG_DOCUMENT 0x00000004
#define XMLCONFIG_CREATE 0x00000010
#define XMLCONFIG_DELETE 0x00000020
#define XMLCONFIG_INSTALL 0x00000100
#define XMLCONFIG_UNINSTALL 0x00000200

enum eXmlAction
{
    xaUnknown = 0,
    xaOpenFile,
    xaWriteValue,
    xaWriteDocument,
    xaDeleteValue,
    xaCreateElement,
    xaDeleteElement,
};

LPCWSTR vcsXmlConfigQuery =
    L"SELECT `XmlConfig`, `File`, `ElementPath`, `VerifyPath`, `Name`, `Value`, `Flags`, `Component_` "
    L"FROM `XmlConfig` ORDER BY `File`, `Sequence`";
enum eXmlConfigQuery { xfqXmlConfig = 1, xfqFile, xfqElementPath, xfqVerifyPath, xfqName, xfqValue, xfqFlags, xfqComponent  };

struct XML_CONFIG_CHANGE
{
    WCHAR wzId[MAX_DARWIN_KEY + 1];

    WCHAR wzComponent[MAX_DARWIN_KEY + 1];
    INSTALLSTATE isInstalled;
    INSTALLSTATE isAction;

    WCHAR wzFile[MAX_PATH];
    LPWSTR pwzElementPath;
    LPWSTR pwzVerifyPath;
    WCHAR wzName[MAX_DARWIN_COLUMN];
    LPWSTR pwzValue;
    BOOL fInstalledFile;

    INT iFlags;

    XML_CONFIG_CHANGE* pxfcAdditionalChanges;
    int cAdditionalChanges;

    XML_CONFIG_CHANGE* pxfcPrev;
    XML_CONFIG_CHANGE* pxfcNext;
};

static HRESULT FreeXmlConfigChangeList(
    __in XML_CONFIG_CHANGE* pxfcList
    )
{
    HRESULT hr = S_OK;

    XML_CONFIG_CHANGE* pxfcDelete;
    while(pxfcList)
    {
        pxfcDelete = pxfcList;
        pxfcList = pxfcList->pxfcNext;

        if (pxfcDelete->pwzElementPath)
        {
            hr = MemFree(pxfcDelete->pwzElementPath);
            ExitOnFailure(hr, "failed to free xml file element path in change list item");
        }

        if (pxfcDelete->pwzVerifyPath)
        {
            hr = MemFree(pxfcDelete->pwzVerifyPath);
            ExitOnFailure(hr, "failed to free xml file verify path in change list item");
        }

        if (pxfcDelete->pwzValue)
        {
            hr = MemFree(pxfcDelete->pwzValue);
            ExitOnFailure(hr, "failed to free xml file value in change list item");
        }

        hr = MemFree(pxfcDelete);
        ExitOnFailure(hr, "failed to free xml file change list item");
    }

LExit:
    return hr;
}

static HRESULT AddXmlConfigChangeToList(
    __inout XML_CONFIG_CHANGE** ppxfcHead,
    __inout XML_CONFIG_CHANGE** ppxfcTail
    )
{
    Assert(ppxfcHead && ppxfcTail);

    HRESULT hr = S_OK;

    XML_CONFIG_CHANGE* pxfc = (XML_CONFIG_CHANGE*)MemAlloc(sizeof(XML_CONFIG_CHANGE), TRUE);
    ExitOnNull(pxfc, hr, E_OUTOFMEMORY, "failed to allocate memory for new xml file change list element");

    // Add it to the end of the list
    if (NULL == *ppxfcHead)
    {
        *ppxfcHead = pxfc;
        *ppxfcTail = pxfc;
    }
    else
    {
        Assert(*ppxfcTail && (*ppxfcTail)->pxfcNext == NULL);
        (*ppxfcTail)->pxfcNext = pxfc;
        pxfc->pxfcPrev = *ppxfcTail;
        *ppxfcTail = pxfc;
    }

LExit:
    return hr;
}


static HRESULT ReadXmlConfigTable(
    __inout XML_CONFIG_CHANGE** ppxfcHead,
    __inout XML_CONFIG_CHANGE** ppxfcTail
    )
{
    Assert(ppxfcHead && ppxfcTail);

    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    PMSIHANDLE hView = NULL;
    PMSIHANDLE hRec = NULL;

    LPWSTR pwzData = NULL;

    // loop through all the xml configurations
    hr = WcaOpenExecuteView(vcsXmlConfigQuery, &hView);
    ExitOnFailure(hr, "failed to open view on XmlConfig table");

    while (S_OK == (hr = WcaFetchRecord(hView, &hRec)))
    {
        hr = AddXmlConfigChangeToList(ppxfcHead, ppxfcTail);
        ExitOnFailure(hr, "failed to add xml file change to list");

        // Get record Id
        hr = WcaGetRecordString(hRec, xfqXmlConfig, &pwzData);
        ExitOnFailure(hr, "failed to get XmlConfig record Id");
        hr = StringCchCopyW((*ppxfcTail)->wzId, countof((*ppxfcTail)->wzId), pwzData);
        ExitOnFailure(hr, "failed to copy XmlConfig record Id");

        // Get component name
        hr = WcaGetRecordString(hRec, xfqComponent, &pwzData);
        ExitOnFailure1(hr, "failed to get component name for XmlConfig: %S", (*ppxfcTail)->wzId);

        // Get the component's state
        if (0 < lstrlenW(pwzData))
        {
            hr = StringCchCopyW((*ppxfcTail)->wzComponent, countof((*ppxfcTail)->wzComponent), pwzData);
            ExitOnFailure1(hr, "failed to copy component: %S", (*ppxfcTail)->wzComponent);
 
            er = ::MsiGetComponentStateW(WcaGetInstallHandle(), (*ppxfcTail)->wzComponent, &(*ppxfcTail)->isInstalled, &(*ppxfcTail)->isAction);
            ExitOnFailure1(hr = HRESULT_FROM_WIN32(er), "failed to get install state for Component: %S", pwzData);
        }

        // Get the xml file
        hr = WcaGetRecordFormattedString(hRec, xfqFile, &pwzData);
        ExitOnFailure1(hr, "failed to get xml file for XmlConfig: %S", (*ppxfcTail)->wzId);
        hr = StringCchCopyW((*ppxfcTail)->wzFile, countof((*ppxfcTail)->wzFile), pwzData);
        ExitOnFailure1(hr, "failed to copy xml file: %S", (*ppxfcTail)->wzFile);

        // Figure out if the file is already on the machine or if it's being installed
        hr = WcaGetRecordString(hRec, xfqFile, &pwzData);
        ExitOnFailure1(hr, "failed to get xml file for XmlConfig: %S", (*ppxfcTail)->wzId);
        if (NULL != wcsstr(pwzData, L"[!") || NULL != wcsstr(pwzData, L"[#"))
        {
            (*ppxfcTail)->fInstalledFile = TRUE;
        }

        // Get the flags
        hr = WcaGetRecordInteger(hRec, xfqFlags, &(*ppxfcTail)->iFlags);
        ExitOnFailure1(hr, "failed to get Flags for XmlConfig: %S", (*ppxfcTail)->wzId);

        // Get the Element Path
        hr = WcaGetRecordFormattedString(hRec, xfqElementPath, &(*ppxfcTail)->pwzElementPath);
        ExitOnFailure1(hr, "failed to get Element Path for XmlConfig: %S", (*ppxfcTail)->wzId);

        // Get the Verify Path
        hr = WcaGetRecordFormattedString(hRec, xfqVerifyPath, &(*ppxfcTail)->pwzVerifyPath);
        ExitOnFailure1(hr, "failed to get Verify Path for XmlConfig: %S", (*ppxfcTail)->wzId);

        // Get the name
        hr = WcaGetRecordFormattedString(hRec, xfqName, &pwzData);
        ExitOnFailure1(hr, "failed to get Name for XmlConfig: %S", (*ppxfcTail)->wzId);
        hr = StringCchCopyW((*ppxfcTail)->wzName, countof((*ppxfcTail)->wzName), pwzData);
        ExitOnFailure1(hr, "failed to copy name: %S", (*ppxfcTail)->wzName);

        // Get the value
        hr = WcaGetRecordFormattedString(hRec, xfqValue, &pwzData);
        ExitOnFailure1(hr, "failed to get Value for XmlConfig: %S", (*ppxfcTail)->wzId);
        hr = StrAllocString(&(*ppxfcTail)->pwzValue, pwzData, 0);
        ExitOnFailure1(hr, "failed to allocate buffer for value: %S", (*ppxfcTail)->pwzValue);
    }

    // if we looped through all records all is well
    if (E_NOMOREITEMS == hr)
        hr = S_OK;
    ExitOnFailure(hr, "failed while looping through all objects to secure");

LExit:
    ReleaseStr(pwzData);

    return hr;
}

static HRESULT ProcessChanges(
    __inout XML_CONFIG_CHANGE** ppxfcHead
    )
{
    Assert(ppxfcHead && *ppxfcHead);
    HRESULT hr = S_OK;

    XML_CONFIG_CHANGE* pxfc = NULL;
    XML_CONFIG_CHANGE* pxfcNext = NULL;
    XML_CONFIG_CHANGE* pxfcCheck = NULL;
    int cAdditionalChanges = 0;
    XML_CONFIG_CHANGE* pxfcLast = NULL;

    // If there's only one item in the list, none of this matters
    if (pxfc && !pxfc->pxfcNext)
        ExitFunction();

    // Loop through the list
    pxfc = *ppxfcHead;
    while (pxfc)
    {
        // Keep track of where our next spot will be since our current node may be moved
        pxfcNext = pxfc->pxfcNext;

        // With each node, check to see if it's element path matches the Id of some other node in the list
        pxfcCheck = *ppxfcHead;
        while (pxfcCheck)
        {
            if (0 == lstrcmpW(pxfc->pwzElementPath, pxfcCheck->wzId) && 0 == pxfc->iFlags
                && XMLCONFIG_CREATE & pxfcCheck->iFlags && XMLCONFIG_ELEMENT & pxfcCheck->iFlags)
            {
                // We found a match.  First, take it out of the current list
                if (pxfc->pxfcPrev)
                {
                    pxfc->pxfcPrev->pxfcNext = pxfc->pxfcNext;
                }
                else // it was the head.  Update the head
                {
                    *ppxfcHead = pxfc->pxfcNext;
                }

                if (pxfc->pxfcNext)
                {
                    pxfc->pxfcNext->pxfcPrev = pxfc->pxfcPrev;
                }

                pxfc->pxfcNext = NULL;
                pxfc->pxfcPrev = NULL;

                // Now, add this node to the end of the matched node's additional changes list
                if (!pxfcCheck->pxfcAdditionalChanges)
                {
                    pxfcCheck->pxfcAdditionalChanges = pxfc;
                    pxfcCheck->cAdditionalChanges = 1;
                }
                else
                {
                    pxfcLast = pxfcCheck->pxfcAdditionalChanges;
                    cAdditionalChanges = 1;
                    while (pxfcLast->pxfcNext)
                    {
                        pxfcLast = pxfcLast->pxfcNext;
                        cAdditionalChanges++;
                    }
                    pxfcLast->pxfcNext = pxfc;
                    pxfc->pxfcPrev = pxfcLast;
                    pxfcCheck->cAdditionalChanges = ++cAdditionalChanges;
                }
            }

            pxfcCheck = pxfcCheck->pxfcNext;
        }

        pxfc = pxfcNext;
    }

LExit:

    return hr;
}


static HRESULT BeginChangeFile(
    __in LPCWSTR pwzFile,
    __inout LPWSTR* ppwzCustomActionData
    )
{
    Assert(pwzFile && *pwzFile && ppwzCustomActionData);

    HRESULT hr = S_OK;

    LPBYTE pbData = NULL;
    DWORD cbData = 0;

    LPWSTR pwzRollbackCustomActionData = NULL;

    hr = WcaWriteIntegerToCaData((int)xaOpenFile, ppwzCustomActionData);
    ExitOnFailure(hr, "failed to write file indicator to custom action data");

    hr = WcaWriteStringToCaData(pwzFile, ppwzCustomActionData);
    ExitOnFailure1(hr, "failed to write file to custom action data: %S", pwzFile);

    // If the file already exits, then we have to put it back the way it was on failure
    if (FileExistsEx(pwzFile, NULL))
    {
        hr = FileRead(&pbData, &cbData, pwzFile);
        ExitOnFailure1(hr, "failed to read file: %S", pwzFile);

        // Set up the rollback for this file
        hr = WcaWriteStringToCaData(pwzFile, &pwzRollbackCustomActionData);
        ExitOnFailure1(hr, "failed to write file name to rollback custom action data: %S", pwzFile);

        hr = WcaWriteStreamToCaData(pbData, cbData, &pwzRollbackCustomActionData);
        ExitOnFailure(hr, "failed to write file contents to rollback custom action data.");

        hr = WcaDoDeferredAction(L"ExecXmlConfigRollback", pwzRollbackCustomActionData, COST_XMLFILE);
        ExitOnFailure1(hr, "failed to schedule ExecXmlConfigRollback for file: %S", pwzFile);

        ReleaseStr(pwzRollbackCustomActionData);
    }
LExit:
    if (NULL != pbData)
        MemFree(pbData);

    return hr;
}


static HRESULT WriteChangeData(
    __in XML_CONFIG_CHANGE* pxfc,
    __in eXmlAction action,
    __inout LPWSTR* ppwzCustomActionData
    )
{
    Assert(pxfc && ppwzCustomActionData);

    HRESULT hr = S_OK;
    XML_CONFIG_CHANGE* pxfcAdditionalChanges = NULL;

    hr = WcaWriteStringToCaData(pxfc->pwzElementPath, ppwzCustomActionData);
    ExitOnFailure1(hr, "failed to write ElementPath to custom action data: %S", pxfc->pwzElementPath);

    hr = WcaWriteStringToCaData(pxfc->pwzVerifyPath, ppwzCustomActionData);
    ExitOnFailure1(hr, "failed to write VerifyPath to custom action data: %S", pxfc->pwzVerifyPath);

    hr = WcaWriteStringToCaData(pxfc->wzName, ppwzCustomActionData);
    ExitOnFailure1(hr, "failed to write Name to custom action data: %S", pxfc->wzName);

    hr = WcaWriteStringToCaData(pxfc->pwzValue, ppwzCustomActionData);
    ExitOnFailure1(hr, "failed to write Value to custom action data: %S", pxfc->pwzValue);

    if (pxfc->iFlags & XMLCONFIG_CREATE && pxfc->iFlags & XMLCONFIG_ELEMENT && xaCreateElement == action && pxfc->pxfcAdditionalChanges)
    {
        hr = WcaWriteIntegerToCaData(pxfc->cAdditionalChanges, ppwzCustomActionData);
        ExitOnFailure(hr, "failed to write additional changes value to custom action data");

        pxfcAdditionalChanges = pxfc->pxfcAdditionalChanges;
        while (pxfcAdditionalChanges)
        {
            Assert((NULL == pxfcAdditionalChanges->wzComponent || 0 == lstrcmpW(pxfcAdditionalChanges->wzComponent, pxfc->wzComponent)) && 0 == pxfcAdditionalChanges->iFlags && (NULL == pxfcAdditionalChanges->wzFile || 0 == lstrcmpW(pxfcAdditionalChanges->wzFile, pxfc->wzFile)));

            hr = WcaWriteStringToCaData(pxfcAdditionalChanges->wzName, ppwzCustomActionData);
            ExitOnFailure1(hr, "failed to write Name to custom action data: %S", pxfc->wzName);

            hr = WcaWriteStringToCaData(pxfcAdditionalChanges->pwzValue, ppwzCustomActionData);
            ExitOnFailure1(hr, "failed to write Value to custom action data: %S", pxfc->pwzValue);

            pxfcAdditionalChanges = pxfcAdditionalChanges->pxfcNext;
        }
    }
    else
    {
        hr = WcaWriteIntegerToCaData(0, ppwzCustomActionData);
        ExitOnFailure(hr, "failed to write additional changes value to custom action data");
    }

LExit:
    return hr;
}


/******************************************************************
 SchedXmlConfig - entry point for XmlConfig Custom Action

********************************************************************/
extern "C" UINT __stdcall SchedXmlConfig(
    __in MSIHANDLE hInstall
    )
{
//    AssertSz(FALSE, "debug SchedXmlConfig");

    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    LPWSTR pwzCurrentFile = NULL;
    BOOL fCurrentFileChanged = FALSE;

    PMSIHANDLE hView = NULL;
    PMSIHANDLE hRec = NULL;

    XML_CONFIG_CHANGE* pxfcHead = NULL;
    XML_CONFIG_CHANGE* pxfcTail = NULL; // TODO: do we need this any more?
    XML_CONFIG_CHANGE* pxfc = NULL;

    eXmlAction xa = xaUnknown;

    LPWSTR pwzCustomActionData = NULL;

    DWORD cFiles = 0;

    // initialize
    hr = WcaInitialize(hInstall, "SchedXmlConfig");
    ExitOnFailure(hr, "failed to initialize");

    hr = ReadXmlConfigTable(&pxfcHead, &pxfcTail);
    MessageExitOnFailure(hr, msierrXmlConfigFailedRead, "failed to read XmlConfig table");

    hr = ProcessChanges(&pxfcHead);
    ExitOnFailure(hr, "failed to process XmlConfig changes");

    // loop through all the xml configurations
    for (pxfc = pxfcHead; pxfc; pxfc = pxfc->pxfcNext)
    {
        // If this is a different file, or the first file...
        if (NULL == pwzCurrentFile || 0 != lstrcmpW(pwzCurrentFile, pxfc->wzFile))
        {
            // Remember the file we're currently working on
            hr = StrAllocString(&pwzCurrentFile, pxfc->wzFile, 0);
            ExitOnFailure1(hr, "failed to copy file name: %S", pxfc->wzFile);

            fCurrentFileChanged = TRUE;
        }

        //
        // Figure out what action to take
        //
        xa = xaUnknown;

        // If it's being installed or reinstalled or uninstalled and that matches
        // what we are doing then calculate the right action.
        if ((XMLCONFIG_INSTALL & pxfc->iFlags && (WcaIsInstalling(pxfc->isInstalled, pxfc->isAction) || WcaIsReInstalling(pxfc->isInstalled, pxfc->isAction))) ||
            (XMLCONFIG_UNINSTALL & pxfc->iFlags && WcaIsUninstalling(pxfc->isInstalled, pxfc->isAction)))
        {
            if (XMLCONFIG_CREATE & pxfc->iFlags && XMLCONFIG_ELEMENT & pxfc->iFlags)
            {
                xa = xaCreateElement;
            }
            else if (XMLCONFIG_DELETE & pxfc->iFlags && XMLCONFIG_ELEMENT & pxfc->iFlags)
            {
                xa = xaDeleteElement;
            }
            else if (XMLCONFIG_DELETE & pxfc->iFlags && XMLCONFIG_VALUE & pxfc->iFlags)
            {
                xa = xaDeleteValue;
            }
            else if (XMLCONFIG_CREATE & pxfc->iFlags && XMLCONFIG_VALUE & pxfc->iFlags)
            {
                xa = xaWriteValue;
            }
            else if (XMLCONFIG_CREATE & pxfc->iFlags && XMLCONFIG_DOCUMENT & pxfc->iFlags)
            {
                xa = xaWriteDocument;
            }
            else if (XMLCONFIG_DELETE & pxfc->iFlags && XMLCONFIG_DOCUMENT & pxfc->iFlags)
            {
                hr = E_INVALIDARG;
                ExitOnFailure(hr, "Invalid flag configuration.  Cannot delete a fragment node.");
            }
        }

        if (xaUnknown != xa)
        {
            if (fCurrentFileChanged)
            {
                hr = BeginChangeFile(pwzCurrentFile, &pwzCustomActionData);
                ExitOnFailure1(hr, "failed to begin file change for file: %S", pwzCurrentFile);

                fCurrentFileChanged = FALSE;
                cFiles++;
            }

            hr = WcaWriteIntegerToCaData((int)xa, &pwzCustomActionData);
            ExitOnFailure(hr, "failed to write action indicator custom action data");


            hr = WriteChangeData(pxfc, xa, &pwzCustomActionData);
            ExitOnFailure(hr, "failed to write change data");
        }
    }

    // If we looped through all records all is well
    if (E_NOMOREITEMS == hr)
        hr = S_OK;
    ExitOnFailure(hr, "failed while looping through all objects to secure");

    // Schedule the custom action and add to progress bar
    if (pwzCustomActionData && *pwzCustomActionData)
    {
        Assert(0 < cFiles);

        hr = WcaDoDeferredAction(L"ExecXmlConfig", pwzCustomActionData, cFiles * COST_XMLFILE);
        ExitOnFailure(hr, "failed to schedule ExecXmlConfig action");
    }

LExit:
    ReleaseStr(pwzCurrentFile);
    ReleaseStr(pwzCustomActionData);

    FreeXmlConfigChangeList(pxfcHead);

    if (FAILED(hr))
    {
        er = ERROR_INSTALL_FAILURE;
    }
    return WcaFinalize(er);
}


/******************************************************************
 ExecXmlConfig - entry point for XmlConfig Custom Action

*******************************************************************/
extern "C" UINT __stdcall ExecXmlConfig(
    __in MSIHANDLE hInstall
    )
{
    //AssertSz(FALSE, "debug ExecXmlConfig");
    HRESULT hr = S_OK;
    HRESULT hrOpenFailure = S_OK;
    UINT er = ERROR_SUCCESS;

    LPWSTR pwzCustomActionData = NULL;
    LPWSTR pwzData = NULL;
    LPWSTR pwzFile = NULL;
    LPWSTR pwzElementPath = NULL;
    LPWSTR pwzVerifyPath = NULL;
    LPWSTR pwzName = NULL;
    LPWSTR pwzValue = NULL;
    LPWSTR pwz = NULL;
    int cAdditionalChanges = 0;

    IXMLDOMDocument* pixd = NULL;
    IXMLDOMNode* pixn = NULL;
    IXMLDOMNode* pixnVerify = NULL;
    IXMLDOMNode* pixnNewNode = NULL;

    IXMLDOMDocument* pixdNew = NULL;
    IXMLDOMElement* pixeNew = NULL;
    BOOL fNamespaceExisted = FALSE;

    int i = 0;

    eXmlAction xa;

    // initialize
    hr = WcaInitialize(hInstall, "ExecXmlConfig");
    ExitOnFailure(hr, "failed to initialize");

    hr = XmlInitialize();
    ExitOnFailure(hr, "failed to initialize xml utilities");

    hr = WcaGetProperty( L"CustomActionData", &pwzCustomActionData);
    ExitOnFailure(hr, "failed to get CustomActionData");

    WcaLog(LOGMSG_TRACEONLY, "CustomActionData: %S", pwzCustomActionData);

    pwz = pwzCustomActionData;

    hr = WcaReadIntegerFromCaData(&pwz, (int*) &xa);
    ExitOnFailure(hr, "failed to process CustomActionData");

    if (xaOpenFile != xa)
    {
        ExitOnFailure(hr = E_INVALIDARG, "invalid custom action data");
    }

    // loop through all the passed in data
    while (pwz && *pwz)
    {
        hr = WcaReadStringFromCaData(&pwz, &pwzFile);
        ExitOnFailure(hr, "failed to read file name from custom action data");

        // Open the file
        ReleaseNullObject(pixd);

        hr = XmlLoadDocumentFromFile(pwzFile, &pixd);
        if (FAILED(hr))
        {
            // Ignore the return code for now.  If they try to add something, we'll fail the install.  If all they do is remove stuff then it doesn't matter.
            hrOpenFailure = hr;
            hr = S_OK;
        }
        else
        {
            hrOpenFailure = S_OK;
        }

        WcaLog(LOGMSG_VERBOSE, "Configuring Xml File: %S", pwzFile);

        while (pwz && *pwz)
        {
            // If we skip past an element that has additional changes we need to strip them off the stream before
            // moving on to the next element. Do that now and then restart the outer loop.
            if (cAdditionalChanges > 0)
            {
                while (cAdditionalChanges > 0)
                {
                    hr = WcaReadStringFromCaData(&pwz, &pwzName);
                    ExitOnFailure(hr, "failed to process CustomActionData");
                    hr = WcaReadStringFromCaData(&pwz, &pwzValue);
                    ExitOnFailure(hr, "failed to process CustomActionData");

                    cAdditionalChanges--;
                }
                continue;
            }

            hr = WcaReadIntegerFromCaData(&pwz, (int*) &xa);
            ExitOnFailure(hr, "failed to process CustomActionData");

            // Break if we need to move on to a different file
            if (xaOpenFile == xa)
                break;

            // Get path, name, and value to be written
            hr = WcaReadStringFromCaData(&pwz, &pwzElementPath);
            ExitOnFailure(hr, "failed to process CustomActionData");
            hr = WcaReadStringFromCaData(&pwz, &pwzVerifyPath);
            ExitOnFailure(hr, "failed to process CustomActionData");
            hr = WcaReadStringFromCaData(&pwz, &pwzName);
            ExitOnFailure(hr, "failed to process CustomActionData");
            hr = WcaReadStringFromCaData(&pwz, &pwzValue);
            ExitOnFailure(hr, "failed to process CustomActionData");
            hr = WcaReadIntegerFromCaData(&pwz, &cAdditionalChanges);
            ExitOnFailure(hr, "failed to process CustomActionData")

            // If we failed to open the file and we're adding something to the file, we've got a problem.  Otherwise, just continue on since the file's already gone.
            if (FAILED(hrOpenFailure))
            {
                if (xaCreateElement == xa || xaWriteValue == xa || xaWriteDocument == xa)
                {
                    MessageExitOnFailure1(hr = hrOpenFailure, msierrXmlConfigFailedOpen, "failed to load XML file: %S", pwzFile);
                }
                else
                {
                    continue;
                }
            }

            // Select the node we're about to modify
            ReleaseNullObject(pixn);

            hr = XmlSelectSingleNode(pixd, pwzElementPath, &pixn);

            // If we failed to find the node that we are going to add to, we've got a problem. Otherwise, just continue since the node's already gone.
            if (S_FALSE == hr)
            {
                if (xaCreateElement == xa || xaWriteValue == xa || xaWriteDocument == xa)
                {
                    hr = HRESULT_FROM_WIN32(ERROR_OBJECT_NOT_FOUND);
                }
                else
                {
                    hr = S_OK;
                    continue;
                }
            }

            MessageExitOnFailure2(hr, msierrXmlConfigFailedSelect, "failed to find node: %S in XML file: %S", pwzElementPath, pwzFile);

            // Make the modification
            switch (xa)
            {
            case xaWriteValue:
                if (pwzName && *pwzName)
                {
                    // We're setting an attribute
                    hr = XmlSetAttribute(pixn, pwzName, pwzValue);
                    ExitOnFailure2(hr, "failed to set attribute: %S to value %S", pwzName, pwzValue);
                }
                else
                {
                    // We're setting the text of the node
                    hr = XmlSetText(pixn, pwzValue);
                    ExitOnFailure2(hr, "failed to set text to: %S for element %S.  Make sure that XPath points to an elment.", pwzValue, pwzElementPath);
                }
                break;
            case xaWriteDocument:
                hr = XmlLoadDocument(pwzValue, &pixdNew);
                ExitOnFailure(hr, "Failed to load value as document.");

                hr = pixdNew->get_documentElement(&pixeNew);
                ExitOnFailure(hr, "Failed to get document element.");

                hr = pixn->appendChild(pixeNew, NULL);
                ExitOnFailure(hr, "Failed to append document element on to parent element.");

                ReleaseNullObject(pixeNew);
                ReleaseNullObject(pixdNew);
                break;

            case xaCreateElement:
                if (NULL != pwzVerifyPath && 0 != pwzVerifyPath[0])
                {
                    hr = XmlSelectSingleNode(pixn, pwzVerifyPath, &pixnVerify);
                    if (S_OK == hr)
                    {
                        // We found the verify path which means we have no further work to do
                        continue;
                    }
                    ExitOnFailure1(hr, "failed to query verify path: %S", pwzVerifyPath);
                }

                hr = XmlCreateChild(pixn, pwzName, &pixnNewNode);
                ExitOnFailure1(hr, "failed to create child element: %S", pwzName);

                if (pwzValue && *pwzValue)
                {
                    hr = XmlSetText(pixnNewNode, pwzValue);
                    ExitOnFailure2(hr, "failed to set text to: %S for node: %S", pwzValue, pwzName);
                }

                while (cAdditionalChanges > 0)
                {
                    hr = WcaReadStringFromCaData(&pwz, &pwzName);
                    ExitOnFailure(hr, "failed to process CustomActionData");
                    hr = WcaReadStringFromCaData(&pwz, &pwzValue);
                    ExitOnFailure(hr, "failed to process CustomActionData");

                    // Set the additional attribute
                    hr = XmlSetAttribute(pixnNewNode, pwzName, pwzValue);
                    ExitOnFailure2(hr, "failed to set attribute: %S to value %S", pwzName, pwzValue);

                    cAdditionalChanges--;
                }

                ReleaseNullObject(pixnNewNode);
                break;
            case xaDeleteValue:
                if (pwzName && *pwzName)
                {
                    // Delete the attribute
                    hr = XmlRemoveAttribute(pixn, pwzName);
                    ExitOnFailure1(hr, "failed to remove attribute: %S", pwzName);
                }
                else
                {
                    // Clear the text value for the node
                    hr = XmlSetText(pixn, L"");
                    ExitOnFailure(hr, "failed to clear text value");
                }
                break;
            case xaDeleteElement:
                if (NULL != pwzVerifyPath && 0 != pwzVerifyPath[0])
                {
                    hr = XmlSelectSingleNode(pixn, pwzVerifyPath, &pixnVerify);
                    if (S_OK == hr)
                    {
                        hr = pixn->removeChild(pixnVerify, NULL);
                        ExitOnFailure(hr, "failed to remove created child element");
                    }
                    else
                    {
                        WcaLog(LOGMSG_VERBOSE, "Failed to select path %S for deleting.  Skipping...", pwzVerifyPath);
                        hr = S_OK;
                    }
                }
                else
                {
                    // TODO: This requires a VerifyPath to delete an element.  Should we support not having one?
                    WcaLog(LOGMSG_VERBOSE, "No VerifyPath specified for delete element of ID: %S", pwzElementPath);
                }
                break;
            default:
                ExitOnFailure(hr = E_UNEXPECTED, "Invalid modification specified in custom action data");
                break;
            }
        }

        // Now that we've made all of the changes to this file, save it and move on to the next
        if (S_OK == hrOpenFailure)
        {
            hr = XmlSaveDocument(pixd, pwzFile);
            MessageExitOnFailure1(hr, msierrXmlConfigFailedSave, "failed to save changes to XML file: %S", pwzFile);
        }
    }

LExit:
    ReleaseStr(pwzCustomActionData);
    ReleaseStr(pwzData);
    ReleaseStr(pwzFile);
    ReleaseStr(pwzElementPath);
    ReleaseStr(pwzVerifyPath);
    ReleaseStr(pwzName);
    ReleaseStr(pwzValue);

    ReleaseObject(pixeNew);
    ReleaseObject(pixdNew);

    ReleaseObject(pixn);
    ReleaseObject(pixd);
    ReleaseObject(pixnNewNode);

    XmlUninitialize();

    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}


/******************************************************************
 ExecXmlConfigRollback - entry point for XmlConfig rollback Custom Action

*******************************************************************/
extern "C" UINT __stdcall ExecXmlConfigRollback(
    __in MSIHANDLE hInstall
    )
{
//    AssertSz(FALSE, "debug ExecXmlConfigRollback");
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    LPWSTR pwzCustomActionData = NULL;
    LPWSTR pwz = NULL;
    LPWSTR pwzFileName = NULL;
    LPBYTE pbData = NULL;
    DWORD_PTR cbData = 0;
    DWORD cbDataWritten = 0;

    HANDLE hFile = INVALID_HANDLE_VALUE;

    // initialize
    hr = WcaInitialize(hInstall, "ExecXmlConfig");
    ExitOnFailure(hr, "failed to initialize");


    hr = WcaGetProperty( L"CustomActionData", &pwzCustomActionData);
    ExitOnFailure(hr, "failed to get CustomActionData");

    WcaLog(LOGMSG_TRACEONLY, "CustomActionData: %S", pwzCustomActionData);

    pwz = pwzCustomActionData;

    hr = WcaReadStringFromCaData(&pwz, &pwzFileName);
    ExitOnFailure(hr, "failed to read file name from custom action data");

    hr = WcaReadStreamFromCaData(&pwz, &pbData, &cbData);
    ExitOnFailure(hr, "failed to read file contents from custom action data");

    // Open the file
    hFile = ::CreateFileW(pwzFileName, GENERIC_WRITE, NULL, NULL, TRUNCATE_EXISTING, NULL, NULL);
    if (INVALID_HANDLE_VALUE == hFile)
        ExitOnLastError1(hr, "failed to open file: %S", pwzFileName);

    // Write out the old data
    if (!::WriteFile(hFile, pbData, (DWORD)cbData, &cbDataWritten, NULL))
        ExitOnLastError1(hr, "failed to write to file: %S", pwzFileName);

    Assert(cbData == cbDataWritten);

LExit:
    ReleaseStr(pwzCustomActionData);
    ReleaseStr(pwzFileName);

    if (INVALID_HANDLE_VALUE != hFile)
        ::CloseHandle(hFile);

    if (NULL != pbData)
        MemFree(pbData);

    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}

