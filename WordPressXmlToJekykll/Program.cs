using Config.Net;
using Pandoc;

namespace WordPressXmlToJekykll
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WordPressXmlToStaticFile");
            if (!Directory.Exists(jsonFilePath))
            {
                Directory.CreateDirectory(jsonFilePath);
            }
            jsonFilePath = Path.Combine(jsonFilePath, "settings.json");
            IMySettings settings = new ConfigurationBuilder<IMySettings>()
                .UseAppConfig()
                .UseJsonFile(jsonFilePath)
                .UseCommandLineArgs()
                .Build();
            settings.InputFile = PromptForTextValue("Enter Path for the XML file exported by WordPress (Press Enter for default):[{0}]", settings.InputFile);
            settings.OutputFolder = PromptForTextValue("Enter Path for the output folder(Press Enter for default):[{0}]", settings.OutputFolder);
            settings.PanDocPath = PromptForTextValue("Enter Path for the pandoc executable (maybe c:\\Program Files\\Pandoc\\, Press Enter for default):[{0}]", settings.PanDocPath);
            settings.DownloadImage= PromptForTextValue("Download image?[y/n], Press Enter for default):[{0}]", settings.DownloadImage);
            PandocEngine pandocEngine = new PandocEngine(settings.PanDocPath);
            WordPressXmlToJekykll wpXmlToJekykll = new WordPressXmlToJekykll(settings, pandocEngine);
            await WordPressXmlToJekykll.Convert();
        }
        static string PromptForTextValue(string prompt, string defaultValue)
        {
            Console.WriteLine(string.Format(prompt, defaultValue));
            var result = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(result))
            {
                return defaultValue;
            }
            return result;
        }
        static bool PromptForBooleanValue(string prompt, bool defaultValue)
        {
            Console.WriteLine(string.Format(prompt, defaultValue ? "y" : "n"));
            var result = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(result))
            {
                return defaultValue;
            }
            switch (result.ToLower())
            {
                case "true":
                    return true;
                case "false":
                    return false;
                case "y":
                case "t":
                    return true;
                case "n":
                case "f":
                    return false;
                default:
                    return defaultValue;
            }
        }
        static int PromptForNumber(string prompt, int defaultValue)
        {
            Console.WriteLine(string.Format(prompt, defaultValue));
            var resultString = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(resultString))
            {
                return defaultValue;
            }
            int result = 0;
            if (int.TryParse(resultString.Trim(), out result))
            {
                switch (result)
                {
                    case 1:
                    case 2:
                    case 3:
                        return result;
                    default:
                        break;
                }
            }
            return defaultValue;
        }
    }
}
