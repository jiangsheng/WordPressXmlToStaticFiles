using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WordPressXmlToJekykll
{
    class WordPressItem
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public DateTime? PubDate{ get; set; }
        public string? Content{ get; set; }  
        public string? Guid { get; set; }
        public string? PostType { get; set; }
        public string? Status { get; set; }
        public int? ParentId { get; set; }
        public string? PostName { get; set; }
        public int? PostId { get; set; }

        public DateTime? PostDate { get; set; }
        public DateTime? PostDateGmt { get; set; }
        public DateTime? PostModified { get; set; }
        public DateTime? PostModifiedGmt { get; set; }

        public List<string>? PostCategories { get; set; }
        public List<string>? PostTags { get; set; }

        public Dictionary<string,string>? PostMeta{ get; set; }

        internal static List<WordPressItem>? FromXmlNode(XmlNodeList? xmlNodeList, XmlNamespaceManager nsmgr)
        {
            if (xmlNodeList == null)
                return null;
            List<WordPressItem> result = new List<WordPressItem>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                WordPressItem wordPressItem = new WordPressItem();
                wordPressItem.Title = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("title"));
                wordPressItem.Link = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("link"));
                wordPressItem.PubDate = WordPressXml.GetNodeDateTime(xmlNode.SelectSingleNode("pubDate"));
                wordPressItem.Content = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("content:encoded", nsmgr));
                wordPressItem.Guid = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("guid"));
                wordPressItem.PostType = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:post_type", nsmgr));
                wordPressItem.Status = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:status", nsmgr));
                wordPressItem.ParentId = WordPressXml.GetNodeInt(xmlNode.SelectSingleNode("wp:post_parent", nsmgr));
                wordPressItem.PostName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:post_name", nsmgr));
                wordPressItem.PostId = WordPressXml.GetNodeInt(xmlNode.SelectSingleNode("wp:post_id", nsmgr));
                wordPressItem.PostDate = WordPressXml.GetNodeDateTime(xmlNode.SelectSingleNode("wp:post_date", nsmgr));
                wordPressItem.PostDateGmt = WordPressXml.GetNodeDateTime(xmlNode.SelectSingleNode("wp:post_date_gmt", nsmgr));
                wordPressItem.PostModified = WordPressXml.GetNodeDateTime(xmlNode.SelectSingleNode("wp:post_modified", nsmgr));
                wordPressItem.PostModifiedGmt = WordPressXml.GetNodeDateTime(xmlNode.SelectSingleNode("wp:post_modified_gmt", nsmgr));
                wordPressItem.PostCategories = GetPostCategories(xmlNode.SelectNodes("category", nsmgr));
                wordPressItem.PostTags = GetPostTags(xmlNode.SelectNodes("category", nsmgr));
                wordPressItem.PostMeta= GetPostMeta(xmlNode.SelectNodes("postmeta", nsmgr));
                result.Add(wordPressItem);

            }
            return result;
        }

        private static Dictionary<string, string>? GetPostMeta(XmlNodeList? xmlNodeList)
        {
            if (xmlNodeList == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.Attributes == null)
                    continue;
                string? metaKey = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("meta_key"));
                string? metaValue = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("meta_value"));
                if (metaKey != null && metaValue != null)
                {
                    result.Add(metaKey, metaValue);
                }
            }
            return result;
        }

        private static List<string>? GetPostTags(XmlNodeList? xmlNodeList)
        {
            if (xmlNodeList == null)
                return null;
            List<string> result = new List<string>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if(xmlNode.Attributes == null)
                    continue;
                string? domain = xmlNode.Attributes["domain"]?.Value;
                if (domain == null || domain != "post_tag")
                    continue;
                string? category = WordPressXml.GetNodeText(xmlNode);
                if (category != null)
                {
                    result.Add(category);
                }
            }
            return result;
        }

        private static List<string>? GetPostCategories(XmlNodeList? xmlNodeList)
        {
            if (xmlNodeList == null)
                return null;
            List<string> result = new List<string>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.Attributes == null)
                    continue;
                string? domain = xmlNode.Attributes["domain"]?.Value;
                if (domain == null || domain != "category")
                    continue;
                string? category = WordPressXml.GetNodeText(xmlNode);
                if (category != null)
                {
                    result.Add(category);
                }
            }
            return result;
        }
    }
}
