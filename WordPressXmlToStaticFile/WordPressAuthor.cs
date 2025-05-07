using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WordPressXmlToStaticFile
{
    class WordPressAuthor
    {
        public int? AuthorId { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        internal static WordPressAuthor? FromXmlNode(XmlNode xmlNode, XmlNamespaceManager nsmgr)
        {
            WordPressAuthor result=new WordPressAuthor();
            result.AuthorId = WordPressXml.GetNodeInt(xmlNode.SelectSingleNode("wp:author_id", nsmgr));
            result.Email = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:author_email", nsmgr));
            result.DisplayName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:author_display_name", nsmgr));
            result.FirstName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:author_first_name", nsmgr));
            result.LastName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:author_last_name", nsmgr));
            return result;
        }
    }
}
