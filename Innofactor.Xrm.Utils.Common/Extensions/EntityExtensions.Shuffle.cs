namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// This class contains methods related to Innofactor Shuffle
    /// </summary>
    public static partial class EntityExtensions
    {
        /// <summary>Serializes the entity</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="style"></param>
        /// <param name="parentNode">May be null to create a new XMLDocument</param>
        /// <returns></returns>
        public static XmlDocument Serialize(this Entity entity, IExecutionContainer container, SerializationStyle style, XmlNode parentNode)
        {
            XmlDocument result;
            if (parentNode != null)
            {
                result = parentNode.OwnerDocument;
            }
            else
            {
                result = new XmlDocument();
                if (style == SerializationStyle.Simple || style == SerializationStyle.SimpleWithValue || style == SerializationStyle.SimpleNoId)
                {
                    parentNode = result.CreateElement("Entities");
                    result.AppendChild(parentNode);
                }
            }
            switch (style)
            {
                case SerializationStyle.Full:
                    entity.SerializeFull(result);
                    break;

                case SerializationStyle.Simple:
                case SerializationStyle.SimpleWithValue:
                case SerializationStyle.SimpleNoId:
                    entity.SerializeSimple(container, style, parentNode, result);
                    break;

                case SerializationStyle.Explicit:
                    entity.SerializeExplicit(container, parentNode, result);
                    break;
            }
            return result;
        }
        private static void SerializeExplicitAttribute(this Entity entity, IExecutionContainer container, XmlDocument result, XmlNode xEntity, string name, object value)
        {
            var xAttribute = result.CreateNode(XmlNodeType.Element, name, "");
            var xType = result.CreateAttribute("type");
            xType.Value = Utils.LastClassName(value);
            xAttribute.Attributes.Append(xType);
            var basetypevalue = container.AttributeAsBaseType(entity, name, "", false);
            if (value is EntityReference)
            {
                var xRefEntity = result.CreateAttribute("entity");
                xRefEntity.Value = ((EntityReference)value).LogicalName;
                xAttribute.Attributes.Append(xRefEntity);
                var xRefValue = result.CreateAttribute("value");
                xRefValue.Value = ((EntityReference)value).Name;
                xAttribute.Attributes.Append(xRefValue);
            }
            if (basetypevalue != null)
            {
                var nodeText = basetypevalue is DateTime ? ((DateTime)basetypevalue).ToString("O") : basetypevalue.ToString();
                var xValue = result.CreateTextNode(nodeText);
                xAttribute.AppendChild(xValue);
            }
            xEntity.AppendChild(xAttribute);
        }

        private static void SerializeFull(this Entity entity, XmlDocument result)
        {
            var serializer = new DataContractSerializer(typeof(Entity), null, int.MaxValue, false, false, null, new KnownTypesResolver());
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);
            serializer.WriteObject(xw, entity);
            xw.Close();
            sw.Close();
            var serialized = sw.ToString();
            result.LoadXml(serialized);
        }

        private static void SerializeSimple(this Entity entity, IExecutionContainer container, SerializationStyle style, XmlNode parentNode, XmlDocument result)
        {
            XmlNode xEntity = result.CreateElement("Entity");
            var xEntityName = result.CreateAttribute("name");
            xEntityName.Value = entity.LogicalName;
            xEntity.Attributes.Append(xEntityName);
            if (style != SerializationStyle.SimpleNoId)
            {
                var xEntityId = result.CreateAttribute("id");
                xEntityId.Value = entity.Id.ToString();
                xEntity.Attributes.Append(xEntityId);
            }
            foreach (var attribute in entity.Attributes)
            {
                if (attribute.Key == container.Entity(entity.LogicalName).PrimaryIdAttribute)
                {
                    continue;
                }
                if (attribute.Key.EndsWith("_base") && entity.Contains(attribute.Key.Substring(0, attribute.Key.Length - 5)))
                {
                    continue;
                }
                entity.SerializeSimpleAttribute(container, style, result, xEntity, attribute.Key, attribute.Value);
            }
            parentNode.AppendChild(xEntity);
        }

        private static void SerializeSimpleAttribute(this Entity entity, IExecutionContainer container, SerializationStyle style, XmlDocument result, XmlNode xEntity, string name, object value)
        {
            var xAttribute = result.CreateNode(XmlNodeType.Element, "Attribute", "");
            var xName = result.CreateAttribute("name");
            xName.Value = name;
            xAttribute.Attributes.Append(xName);
            var xType = result.CreateAttribute("type");
            xType.Value = Utils.LastClassName(value);
            xAttribute.Attributes.Append(xType);
            var basetypevalue = container.AttributeAsBaseType(entity, name, "", false);
            if (value is EntityReference)
            {
                var xRefEntity = result.CreateAttribute("entity");
                xRefEntity.Value = ((EntityReference)value).LogicalName;
                xAttribute.Attributes.Append(xRefEntity);
                if (style == SerializationStyle.SimpleWithValue)
                {
                    var xRefValue = result.CreateAttribute("value");
                    xRefValue.Value = ((EntityReference)value).Name;
                    xAttribute.Attributes.Append(xRefValue);
                }
                if (style == SerializationStyle.SimpleNoId && !string.IsNullOrEmpty(((EntityReference)value).Name))
                {
                    basetypevalue = ((EntityReference)value).Name;
                }
            }
            if (basetypevalue != null)
            {
                var xValue = result.CreateTextNode(basetypevalue.ToString());
                xAttribute.AppendChild(xValue);
            }
            xEntity.AppendChild(xAttribute);
        }
        private static void SerializeExplicit(this Entity entity, IExecutionContainer container, XmlNode parentNode, XmlDocument result)
        {
            XmlNode xEntity = result.CreateElement(entity.LogicalName);
            var xEntityId = result.CreateAttribute("id");
            xEntityId.Value = entity.Id.ToString();
            xEntity.Attributes.Append(xEntityId);
            foreach (var attribute in entity.Attributes)
            {
                if (attribute.Key == container.Entity(entity.LogicalName).PrimaryIdAttribute)
                {
                    continue;
                }
                if (attribute.Key.EndsWith("_base") && entity.Contains(attribute.Key.Substring(0, attribute.Key.Length - 5)))
                {
                    continue;
                }
                entity.SerializeExplicitAttribute(container, result, xEntity, attribute.Key, attribute.Value);
            }
            parentNode.AppendChild(xEntity);
        }
    }
}