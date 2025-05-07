using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WordPressXmlToJekykll
{
    class WordPressCategory
    {
        public int? Id { get; set; }
        public string? NiceName { get; set; }
        public string? Name { get; set; }
        public string? ParentNiceName { get; set; }

        internal static List<WordPressCategory>? FromXmlNode(XmlNodeList? xmlNodeList, XmlNamespaceManager nsmgr)
        {
            if(xmlNodeList == null)
                return null;
            List<WordPressCategory> result = new List<WordPressCategory>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                WordPressCategory category = new WordPressCategory();
                category.Id = WordPressXml.GetNodeInt(xmlNode.SelectSingleNode("wp:term_id", nsmgr));
                category.NiceName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:category_nicename", nsmgr));
                category.Name = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:cat_name", nsmgr));
                category.ParentNiceName = WordPressXml.GetNodeText(xmlNode.SelectSingleNode("wp:category_parent", nsmgr));
                result.Add(category);
            }
            return result;
        }
    }
}
