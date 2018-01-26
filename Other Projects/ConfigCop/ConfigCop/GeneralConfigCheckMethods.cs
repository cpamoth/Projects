using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class GeneralConfigCheckMethods
    {
        public static void CheckForDuplicateKeys()
        {
            if (Program.AppSettingsCollection != null && Program.AppSettingsCollection.Count() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine(Program.AppSettingsCollection.Count() + " AppSettings keys found.  Checking for duplicates...");
                Console.ResetColor();

                var keyGroups = from k in Program.AppSettingsCollection group k by k.KeyName;
                foreach (var grp in keyGroups)
                {
                    var valGroups = from v in grp group v by v.KeyValue;
                    if (valGroups.Count() > 1)
                    {
                        Errors.DuplicateKeyWarning(grp);
                    }
                }
            }
        }
    }
}
