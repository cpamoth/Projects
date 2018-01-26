#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scacost.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Costs for various server custom actions.
// </summary>
//-------------------------------------------------------------------------------------------------

const UINT COST_IIS_TRANSACTIONS = 10000;

const UINT COST_IIS_CREATEKEY = 5000;
const UINT COST_IIS_DELETEKEY = 5000;
const UINT COST_IIS_WRITEKEY = 5000;
const UINT COST_IIS_CREATEAPP = 5000;

const UINT COST_SQL_CREATEDB = 10000;
const UINT COST_SQL_DROPDB = 5000;
const UINT COST_SQL_CONNECTDB = 5000;
const UINT COST_SQL_STRING = 5000;

const UINT COST_PERFMON_REGISTER = 1000;
const UINT COST_PERFMON_UNREGISTER = 1000;

const UINT COST_SMB_CREATESMB = 10000;
const UINT COST_SMB_DROPSMB = 5000;

const UINT COST_CERT_ADD = 5000;
const UINT COST_CERT_DELETE = 5000;

const UINT COST_USER_ADD = 10000;
const UINT COST_USER_DELETE = 10000;

