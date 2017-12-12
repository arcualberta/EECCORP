using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECCORP.Services
{
    public class HelperService
    {
        public string GetCSVLine(string[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = string.Format("\"{0}\",", values[i].Replace("\"", "\"\""));
            }
            return String.Join(",", values) + "\n";
        }

        public string GetReportFileName(string name)
        {
            return string.Format("{0}_Report-{1}.csv", (string)name, DateTime.Now.ToString("yyyyMMddHHmm"));
        }
    }
}