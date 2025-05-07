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
        [Option(DefaultValue = "True")]
        bool CreateYearFolders { get; set; }
        [Option(DefaultValue = "True")]
        bool CreateMonthFolders { get; set; }

        [Option(DefaultValue = "True")]
        bool CreateDayFolders { get; set; }
        

        [Option(DefaultValue = "1")]
        int OutputFormat { get; set; }

        bool CreateRedirectForABlog { get; set; }
        string SphinixSourceFolder { get; set; }
        string SphinixBuildFolder { get; set; }
    }
}
