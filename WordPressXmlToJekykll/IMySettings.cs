using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressXmlToJekykll
{
    public interface IMySettings
    {
        string InputFile { get; set; }
        string OutputFolder { get; set; }
        string PanDocPath { get; set; }
        string DownloadImage { get; set; }
    }
}
