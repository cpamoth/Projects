using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class RegionCheckMethods
    {
        public static void CheckForRegionDiscrepancies(string line, int lineNumber)
        {
            bool error = false;
            string[] regions = HelperMethods.CheckConfiguration("regions").Split(',');
            foreach (string r in regions)
            {
                if (IgnoreMethods.Ignore(r) == false)
                {
                    error = LineContainsRegion(line, error, r);

                    if (error == true)
                    {
                        Errors.RegionDiscrepancy(lineNumber, r, line);
                    }
                }
            }
        }

        private static bool LineContainsRegion(string line, bool error, string r)
        {
            line = line.ToUpper();

            if (r.ToUpper() != Program.Region.ToUpper())
            {
                foreach (string i in Program.Ignores)
                {
                    if (line.Contains(i.ToUpper()))
                    {
                        line = line.Replace(i.ToUpper(), "");
                    }
                }

                if (line.Contains(r.ToUpper()))
                {
                    error = true;
                }
            }

            return error;
        }
    }
}
