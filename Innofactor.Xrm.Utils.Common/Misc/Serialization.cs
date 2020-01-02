namespace Innofactor.Xrm.Utils.Common.Misc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using ElencySolutions.CsvHelper;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;

    /// <summary>
    /// Serialization methods for bare SDK classes
    /// </summary>
    public static class Serialization
    {
        /// <summary>Serves as a constructor for EntityCollection class, initializing collection with text file entities</summary>
        /// <param name="text"></param>
        /// <param name="container"></param>
        /// <param name="delimeter"></param>
        public static IEnumerable<Entity> DeserializeEntities(this string text, IExecutionContainer container, char delimeter)
        {
            var log = container.Logger;
            container.StartSection("string.ToEntityList");
            var result = new List<Entity>();
            container.Log("Text {0} characters, Delimeter: {1}", text.Length, delimeter);
            if (text.Length > 0)
            {
                List<string> columns;
                List<string> types = null;
                var reader = new CsvReader(Encoding.UTF8, text) { Delimeter = delimeter, RemoveEmptyTrailingFields = true };
                if (!reader.ReadNextRecord())
                {
                    throw new InvalidDataContractException("CSV file does not contain a column names header row.");
                }
                columns = reader.Fields;
                while (reader.ReadNextRecord())
                {
                    var values = reader.Fields;
                    if (values.Count >= 1)
                    {   // Två kolumner måste finnas för att ha något alls - Entity och Id (även om Id ju faktiskt kan vara tomt)
                        if (types == null)
                        {   // Typer ej ännu definierade
                            if (values[0] == "String" && values[1] == "Guid")
                            {   // Typer definierades på raden
                                container.Log("Assigning types from serialized text");
                                types = values;
                                continue;
                            }
                            else
                            {   // Leta typer från metadata
                                container.StartSection("Retrieving types from metadata");
                                var entity = values[0];
                                types = new List<string>(columns.Count);
                                // Idiotisk kod bara för att jag inte vet hur man hanterar listor vettigt, eller nåt //JR
                                for (var i = 0; i < columns.Count; i++) { types.Add(""); }

                                types[0] = "String";    // Entity name type
                                types[1] = "Guid";      // Record id type
                                container.Log("Retrieving metadata for {0}", entity);

                                var metadata = (EntityMetadata)container.Execute(
                                    new RetrieveEntityRequest()
                                    {
                                        LogicalName = entity,
                                        EntityFilters = EntityFilters.Attributes,
                                        RetrieveAsIfPublished = true
                                    });
                                foreach (var attrmeta in metadata.Attributes)
                                {
                                    if (columns.Contains(attrmeta.LogicalName))
                                    {
                                        container.Log("Column: {0} is {1}", attrmeta.LogicalName, attrmeta.AttributeType);
                                        types[columns.IndexOf(attrmeta.LogicalName)] = attrmeta.AttributeType.ToString();
                                    }
                                }
                                for (var i = 0; i < columns.Count; i++)
                                {
                                    if (string.IsNullOrWhiteSpace(types[i]))
                                    {
                                        throw new ArgumentNullException("Type", "Cannot find type for column " + columns[i]);
                                    }
                                }
                                container.EndSection();
                            }
                        }
                        result.Add(EntityFromTextLine(container, columns, types, values));
                    }
                }
            }
            container.EndSection();
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializedEntities"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<Entity> DeserializeEntities(this XmlDocument serializedEntities, IExecutionContainer container)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            var result = new List<Entity>();
            if (serializedEntities != null && serializedEntities.ChildNodes.Count > 0)
            {
                if (serializedEntities.ChildNodes[0].Name == "Entities")
                {
                    foreach (XmlNode xEntity in serializedEntities.ChildNodes[0].ChildNodes)
                    {
                        result.Add(EntityFromXml(xEntity, container));
                    }
                }
                else
                {
                    var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                    var sr = new StringReader(serializedEntities.OuterXml);
                    using (var reader = new XmlTextReader(sr))
                    {
                        result = (List<Entity>)serializer.ReadObject(reader);
                    }
                    sr.Close();
                }
            }
            container.EndSection();
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="container"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static XmlDocument Serialize(this IEnumerable<Entity> entities, IExecutionContainer container, SerializationStyle style)
        {
            var pkattribute = entities.Select(e => container.Entity(e.LogicalName).PrimaryIdAttribute).FirstOrDefault();
            return entities.Serialize(container, pkattribute, style);
        }

        /// <summary>Serialize collection, using specified formatting</summary>
        /// <param name="entities"></param>
        /// <param name="container"></param>
        /// <param name="primarykeyattribute"></param>
        /// <param name="style">Requested serialization style</param>
        /// <returns></returns>
        public static XmlDocument Serialize(this IEnumerable<Entity> entities, IExecutionContainer container, string primarykeyattribute, SerializationStyle style)
        {
            var result = new XmlDocument();
            switch (style)
            {
                case SerializationStyle.Full:
                    var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                    var sw = new StringWriter();
                    var xw = new XmlTextWriter(sw);
                    serializer.WriteObject(xw, entities);
                    xw.Close();
                    sw.Close();
                    var serialized = sw.ToString();
                    result.LoadXml(serialized);
                    break;

                case SerializationStyle.Simple:
                case SerializationStyle.SimpleWithValue:
                case SerializationStyle.SimpleNoId:
                case SerializationStyle.Explicit:
                    var root = result.CreateNode(XmlNodeType.Element, "Entities", "");
                    foreach (var entity in entities)
                    {
                        entity.Serialize(container, primarykeyattribute, style, root);
                    }
                    result.AppendChild(root);
                    break;
            }
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entityCollection"></param>
        /// <param name="container"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        public static string ToTextFile(this EntityCollection entityCollection, IExecutionContainer container, char delimeter)
        {
            for(var i=0;i<= entityCollection.Count();i++)
            {
                var pkattribute = container.Entity(entityCollection[i].LogicalName).PrimaryIdAttribute;
                if (!string.IsNullOrWhiteSpace(pkattribute))
                {
                    return entityCollection.ToTextFile(container, pkattribute, delimeter);
                }
            }
            return null;
        }

        /// <summary>Returns the EntityCollection in a text file format with given separator.
        /// First line defines column names.
        /// Second line defines column types.
        /// First column is always entity name.
        /// Secon column is always record id.</summary>
        /// <param name="entityCollection"></param>
        /// <param name="container"></param>
        /// <param name="primarykeyattribute"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        public static string ToTextFile(this EntityCollection entityCollection, IExecutionContainer container, string primarykeyattribute, char delimeter)
        {
            var csv = new CsvFile();
            var columns = new CsvRecord();
            columns.Fields.Add("Entity");
            columns.Fields.Add("Id");
            var types = new CsvRecord();
            types.Fields.Add("String");
            types.Fields.Add("Guid");

            var entityname = "";
            foreach (var entity in entityCollection.Entities)
            {
                if (string.IsNullOrWhiteSpace(entityname))
                {
                    entityname = entity.LogicalName;
                }
                else if (entityname != entity.LogicalName)
                {
                    throw new InvalidPluginExecutionException($"Cannot compose text file when collection contains entities of different types. { entityname } <> {entity.LogicalName}");
                }
                foreach (var attribute in entity.Attributes)
                {
                    if (attribute.Key == primarykeyattribute)
                    {
                        continue;
                    }
                    if (attribute.Key.EndsWith("_base", StringComparison.OrdinalIgnoreCase) && entity.Contains(attribute.Key.Substring(0, attribute.Key.Length - 5)))
                    {
                        continue;
                    }
                    if (!columns.Fields.Contains(attribute.Key))
                    {
                        columns.Fields.Add(attribute.Key);
                        types.Fields.Add(Utils.LastClassName(attribute.Value));
                    }
                    if (types.Fields[columns.Fields.IndexOf(attribute.Key)] == "null" && attribute.Value != null)
                    {
                        types.Fields[columns.Fields.IndexOf(attribute.Key)] = Utils.LastClassName(attribute.Value);
                    }
                }
            }

            csv.Records.Add(columns);
            csv.Records.Add(types);
            foreach (var entity in entityCollection.Entities)
            {
                var csventity = new CsvRecord();
                csventity.Fields.AddRange(EntityToTextLine(container, entity, columns.Fields));
                csv.Records.Add(csventity);
            }
            var writer = new CsvWriter() { Delimeter = delimeter };
            var result = writer.WriteCsv(csv, Encoding.UTF8);
            return result;
        }

        private static Entity EntityFromTextLine(IExecutionContainer container, List<string> columns, List<string> types, List<string> values)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            try
            {
                container.Log($"Columns: {columns.Count} Types: {types.Count} Fields: {values.Count}");
                var name = values[0];
                var strId = values[1];
                container.Log($"Entity: {name} Id: {strId}");
                var id = container.StringToGuidish(strId);
                Entity entity;
                if (!id.Equals(Guid.Empty))
                {
                    entity = new Entity(name, id);
                }
                else
                {
                    entity = new Entity(name);
                }
                for (var col = 2; col < columns.Count; col++)
                {
                    var attribute = columns[col];
                    var type = types[col];
                    var value = values.Count > col ? values[col] : "";
                    if (value == "<null>")
                    {
                        type = "null";
                    }
                    entity.SetAttribute(container, attribute, type, value);
                }
                container.Log("Initiated {0} \"{1}\" with {2} attributes", entity.LogicalName, entity, entity.Attributes.Count);
                return entity;
            }
            finally
            {
                container.EndSection();
            }
        }

        private static Entity EntityFromXml(XmlNode xEntity, IExecutionContainer container)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            Entity result;
            var name = xEntity.Name == "Entity" ? XML.GetAttribute(xEntity, "name") : xEntity.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new XmlException("Cannot deserialize entity, missing entity name");
            }
            var strId = XML.GetAttribute(xEntity, "id");
            var id = container.StringToGuidish(strId);
            if (!id.Equals(Guid.Empty))
            {
                result = new Entity(name, id);
            }
            else
            {
                result = new Entity(name);
            }
            foreach (XmlNode xAttribute in xEntity.ChildNodes)
            {
                if (xAttribute.NodeType == XmlNodeType.Element)
                {
                    var attribute = xAttribute.Name == "Attribute" ? XML.GetAttribute(xAttribute, "name") : xAttribute.Name;
                    var type = XML.GetAttribute(xAttribute, "type");
                    var value = xAttribute.ChildNodes.Count > 0 ? xAttribute.ChildNodes[0].InnerText : "";
                    if (type == "EntityReference")
                    {
                        var entity = XML.GetAttribute(xAttribute, "entity");
                        value = entity + ":" + value;
                        var entrefname = XML.GetAttribute(xAttribute, "value");
                        if (!string.IsNullOrEmpty(entrefname))
                        {
                            value += ":" + entrefname;
                        }
                    }
                    result.SetAttribute(container, attribute, type, value);
                }
            }
            container.EndSection();
            return result;
        }

        /// <summary>Returns list of values from this entity with given attributes</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static List<string> EntityToTextLine(IExecutionContainer container, Entity entity, List<string> attributes)
        {
            var record = new List<string>();
            foreach (var attribute in attributes)
            {
                if (attribute == "Entity")
                {
                    record.Add(entity.LogicalName);
                }
                else if (attribute == "Id")
                {
                    record.Add(entity.Id.ToString());
                }
                else
                {
                    var value = "<null>";
                    if (entity.Contains(attribute) && entity[attribute] != null)
                    {
                        value = container.AttributeAsBaseType(entity, attribute, "", false).ToString();
                        if (entity[attribute] is EntityReference)
                        {
                            value = entity.GetAttribute<EntityReference>(attribute, null).LogicalName + ":" + value;
                        }
                    }
                    record.Add(value);
                }
            }
            return record;
        }

        private static XmlDocument Serialize(this Entity entity, IExecutionContainer container, string primarykeyattribute, SerializationStyle style, XmlNode parentNode)
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
                //case Common.SerializationStyle.Full:
                //    SerializeFull(result);
                //    break;

                case SerializationStyle.Simple:
                case SerializationStyle.SimpleWithValue:
                case SerializationStyle.SimpleNoId:
                    entity.SerializeSimple(container, primarykeyattribute, style, parentNode, result);
                    break;

                case SerializationStyle.Explicit:
                    entity.SerializeExplicit(container, primarykeyattribute, parentNode, result);
                    break;

                default:
                    throw new NotImplementedException($"SerializationStyle {style} is not implemented for Slim methods");
            }
            return result;
        }

        private static void SerializeExplicit(this Entity entity, IExecutionContainer container, string primarykeyattribute, XmlNode parentNode, XmlDocument result)
        {
            XmlNode xEntity = result.CreateElement(entity.LogicalName);
            var xEntityId = result.CreateAttribute("id");
            xEntityId.Value = entity.Id.ToString();
            xEntity.Attributes.Append(xEntityId);
            foreach (var attribute in entity.Attributes)
            {
                if (attribute.Key == primarykeyattribute)
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

        private static void SerializeSimple(this Entity entity, IExecutionContainer container, string primarykeyattribute, SerializationStyle style, XmlNode parentNode, XmlDocument result)
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
                if (attribute.Key == primarykeyattribute)
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
    }
}