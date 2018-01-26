using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace TableCreateLib
{
    public class Creator
    {
        public string Database { get; set; }
        private string _tableName;
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value.Replace("_base", ""); }
        }
        public List<string> Columns { get; private set; }
        public string PrimaryKey { get; set; }
        private string _storageDirectory;

        public static bool IsFullDescription(string colName)
        {
            if (colName.Contains(" "))
                return true;
            return false;
        }

        public Creator ()
        {
            Columns = new List<string>();
        }

        public void Create(string directory)
        {
            Create(directory, true);
        }

        public void Create(string directory, bool createArchive)
        {
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _storageDirectory = directory;
            //create table
            BuildTable();
            if (createArchive)
            {
                //create audit
                BuildAuditTable();
                //create triggers
                BuildTriggers();
            }
            //create view
            BuildView();
        }

        private string GetColumnListForTable()
        {
            Regex prettyColumn = new Regex(@"(?<cn>.+?)\s(?<dt>.+?)(?<len>\(.+?\))?(\s(?<rest>.+)|$)");
            for (int i = 0; i < Columns.Count;i++ )
            {
                Match pc = prettyColumn.Match(Columns[i]);
                Columns[i] = prettyColumn.Replace(Columns[i], "[${cn}] [${dt}]${len} ${rest}")
                    .Replace(pc.Groups["dt"].Value, pc.Groups["dt"].Value.ToUpper())
                    .Replace(pc.Groups["rest"].Value, pc.Groups["rest"].Value.ToUpper())
                    .Replace("]]", "]")
                    .Replace("[[", "[");
            }

            return String.Join(",\r\n    ", Columns.ToArray());
        }

        private string GetColumnListForView()
        {
            string[] cols = new string[Columns.Count];
            int i = 0;
            foreach(string column in Columns)
            {
                cols[i++] = column.Split(' ')[0];
            }
            return String.Join(",\r\n    ",cols);
        }

        private void BuildTable()
        {

            string script = String.Format(DropBase, Database, TableName) +
                String.Format(BuildTableScript, Database, TableName, GetColumnListForTable(), GetPrimaryKeyString());

            SaveFile("1Table",TableName + "_base.sql", script, false);

            script = String.Format(DropBase, Database, TableName);
            SaveFile("1Table",TableName + "_base.rollback.sql", script, true);

        }


        private void BuildAuditTable()
        {
            string script = String.Format(DropAudit, Database, TableName) + 
                String.Format(AuditTable,Database,TableName,GetColumnListForTable());
            SaveFile("1Table",TableName + "_base_audit.sql", script, false);

            script = String.Format(DropAudit, Database, TableName);
            SaveFile("1Table",TableName + "_base_audit.rollback.sql", script, true);

        }

        private void BuildTriggers()
        {
            string script = String.Format(DropUpdateTrigger, Database, TableName) +
                String.Format(UpdateTrigger, Database, TableName);
            SaveFile("3Trigger",TableName + "_AuditUpdates_base.sql", script, false);

            script = String.Format(DropDeleteTrigger, Database, TableName) +
                String.Format(DeleteTrigger, Database, TableName);
            SaveFile("3Trigger", TableName + "_AuditDeletes_base.sql", script, false);

            script = String.Format(DropUpdateTrigger, Database, TableName);
            SaveFile("3Trigger", TableName + "_AuditUpdates_base.rollback.sql", script, true);

            script = String.Format(DropDeleteTrigger, Database, TableName);
            SaveFile("3Trigger", TableName + "_AuditDeletes_base.rollback.sql", script, true);
        }

        private void BuildView()
        {
            string script = String.Format(DropView, Database, TableName) +
                String.Format(View, Database, TableName, GetColumnListForView());
            SaveFile("2View",TableName + ".sql", script, false);

            script = String.Format(DropView, Database, TableName);
            SaveFile("2View",TableName + ".rollback.sql", script, true);
        }

        public bool UseDirectories { get; set; }
        public string Server { get; set; }

        private void SaveFile(string directoryAddition, string fileName, string script, bool rollback)
        {
            if (UseDirectories)
            {
                string stor = Path.Combine(_storageDirectory, directoryAddition);
                if (!Directory.Exists(stor))
                    Directory.CreateDirectory(stor);
                fileName = Path.Combine(stor, fileName);
            }
            else
            {
                string lastDir = Path.GetFileName(_storageDirectory);
                if (lastDir != null && !lastDir.Equals("ChangeScripts", StringComparison.CurrentCultureIgnoreCase))
                {
                    _storageDirectory = Path.Combine(_storageDirectory, "ChangeScripts");
                    if (!Directory.Exists(_storageDirectory))
                        Directory.CreateDirectory(_storageDirectory);
                }

                string stor = Path.Combine(_storageDirectory, rollback ? "Rollback" : "Upgrade");
                
                if (!Directory.Exists(stor))
                    Directory.CreateDirectory(stor);

                stor = Path.Combine(stor, Server);

                if (!Directory.Exists(stor))
                    Directory.CreateDirectory(stor);
                
                fileName = Path.Combine(stor, String.Concat(directoryAddition, ".", fileName));
            }
            

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (FileStream fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(script);
                    sw.Close();
                }
                fs.Close();
            }
            Trace.WriteLine(fileName + " created");
        }

        private string GetPrimaryKeyString()
        {
            if(String.IsNullOrWhiteSpace(PrimaryKey))
            {
                return String.Empty;
            }
            const string pkstr = @",
 CONSTRAINT [PK_{0}_base] PRIMARY KEY CLUSTERED 
(
    {1}
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 70) ON [PRIMARY]";

            return String.Format(pkstr, TableName, PrimaryKey);
        }

        private const string DropView = @"USE [{0}]
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[{1}]'))
DROP VIEW [dbo].[{1}]
GO
";
        private const string View = @"USE [{0}]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[{1}] AS
    
