using Config.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressXmlToStaticFile
{
    public interface IMySettings
    {
        string InputFile { get; set; }
        string OutputFolder{ get; set; }
        string PanDocPath { get; set; }
        bool CreateYearFolders { get; set; }
        bool CreateMonthFolders { get; set; }

        [Option(DefaultValue = "1")]
        int OutputFormat { get; set; }
    }
}
