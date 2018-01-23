//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************
using System;
using System.Reflection;
using System.Resources;

using System;
[assembly: AssemblyTitle("XML Documentation Tool")]
[assembly: AssemblyDescription("Creates and manages XML Documentation")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("Copyright (C) Microsoft Corp. 1999-2002.  All Rights Reserved.")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' shown below:

[assembly: AssemblyVersion("1.0.*")]
#region " Helper class to get information for the About form. "

// This class uses the System.Reflection.Assembly class to

// access assembly meta-data

// This class is not a normal feature of AssemblyInfo.cs

public class AssemblyInfo {

    // Used by Helper Functions to access information from Assembly Attributes

    private Type myType;

    public AssemblyInfo() {

        myType = typeof(MainWindow);

    }

    public string AsmName {

        get {

            return myType.Assembly.GetName().Name.ToString();

        }

    }

    public string AsmFQName {

        get {

            return myType.Assembly.GetName().FullName.ToString();

        }

    }

    public string CodeBase {

        get {

            return myType.Assembly.CodeBase;

        }

    }

    public string Copyright {

        get {

            Type at = typeof(AssemblyCopyrightAttribute);

            object[] r = myType.Assembly.GetCustomAttributes(at, false);

            AssemblyCopyrightAttribute ct = (AssemblyCopyrightAttribute) r[0];

            return ct.Copyright;

        }

    }

    public string Company {

        get {

            Type at = typeof(AssemblyCopyrightAttribute);

            object[] r = myType.Assembly.GetCustomAttributes(at, false);

            AssemblyCompanyAttribute ct = (AssemblyCompanyAttribute) r[0];

            return ct.Company;

        }

    }

    public string Description {

        get {

            Type at = typeof(AssemblyDescriptionAttribute);

            object[] r = myType.Assembly.GetCustomAttributes(at, false);

            AssemblyDescriptionAttribute da = (AssemblyDescriptionAttribute) r[0];

            return da.Description;

        }

    }

    public string Product {

        get {

            Type at = typeof(AssemblyProductAttribute);

            object[] r = myType.Assembly.GetCustomAttributes(at, false);

            AssemblyProductAttribute pt = (AssemblyProductAttribute) r[0];

            return pt.Product;

        }

    }

    public string Title {

        get {

            Type at = typeof(AssemblyTitleAttribute);

            object[] r = myType.Assembly.GetCustomAttributes(at, false);

            AssemblyTitleAttribute ta = (AssemblyTitleAttribute) r[0];

            return ta.Title;

        }

    }

    public string Version {

        get {

            return myType.Assembly.GetName().Version.ToString();

        }

    }

}

#endregion
