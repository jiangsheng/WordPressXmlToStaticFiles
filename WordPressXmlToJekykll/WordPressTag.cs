using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WordPressXmlToJekykll
{
    class WordPressTag
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }

        internal static List<WordPressTag>? FromXmlNode(XmlNodeList? xmlNodeList, XmlNamespaceManager nsmgr)
        {
            if(xmlNodeList == null)
                return null;
            List<WordPressTag> result = new List<WordPressTag>();
            foreach(XmlNode node in xmlNodeList) {
                WordPressTag tag = new WordPressTag();
                tag.Id = WordPressXml.GetNodeInt(node.SelectSingleNode("wp:term_id", nsmgr));
                tag.Slug = WordPressXml.GetNodeText(node.SelectSingleNode("wp:tag_slug", nsmgr));
                tag.Name = WordPressXml.GetNodeText(node.SelectSingleNode("wp:tag_name", nsmgr));
                result.Add(tag);
            }
            return result;
        }
}
