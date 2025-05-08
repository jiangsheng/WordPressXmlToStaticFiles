using Pandoc;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
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
        public static string? GetNodeText(XmlNode? xmlNode)
        {
            if (xmlNode != null)
            {
                foreach (XmlNode child in xmlNode.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Text)
                    {
                        return child.Value;
                    }
                    if (child.NodeType == XmlNodeType.CDATA)
                    {
                        return ((XmlCDataSection)child).Data;
                    }
                }
            }
            return null;
        }
        public static DateTime? GetNodeDateTime(XmlNode? xmlNode)
        {
            if (xmlNode == null) return null;
            var nodeText = GetNodeText(xmlNode);
            if (nodeText != null)
            {
                DateTime dateTime;
                if (DateTime.TryParse(nodeText, out dateTime))
                {
                    return dateTime;
                }
            }
            return null;
        }
        public static int? GetNodeInt(XmlNode? xmlNode)
        {
            if (xmlNode == null) return null;
            var nodeText = GetNodeText(xmlNode);
            if (nodeText != null)
            {
                int test;
                if (int.TryParse(nodeText, out test))
                {
                    return test;
                }
            }
            return null;
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
            if (!Directory.Exists(Settings.PanDocPath))
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
                Dictionary<string, string> urlReplacement = new Dictionary<string, string>();
                Dictionary<string, string> downloadQueue= new Dictionary<string, string>();
                var attachments = wordPressXml.Items.Where(p => p.PostType == "attachment");
                foreach (var attachment in attachments)
                {
                    ParseAttachment(attachment, wordPressXml, Settings, urlReplacement, downloadQueue);
                }
                Parallel.ForEach(downloadQueue, keyValuePair =>
                {
                    DownloadAttachment(keyValuePair.Key,keyValuePair.Value);
                });

                var posts= wordPressXml.Items.Where(p=>p.PostType=="post");
                foreach (var post in posts)
                {
                    await WritePost(post, wordPressXml, Settings, PandocEngine);
                }
            }
            else
            {
                Console.WriteLine("No items found in the XML file.");
            }
            if (wordPressXml.Tags != null)
            {
                foreach (var tag in wordPressXml.Tags)
                {
                    WriteTagRedirect(tag);
                }
            }
        }

        private void WriteTagRedirect(WordPressTag tag)
        {
            if (Settings.CreateRedirectForABlog)
            {
                var tagRedirectOutputFolder = Settings.OutputFolder;
                var tagBuildFolder = Settings.OutputFolder;
                if (!string.IsNullOrEmpty(Settings.SphinixBuildFolder))
                {
                    tagBuildFolder = Path.Combine(tagBuildFolder, Settings.SphinixBuildFolder);
                }
                tagBuildFolder = Path.Combine(tagBuildFolder, "blogs\\tag");
                var targetTagHtmlFileName = Path.Combine(tagBuildFolder, tag.Slug+".html");
                var sourceRedirectFileName = Path.Combine(tagRedirectOutputFolder, "tag");
                sourceRedirectFileName = Path.Combine(sourceRedirectFileName, tag.Slug);
                if(!Directory.Exists(sourceRedirectFileName))
                    Directory.CreateDirectory(sourceRedirectFileName);
                sourceRedirectFileName = Path.Combine(sourceRedirectFileName,"index.html");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!doctype html>\r\n<html>\r\n<head>");
                var redirectPath = Path.GetRelativePath(sourceRedirectFileName, targetTagHtmlFileName);
                sb.AppendLine("<meta http-equiv=\"refresh\" content=\"0; url=" + redirectPath.Replace('\\', '/') + "\">");
                sb.AppendLine("</head>\r\n<body>\t\r\n</body>\r\n</html>");
                var encodedText = Encoding.UTF8.GetBytes(sb.ToString());
                File.WriteAllBytes(sourceRedirectFileName, encodedText);
            }
        }

        private void DownloadAttachment(string fromUrl, string toFile)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(fromUrl, toFile);
            }
        }
        private void ParseAttachment(WordPressItem attachment, WordPressXml wordPressXml, IMySettings settings, 
            Dictionary<string, string> urlReplacement, Dictionary<string, string> downloadQueue)
        {
            Uri attachmentUri=new  Uri(attachment.Link);
            Uri baseBlogUri= new Uri(wordPressXml.BaseBlogUrl);

            var relativeAttachmentUri = attachmentUri.MakeRelativeUri(baseBlogUri);
            var mediaFolder=Path.Combine(settings.OutputFolder, "docs\\blogs\\images");
            if (!Directory.Exists(mediaFolder))
            {
                Directory.CreateDirectory(mediaFolder);
            }
        }

        private async Task  WritePost(WordPressItem post,
            WordPressXml wordPressXml, IMySettings settings, PandocEngine pandocEngine)
        {
            var slug = post.Link;
            if (slug.EndsWith("/"))
            {
                slug = slug.Substring(0, slug.Length - 1);
            }
            var postLinkUri = new Uri(slug);
            slug = Uri.UnescapeDataString(Path.GetFileName(postLinkUri.AbsolutePath));
            Debug.Assert(!string.IsNullOrWhiteSpace(slug));
            Debug.WriteLine(string.Format("Processing post: {0}", post.Title));
            if (post.PubDate.HasValue)
                Debug.WriteLine(string.Format("published on : {0}", post.PubDate));
            Debug.WriteLine(string.Format("post content: {0}", post.Content));
            Debug.WriteLine(string.Format("slug: {0}", slug));
            string resultText = string.Empty;
            var targetPath = Settings.OutputFolder;
            var targetRedirectPath = Settings.OutputFolder;
            string? sphinixSourceFolder;
            string sphinixBuildFolder;
            if (!string.IsNullOrEmpty(Settings.SphinixBuildFolder))
                sphinixBuildFolder = Path.Combine(targetPath, Settings.SphinixBuildFolder);
            else
                sphinixBuildFolder = targetPath;

            if (!string.IsNullOrEmpty(Settings.SphinixSourceFolder))
                sphinixSourceFolder = targetPath = Path.Combine(targetPath, Settings.SphinixSourceFolder);
            else
                sphinixSourceFolder = targetPath;


            if (settings.CreateRedirectForABlog)
            {
                targetPath = Path.Combine(targetPath, "blogs");
            }
            if (post.PubDate.HasValue)
            {
                if (Settings.CreateYearFolders)
                {
                    targetPath = Path.Combine(targetPath, post.PubDate.Value.Year.ToString());
                }
                if (Settings.CreateMonthFolders)
                {
                    targetPath = Path.Combine(targetPath, post.PubDate.Value.Month.ToString());
                }
                if (Settings.CreateDayFolders)
                {
                    targetPath = Path.Combine(targetPath, post.PubDate.Value.Day.ToString());
                }
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            byte[]? encodedText = null;
            string? targetFileName = null;
            var tempFileName = Path.GetTempFileName();
            switch (this.Settings.OutputFormat)
            {
                case 1:
                    targetFileName = GetUniqueFileName(targetPath, slug,".html");
                    resultText = string.Format("<!doctype html><html><head><meta charset=\"UTF-8\"<title>{0}</head><body>{1}</body></html>",
                        post.Title, post.Content);
                    encodedText = Encoding.UTF8.GetBytes(resultText);
                    File.WriteAllBytes(targetPath, encodedText);
                    break;
                case 2:
                    targetFileName = GetUniqueFileName(targetPath, slug, ".md");
                    File.WriteAllText(tempFileName, post.Content);
                    await PandocInstance.Convert<HtmlIn, GhMdOut>(tempFileName, targetFileName);
                    File.Delete(tempFileName);
                    break;
                case 3:
                    targetFileName = GetUniqueFileName(targetPath, slug, ".rst");
                    if (settings.CreateRedirectForABlog)
                    {
                        var redirectFileName = Path.Combine(
                            settings.OutputFolder
                            , postLinkUri.LocalPath.Replace('/', '\\').Substring(1) + ".html");
                        var sourceRelativePath = Path.GetRelativePath(sphinixSourceFolder, targetFileName);

                        var outputFileName = Path.Combine(
                            sphinixBuildFolder, sourceRelativePath);
                        outputFileName = Path.ChangeExtension(outputFileName, ".html");
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("<!doctype html>\r\n<html>\r\n<head>");
                        var redirectPath = Path.GetRelativePath(redirectFileName, outputFileName);
                        sb.AppendLine("<meta http-equiv=\"refresh\" content=\"0; url=" + redirectPath.Replace('\\','/') + "\">");
                        sb.AppendLine("</head>\r\n<body>\t\r\n</body>\r\n</html>");
                        encodedText = Encoding.UTF8.GetBytes(sb.ToString());
                        var redirectDirectory = Path.GetDirectoryName(redirectFileName);
                        if (!Directory.Exists(redirectDirectory))
                            Directory.CreateDirectory(redirectDirectory);
                        File.WriteAllBytes(redirectFileName, encodedText);
                    }
                    var targetFileNameDirectory = Path.GetDirectoryName(targetFileName);
                    if (!Directory.Exists(targetFileNameDirectory))
                        Directory.CreateDirectory(targetFileNameDirectory);
                    File.WriteAllText(tempFileName, post.Content);
                    await PandocInstance.Convert<HtmlIn, RstOut>(tempFileName, targetFileName);
                    File.Delete(tempFileName);
                    var convertedContent=File.ReadAllText(targetFileName);

                    var postStringBuilder = new StringBuilder();
                    postStringBuilder.AppendLine(post.Title);
                    Debug.Assert(post.Content!=null && post.Content.Length > 0);
                    Debug.Assert(post.Title != null && post.Title.Length>0);
                    postStringBuilder.AppendLine(new string('=',post.Title.Length*2));
                    if (post.PubDate != null)
                    {
                        postStringBuilder.AppendLine(string.Format(".. post:: {0}, {1}, {2}", post.PubDate.Value.Day
                            , post.PubDate.Value.ToString("MMM"), post.PubDate.Value.Year));
                        if (post.PostTags != null && post.PostTags.Count > 0)
                        {

                            postStringBuilder.AppendLine(string.Format("   :tags: {0}", string.Join(',', post.PostTags)));
                        }
                        if (post.PostCategories != null && post.PostCategories.Count > 0)
                        {
                            postStringBuilder.AppendLine(string.Format("   :category: {0}", string.Join(',', post.PostCategories)));
                        }
                        if (post.Creator != null)
                        {
                            postStringBuilder.AppendLine("   :author: "+ post.Creator);
                        }
                        postStringBuilder.AppendLine("   :nocomments:");
                    }
                    postStringBuilder.AppendLine();
                    postStringBuilder.Append(convertedContent);
                    encodedText = Encoding.UTF8.GetBytes(postStringBuilder.ToString());
                    File.WriteAllBytes(targetFileName, encodedText);

                    break;
            }

        }

        private string GetUniqueFileName(string targetPath, string slug, string extension)
        {
            int revision = 0;
            string? testFileName;
            do
            {
                if (revision == 0)
                {
                    testFileName = Path.Combine(targetPath, slug);
                    testFileName = Path.Combine(testFileName , "index"+extension);
                }
                else
                {
                    testFileName = Path.Combine(targetPath, string.Format("{0}_{1}", slug, revision));
                    testFileName = Path.Combine(testFileName, "index" + extension);
                }
            }
            while (File.Exists(testFileName));
            return testFileName;
        }
    }
}
