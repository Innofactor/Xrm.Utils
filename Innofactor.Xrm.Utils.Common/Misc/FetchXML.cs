using System.Xml;
using Innofactor.Xrm.Utils.Common.Extensions;
using Innofactor.Xrm.Utils.Common.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Innofactor.Xrm.Utils.Common.Misc
{
    /// <summary>Specifies how rows of data are sorted.</summary>
    public enum SortOrder
    {
        /// <summary>The default. No sort order is specified.</summary>
        Unspecified = -1,
        /// <summary>Rows are sorted in ascending order.</summary>
        Ascending = 0,
        /// <summary>Rows are sorted in descending order.</summary>
        Descending = 1,
    }
    /// <summary>
    ///
    /// </summary>
    public static class FetchXML
    {
        /// <summary>Creates the root parentNode in a FetchXML document</summary>
        /// <param name="xml">Target XmlDocument</param>
        /// <param name="entity">Main entity for the FetchXML</param>
        /// <returns>The entity parentNode in the XML document</returns>
        public static XmlNode Create(XmlDocument xml, string entity)
        {
            return Create(xml, entity, false, false);
        }

        /// <summary>Creates the root parentNode in a FetchXML document</summary>
        /// <param name="xml">Target XmlDocument</param>
        /// <param name="entity">Main entity for the FetchXML</param>
        /// <param name="distinct"></param>
        /// <returns>The entity parentNode in the XML document</returns>
        public static XmlNode Create(XmlDocument xml, string entity, bool distinct)
        {
            return Create(xml, entity, distinct, false);
        }

        /// <summary>Creates the root parentNode in a FetchXML document</summary>
        /// <param name="xml">Target XmlDocument</param>
        /// <param name="entity">Main entity for the FetchXML</param>
        /// <param name="distinct"></param>
        /// <param name="nolock"></param>
        /// <returns>The entity parentNode in the XML document</returns>
        public static XmlNode Create(XmlDocument xml, string entity, bool distinct, bool nolock)
        {
            if (xml != null)
            {
                var fetch = xml.CreateNode(XmlNodeType.Element, "fetch", "");
                AppendAttribute(fetch, "mapping", "logical");
                if (distinct)
                {
                    AppendAttribute(fetch, "distinct", "true");
                }

                if (nolock)
                {
                    AppendAttribute(fetch, "no-lock", "true");
                }

                xml.AppendChild(fetch);
                var ent = xml.CreateNode(XmlNodeType.Element, "entity", "");
                AppendAttribute(ent, "name", entity);
                fetch.AppendChild(ent);
                return ent;
            }
            return null;
        }

        /// <summary>Add parentNode to retrieve all attributes of the entity in the parentNode</summary>
        /// <param name="node">Node of an Entity or a LinkedEntity</param>
        /// <returns>The AllAttributes parentNode</returns>
        public static XmlNode AddAllAttributes(XmlNode node)
        {
            if (node != null)
            {
                var attr = node.OwnerDocument.CreateNode(XmlNodeType.Element, "all-attributes", "");
                node.AppendChild(attr);
                return attr;
            }
            return null;
        }

        /// <summary>Add named attribute of the parentNode to the resultset</summary>
        /// <param name="entitynode"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static void AddAttribute(XmlNode entitynode, params string[] attributes)
        {
            if (attributes != null && entitynode != null)
            {
                foreach (var attrname in attributes)
                {
                    var attr = entitynode.OwnerDocument.CreateNode(XmlNodeType.Element, "attribute", "");
                    AppendAttribute(attr, "name", attrname);
                    entitynode.AppendChild(attr);
                }
            }
        }

        /// <summary>Add sort-order to an entity entitynode</summary>
        /// <param name="entitynode"></param>
        /// <param name="attribute"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static XmlNode OrderBy(XmlNode entitynode, string attribute, SortOrder order)
        {
            if (entitynode != null)
            {
                var ord = entitynode.OwnerDocument.CreateNode(XmlNodeType.Element, "order", "");
                AppendAttribute(ord, "attribute", attribute);
                if (order == SortOrder.Descending)
                {
                    AppendAttribute(ord, "descending", "true");
                }
                entitynode.AppendChild(ord);
                return ord;
            }
            return null;
        }

        /// <summary>Add subnode of type Text to given entitynode</summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static XmlNode AddTextNode(XmlNode node, string name, string value)
        {
            if (node != null)
            {
                var textNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, name, "");
                node.AppendChild(textNode);
                var valueNode = node.OwnerDocument.CreateNode(XmlNodeType.Text, "", "");
                valueNode.Value = value;
                textNode.AppendChild(valueNode);
                return textNode;
            }
            return null;
        }

        /// <summary>Add subnode of type CDATA to given entitynode</summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static XmlNode AddCDATANode(XmlNode node, string name, string value)
        {
            if (node != null)
            {
                var textNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, name, "");
                node.AppendChild(textNode);
                var valueNode = node.OwnerDocument.CreateNode(XmlNodeType.CDATA, "", "");
                valueNode.Value = value;
                textNode.AppendChild(valueNode);
                return textNode;
            }
            return null;
        }

        /// <summary>Use to generate link between 1:N entities (for standard identifying field names)</summary>
        /// <param name="node"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static XmlNode AppendLink1N(XmlNode node, string to)
        {
            return AppendLink1N(node, to, to + "_id", to + "id", "");
        }

        /// <summary>Use to generate link between 1:N entities (for standard identifying field names)</summary>
        /// <param name="node"></param>
        /// <param name="to"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static XmlNode AppendLink1N(XmlNode node, string to, string alias)
        {
            return AppendLink1N(node, to, to + "_id", to + "id", alias);
        }

        /// <summary>Use to generate link between 1:N entities (for non-standard identifying field names)</summary>
        /// <param name="node"></param>
        /// <param name="to"></param>
        /// <param name="from_fk"></param>
        /// <param name="to_pk"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static XmlNode AppendLink1N(XmlNode node, string to, string from_fk, string to_pk, string alias)
        {
            return AppendLink(node, to, from_fk, to_pk, "inner", alias);
        }

        /// <summary>Use to generate link between N:N entities (for standard identifying field names and intersect table name)</summary>
        /// <param name="node"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static XmlNode AppendLinkNN(XmlNode node, string from, string to)
        {
            return AppendLinkNN(node, to + "_" + from, from + "id", to, to + "id");
        }

        /// <summary>Use to generate link between N:N entities (for non-standard identifying field names and intersect table name)</summary>
        /// <param name="node"></param>
        /// <param name="intersect"></param>
        /// <param name="from_pk"></param>
        /// <param name="to"></param>
        /// <param name="to_pk"></param>
        /// <returns></returns>
        public static XmlNode AppendLinkNN(XmlNode node, string intersect, string from_pk, string to, string to_pk)
        {
            var NN = AppendLink(node, intersect, from_pk, from_pk, "inner", "");
            return AppendLink(NN, to, to_pk, to_pk, "inner", "");
        }

        /// <summary>Appends FetchXML linked entity to the parentNode</summary>
        /// <param name="node"></param>
        /// <param name="entity"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="type"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static XmlNode AppendLink(XmlNode node, string entity, string to, string from, string type, string alias)
        {
            var xlink = node.OwnerDocument.CreateNode(XmlNodeType.Element, "link-entity", "");
            AppendAttribute(xlink, "name", entity);
            AppendAttribute(xlink, "to", to);
            AppendAttribute(xlink, "from", from);
            AppendAttribute(xlink, "link-type", type);
            if (!string.IsNullOrEmpty(alias))
            {
                AppendAttribute(xlink, "alias", alias);
            }
            node.AppendChild(xlink);
            return xlink;
        }

        /// <summary>Appends FetchXML filter to the parentNode</summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <param name="attr"></param>
        /// <param name="oper"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static XmlNode AppendFilter(XmlNode node, string type, string attr, string oper, params string[] val)
        {
            var xfilter = AppendFilter(node, type);
            AppendCondition(xfilter, attr, oper, val);
            return xfilter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static XmlNode AppendFilter(XmlNode node, string type)
        {
            var xfilter = node.OwnerDocument.CreateNode(XmlNodeType.Element, "filter", "");
            var xtype = node.OwnerDocument.CreateAttribute("type");
            xtype.Value = type;
            xfilter.Attributes.Append(xtype);
            node.AppendChild(xfilter);
            return xfilter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="attr"></param>
        /// <param name="oper"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static XmlNode AppendCondition(XmlNode filter, string attr, string oper, params string[] val)
        {
            var xcond = filter.OwnerDocument.CreateNode(XmlNodeType.Element, "condition", "");
            AppendAttribute(xcond, "attribute", attr);
            if (val != null && val.Length == 1 && oper == "in")
            {
                oper = "eq";   // "in" is not allowed with only one value (??)
            }

            AppendAttribute(xcond, "operator", oper);

            if (val != null)
            {
                if (val.Length > 1)
                {
                    foreach (var value in val)
                    {
                        var xval = filter.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
                        xval.InnerXml = value;
                        xcond.AppendChild(xval);
                    }
                }
                else if (val.Length == 1)
                {
                    AppendAttribute(xcond, "value", val[0]);
                }
            }
            filter.AppendChild(xcond);
            return xcond;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static XmlNode AppendFilterActive(XmlNode node)
        {
            return AppendFilter(node, "and", "statecode", "eq", "0");
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

        /// <summary>XML helper function to find a specific childnode</summary>
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
        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ConvertToFetchXml(this IExecutionContainer container, QueryExpression query)
        {
            var request = new QueryExpressionToFetchXmlRequest() { Query = query };
            var response = (QueryExpressionToFetchXmlResponse)container.Execute(request);
            return response.FetchXml;
        }
    }
}