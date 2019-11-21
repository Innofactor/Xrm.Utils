namespace Innofactor.Xrm.Utils.Common.Misc
{
    using System.Xml;
    /// <summary>
    /// Utility class with XML utilities.
    /// </summary>
    public static class XML
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static XmlDocument Load(string str)
        {
            return FromString(str, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="supresserrors"></param>
        /// <returns></returns>
        public static XmlDocument FromString(string str, bool supresserrors)
        {
            if (string.IsNullOrEmpty(str) ||
                string.IsNullOrEmpty(str.Trim()) ||
                !str.StartsWith("<"))
            {
                return null;
            }

            try
            {
                var result = new XmlDocument();
                result.LoadXml(str);
                return result;
            }
            catch
            {
                if (supresserrors)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetAttribute(XmlNode node, string attribute)
        {
            var xAtt = node.Attributes[attribute];
            if (xAtt != null)
            {
                return xAtt.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="def">Default value to return</param>
        /// <returns></returns>
        public static bool GetBoolAttribute(XmlNode node, string attribute, bool def)
        {
            bool result;
            var value = GetAttribute(node, attribute);
            if (value == "1")
            {
                result = true;
            }
            else if (value == "0")
            {
                result = false;
            }
            else if (!bool.TryParse(value, out result))
            {
                result = def;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="def">Default value to return</param>
        /// <returns></returns>
        public static int GetIntAttribute(XmlNode node, string attribute, int def)
        {
            if (!int.TryParse(GetAttribute(node, attribute), out var result))
            {
                result = def;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XmlNode FindChild(XmlNode node, string name)
        {
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                if (node.ChildNodes[i].Name == name)
                {
                    return node.ChildNodes[i];
                }
            }
            return null;
        }

        /// <summary>Appends attribute to the parentNode</summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void AppendAttribute(XmlNode node, string name, string value)
        {
            var attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);
        }

        /// <summary>Add subnode of type CDATA to given entitynode</summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static XmlNode AddCDATANode(XmlNode node, string name, string value)
        {
            XmlNode textNode = node.OwnerDocument.CreateElement(name);
            node.AppendChild(textNode);
            textNode.AppendChild(node.OwnerDocument.CreateCDataSection(value));
            return textNode;
        }
    }
}