SELECT 
{2}
FROM [dbo].[{1}_base]


GO

GRANT SELECT ON [dbo].[{1}] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT INSERT ON [dbo].[{1}] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT UPDATE ON [dbo].[{1}] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT DELETE ON [dbo].[{1}] TO [DBRole_GISWEBUsers] AS [dbo]
GO

";

        private const string DropDeleteTrigger = @"USE [{0}]
GO

IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AuditDeletes{1}_base]'))
DROP TRIGGER [dbo].[AuditDeletes{1}_base]
GO

";
        private const string DropUpdateTrigger = @"USE [{0}]
GO

IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AuditUpdates{1}_base]'))
DROP TRIGGER [dbo].[AuditUpdates{1}_base]
GO
";

        private const string DeleteTrigger = @"USE [{0}]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[AuditDeletes{1}_base] ON [dbo].[{1}_base] 
FOR  delete
AS
insert {1}_base_audit
select 
'deleted' as delete_or_update,
SYSDATETIME(),
SUser_Sname() as ModifiedBy,
host_name() as hostname,
* from deleted


GO

";

        private const string UpdateTrigger = @"USE [{0}]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[AuditUpdates{1}_base] ON [dbo].[{1}_base] 
FOR  update
AS
insert {1}_base_audit
select 
'updated' as delete_or_update,
SYSDATETIME(),
SUser_Sname() as ModifiedBy,
host_name() as hostname,
* from deleted


GO

";

        private const string BuildTableScript = @"USE [{0}]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[{1}_base](
    {2}{3}
) ON [PRIMARY]

GO

GRANT SELECT ON [dbo].[{1}_base] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT INSERT ON [dbo].[{1}_base] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT UPDATE ON [dbo].[{1}_base] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT DELETE ON [dbo].[{1}_base] TO [DBRole_GISWEBUsers] AS [dbo]
GO

SET ANSI_PADDING OFF
GO

";
        private const string DropBase = @"
USE [{0}]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{1}_base]') AND type in (N'U'))
DROP TABLE [dbo].[{1}_base]
GO

";
        private const string AuditTable = @"
USE [{0}]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[{1}_base_audit](
    [DeletedOrUpdated] [NVARCHAR](25) NOT NULL,
    [ModifiedDate] [DATETIME2](6) NOT NULL,
    [ModifiedBy] [NVARCHAR](255) NOT NULL,
    [HostName] [NVARCHAR](255) NOT NULL,
    {2}
) ON [PRIMARY]

GO

GRANT SELECT ON [dbo].[{1}_base_audit] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT INSERT ON [dbo].[{1}_base_audit] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT UPDATE ON [dbo].[{1}_base_audit] TO [DBRole_GISWEBUsers] AS [dbo]
GO

GRANT DELETE ON [dbo].[{1}_base_audit] TO [DBRole_GISWEBUsers] AS [dbo]
GO

SET ANSI_PADDING OFF
GO

";
        private const string DropAudit = @"
USE [{0}]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{1}_base_audit]') AND type in (N'U'))
DROP TABLE [dbo].[{1}_base_audit]
GO

";
    }
}
