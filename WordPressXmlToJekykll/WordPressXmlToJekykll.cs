using Pandoc;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace WordPressXmlToJekykll
{
    public class WordPressXmlToJekykll
    {
        public IMySettings Settings { get; set; }
        public PandocEngine PandocEngine { get; set; }
        public WordPressXmlToJekykll(IMySettings settings, PandocEngine pandocEngine)
        {
            Settings = settings;
            PandocEngine = pandocEngine;
        }
        public async Task Convert()
        {
            var outputPath = Settings.OutputFolder;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            if (string.IsNullOrWhiteSpace(Settings.InputFile))
            {
                Console.WriteLine("Input file is not specified.");
                return;
            }
            if (!File.Exists(Settings.InputFile))
            {
                Console.WriteLine("Input file does not exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Settings.PanDocPath))
            {
                Console.WriteLine("Pandoc path is not specified.");
                return;
            }
            if (!File.Exists(Settings.PanDocPath))
            {
                Console.WriteLine("Pandoc path does not exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Settings.OutputFolder))
            {
                Console.WriteLine("Output folder is not specified.");
                return;
            }
            
            WordPressXml wordPressXml = WordPressXml.FromFile(Settings.InputFile);
            if (wordPressXml.Items != null)
            {

                foreach (var item in wordPressXml.Items)
                {
                    switch (item.PostType)
                    {
                        case "post":
                            WritePost(item, wordPressXml, Settings, PandocEngine);
                            break;
                        case "attachment":
                            WriteAttachment(item, wordPressXml, Settings, PandocEngine);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("No items found in the XML file.");
            }
    }

        private void WriteAttachment(WordPressItem item, WordPressXml wordPressXml, IMySettings settings, PandocEngine pandocEngine)
        {
            var outputPath = settings.OutputFolder;
        }

        private void WritePost(WordPressItem item, WordPressXml wordPressXml, IMySettings settings, PandocEngine pandocEngine)
        {
            //most images/attachments in wordpress are formatted as ahref wrapping a img. In these cases, the img tag has info about the thumbnail... while the ahref points to the actual file.
        }
    }
