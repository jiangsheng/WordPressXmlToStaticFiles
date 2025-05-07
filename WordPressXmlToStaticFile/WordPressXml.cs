using Castle.Components.DictionaryAdapter.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WordPressXmlToStaticFile
{
    class WordPressXml
    {
        public List<WordPressItem>? Items { get; set; }

        public WordPressItem? Index { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? Language { get; set; }
        public string? BaseSiteUrl { get; set; }
        public string? BaseBlogUrl { get; set; }

        public WordPressAuthor? Author { get; set; }

        public List<WordPressCategory>? Categories { get; private set; }
        public List<WordPressTag>? Tags { get; private set; }

        public static WordPressXml FromFile(string inputFile)
        {
            WordPressXml result = new WordPressXml();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(inputFile);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("wp", "http://wordpress.org/export/1.2/");
            nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
            nsmgr.AddNamespace("excerpt", "http://wordpress.org/export/1.2/excerpt/");
            nsmgr.AddNamespace("wfw", "http://wellformedweb.org/CommentAPI/");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            var channel = xmlDocument.SelectSingleNode("/rss/channel");
            result.Title = GetNodeText(channel.SelectSingleNode("title"));
            result.Description = GetNodeText(channel.SelectSingleNode("description"));
            result.Link = GetNodeText(channel.SelectSingleNode("link"));
            result.Language = GetNodeText(channel.SelectSingleNode("language"));
            result.BaseSiteUrl = GetNodeText(channel.SelectSingleNode("wp:base_site_url", nsmgr));
            result.BaseBlogUrl = GetNodeText(channel.SelectSingleNode("wp:base_blog_url", nsmgr));

            if (channel != null)
            {
                result.Author = WordPressAuthor.FromXmlNode(
                    channel.SelectSingleNode("wp:author", nsmgr), nsmgr);
                result.Categories=WordPressCategory.FromXmlNode(
                    channel.SelectNodes("wp:category", nsmgr), nsmgr);
                result.Tags=WordPressTag.FromXmlNode(
                    channel.SelectNodes("wp:tag", nsmgr), nsmgr);
                result.Items = WordPressItem.FromXmlNode(
                    channel.SelectNodes("item", nsmgr), nsmgr);
            }
            return result;
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
    }
}
