using Pandoc;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace WordPressXmlToStaticFile
{
    public class WordPressXmlToStaticFile
    {
        public IMySettings Settings { get; set; }
        public PandocEngine PandocEngine  { get; set; }
        public WordPressXmlToStaticFile(IMySettings settings, PandocEngine pandocEngine)
        {
            Settings = settings;
            PandocEngine = pandocEngine;
        }
        public async Task Convert()
        {  
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Settings.InputFile);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("wp", "http://wordpress.org/export/1.2/");
            nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
            nsmgr.AddNamespace("excerpt", "http://wordpress.org/export/1.2/excerpt/");
            nsmgr.AddNamespace("wfw", "http://wellformedweb.org/CommentAPI/");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");


            var channel= xmlDocument.SelectSingleNode("/rss/channel");
            var posts = channel.SelectNodes("item");
            foreach (XmlNode post in posts)
            {
                var postType = post.SelectSingleNode("wp:post_type", nsmgr);
                if (postType != null)
                {
                    if (postType.ChildNodes.Count > 0)
                    {
                        if (string.Compare(postType.ChildNodes[0].Value, "post") != 0)
                        {
                            continue;
                        }
                    }
                    else
                        continue;
                }
                else 
                    continue;

                XmlCDataSection cDataTitleNode = post.SelectSingleNode("title").ChildNodes[0] as XmlCDataSection;
                string title = cDataTitleNode.Data;
                XmlCDataSection cDataContentNode = post.SelectSingleNode("content:encoded", nsmgr).ChildNodes[0] as XmlCDataSection;
                string content = cDataContentNode.Data;
                DateTime? postDate=null;
                var postDateElement= post.SelectSingleNode("wp:post_date", nsmgr);
                if (postDateElement != null && postDateElement.ChildNodes.Count == 1)
                {
                    var postDateString = postDateElement.ChildNodes[0].Value;
                    DateTime test;
                    if(DateTime.TryParse(postDateString,out test))
                    {
                        postDate = test;
                    }
                }
                var guidElement = post.SelectSingleNode("guid");
                string fileName = string.Empty;
                if (guidElement != null)
                {
                    if (guidElement.ChildNodes.Count == 1)
                    {
                        var postUrl = new Uri(guidElement.ChildNodes[0].Value);
                        fileName = Path.GetFileName(postUrl.AbsolutePath);
                        
                    }
                }
                Debug.WriteLine(string.Format("Processing post: {0}", title));
                if(postDate.HasValue)
                    Debug.WriteLine(string.Format("published on : {0}", postDate));
                Debug.WriteLine(string.Format("post content: {0}", content));
                Debug.WriteLine(string.Format("Save as fileName: {0}", fileName));
                string resultText = string.Empty;
                var targetPath = Settings.OutputFolder;
                if (postDate.HasValue)
                {
                    if (Settings.CreateYearFolders)
                    {

                        targetPath = Path.Combine(targetPath, postDate.Value.Year.ToString());
                    }
                    if (Settings.CreateMonthFolders)
                    {
                        targetPath = Path.Combine(targetPath, postDate.Value.Month.ToString());
                    }
                }
                if(!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }   
                byte[] encodedText = null;
                var tempFileName = Path.GetTempFileName();
                switch (this.Settings.OutputFormat)
                {
                    case 1:
                        targetPath = Path.Combine(targetPath, fileName);
                        if (!Directory.Exists(targetPath))
                        {
                            Directory.CreateDirectory(targetPath);
                        }

                        targetPath = Path.Combine(targetPath, "index.html");
                        resultText = string.Format("<!doctype html><html><head><meta charset=\"UTF-8\"<title>{0}</head><body>{1}</body></html>", title, content);
                        encodedText = Encoding.UTF8.GetBytes(resultText);
                        File.WriteAllBytes(targetPath,encodedText);
                        break;
                    case 2:
                        targetPath = Path.Combine(targetPath, fileName);
                        if (!Directory.Exists(targetPath))
                        {
                            Directory.CreateDirectory(targetPath);
                        }
                        targetPath = Path.Combine(targetPath, "index.md");
                        File.WriteAllText(tempFileName, content);
                        await PandocInstance.Convert<HtmlIn, GhMdOut>(tempFileName,targetPath);
                        File.Delete(tempFileName);
                        break;
                    case 3:
                        targetPath = Path.Combine(targetPath, fileName);
                        if (!Directory.Exists(targetPath))
                        {
                            Directory.CreateDirectory(targetPath);
                        }
                        targetPath = Path.Combine(targetPath, "index.rst");

                        File.WriteAllText(tempFileName, content);
                        await PandocInstance.Convert<HtmlIn, RstOut>(tempFileName, targetPath);
                        File.Delete(tempFileName);                        
                        break;
                }
            }
        }
    }
}
