namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using ElencySolutions.CsvHelper;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extension methods for IContainable classes
    /// </summary>
    public static partial class ContainerExtensions
    {
        #region Public Methods

        /// <summary>Associates current record with relatedentity, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity">Current entity</param>
        /// <param name="relatedentity">Related entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when association already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, Entity relatedentity, string intersect)
        {
            var collection = new EntityCollection();
            collection.Add(relatedentity);
            container.Associate(entity, collection, intersect);
        }

        /// <summary>Associates current record with relatedentities, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="relatedEntities">Collection of the entities to be related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            container.Associate(entity, relatedEntities, intersect, int.MaxValue);
        }

        /// <summary>Associates current record with relatedentities, using specified intersect relationship</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="relatedEntities">Collection of the entities to be related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <param name="batchSize">Optional. Determines the max number of entities to associate per request</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Associate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect, int batchSize)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var role = default(EntityRole?);
            if (relatedEntities.Entities.Count > 0 && relatedEntities[0].LogicalName == entity.LogicalName)
            {
                // N:N-relation to the same entity, so role have to be specified.
                role = EntityRole.Referencing;
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be larger than zero.");
            }

            var entRefCollection = relatedEntities.ToEntityReferenceCollection();
            var processed = 0;
            while (processed < relatedEntities.Entities.Count)
            {
                var batch = new EntityReferenceCollection(entRefCollection.Skip(processed).Take(batchSize).ToList());
                processed += batch.Count();

                var req = new AssociateRequest
                {
                    Target = entity.ToEntityReference(),
                    Relationship = new Relationship(intersect)
                    {
                        PrimaryEntityRole = role
                    },
                    RelatedEntities = batch
                };
                container.Service.Execute(req);
                container.Log("Associated {0} {1} with {2}", batch.Count, relatedEntities.Entities.Count > 0 ? relatedEntities[0].LogicalName : "", entity.LogicalName);
            }
        }

        /// <summary>Gets the value of a property derived to its base type</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <param name="supresserrors"></param>
        /// <returns>Base type of attribute</returns>
        /// <remarks>Translates <c>AliasedValue, EntityReference, OptionSetValue and Money</c> to their underlying base types</remarks>
        public static object AttributeAsBaseType(this IExecutionContainer container, Entity entity, string name, object @default, bool supresserrors)
        {
            if (!entity.Contains(name))
            {
                if (!supresserrors)
                {
                    throw new InvalidPluginExecutionException(string.Format("Attribute {0} not found in entity {1} {2}", name, entity.LogicalName, container.Entity(entity).ToString()));
                }
                else
                {
                    return @default;
                }
            }
            return AttributeToBaseType(entity[name]);
        }

        /// <summary>
        /// Converts QueryExpression to FetchXml
        /// </summary>
        /// <param name="container"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string Convert(this IExecutionContainer container, QueryExpression query)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            try
            {
                var request = new QueryExpressionToFetchXmlRequest()
                {
                    Query = query
                };

                var response = (QueryExpressionToFetchXmlResponse)container?.Service?.Execute(request);

                if (response != null)
                {
                    container.Log("Query was converted successfully.");
                }
                else
                {
                    container.Log("It was an issue converting query.");
                }

                return response?.FetchXml;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>Constructor for EntityCollection class, initializing collection with serialized entities</summary>
        /// <param name="container"></param>
        /// <param name="serializedEntities"></param>
        public static EntityCollection CreateEntityCollection(this IExecutionContainer container, XmlDocument serializedEntities)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            try
            {
                if (serializedEntities != null && serializedEntities.ChildNodes.Count > 0)
                {
                    var entityCollection = new EntityCollection();
                    if (serializedEntities.ChildNodes[0].Name == "Entities")
                    {
                        foreach (XmlNode xEntity in serializedEntities.ChildNodes[0].ChildNodes)
                        {
                            entityCollection.Add(container.Deserialize(xEntity));
                        }
                    }
                    else
                    {
                        List<Entity> entities;
                        var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                        var sr = new StringReader(serializedEntities.OuterXml);
                        using (var reader = new XmlTextReader(sr))
                        {
                            entities = (List<Entity>)serializer.ReadObject(reader);
                        }
                        sr.Close();
                        foreach (var entity in entities)
                        {
                            entityCollection.Add(entity);
                        }
                    }
                    return entityCollection;
                }
                else
                {
                    return new EntityCollection();
                }
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>Constructor for EntityCollection class, initializing collection with text file entities</summary>
        /// <param name="container"></param>
        /// <param name="text"></param>
        /// <param name="delimeter"></param>
        public static EntityCollection CreateEntityCollection(this IExecutionContainer container, string text, char delimeter)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            try
            {
                container.Log($"Text {text.Length} characters, Delimeter: {delimeter}");
                var entityCollection = new EntityCollection();
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
                        var current = reader.Fields;
                        if (current.Count >= 1)
                        {   // Two columns must exist to have anything at all - Entity and Id (although Id can actually be empty)
                            if (types == null)
                            {   //
                                if (current[0] == "String" && current[1] == "Guid")
                                {   // Types not yet defined
                                    container.Log("Assigning types from serialized text");
                                    types = current;
                                    continue;
                                }
                                else
                                {
                                    container.StartSection("Retrieving types from metadata");
                                    var entity = current[0];
                                    types = new List<string>(columns.Count);
                                    // Idiotisk kod bara för att jag inte vet hur man hanterar listor vettigt, eller nåt //JR
                                    for (var i = 0; i < columns.Count; i++) { types.Add(""); }

                                    types[0] = "String";    // Entity name type
                                    types[1] = "Guid";      // Record id type
                                    container.Log($"Retrieving metadata for {entity}");

                                    var metadata = (EntityMetadata)container.Execute(new RetrieveEntityRequest()
                                    {
                                        LogicalName = entity,
                                        EntityFilters = EntityFilters.Attributes,
                                        RetrieveAsIfPublished = true
                                    });
                                    foreach (var attrmeta in metadata.Attributes)
                                    {
                                        if (columns.Contains(attrmeta.LogicalName))
                                        {
                                            container.Log($"Column: {attrmeta.LogicalName} is {attrmeta.AttributeType}");
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

                            entityCollection.Add(container.InitFromTextLine(columns, types, current));
                        }
                    }
                }
                return entityCollection;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>Deserializes a file into a list of entities</summary>
        /// <param name="container"></param>
        /// <param name="filename">Source file</param>
        /// <returns>List of entities</returns>
        public static List<Entity> Deserialize(this IExecutionContainer container, string filename)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            List<Entity> oObject;

            try
            {
                var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                var reader = new XmlTextReader(filename);

                oObject = (List<Entity>)serializer.ReadObject(reader);
                return oObject;
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>Deserialize Entity from XML node</summary>
        /// <param name="container"></param>
        /// <param name="xEntity"></param>
        /// <returns></returns>
        public static Entity Deserialize(this IExecutionContainer container, XmlNode xEntity)
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

        /// <summary>Disassociates current record from relatedentities, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity">Current entity</param>
        /// <param name="relatedentity">Related entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when association already exists.
        /// </para>
        /// </exception>
        public static void Disassociate(this IExecutionContainer container, Entity entity, Entity relatedentity, string intersect)
        {
            var collection = new EntityCollection();
            collection.Add(relatedentity);
            container.Disassociate(entity, collection, intersect);
        }

        /// <summary>Disassociates current record from relatedentities, using specified intersect relationship</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="relatedEntities">Collection of the entities related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Disassociate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            container.Disassociate(entity, relatedEntities, intersect, int.MaxValue);
        }

        /// <summary>Disassociates current record from relatedentities, using specified intersect relationship</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="relatedEntities">Collection of the entities related to current entity</param>
        /// <param name="intersect">Name of the intersect relationship/entity</param>
        /// <param name="batchSize">Optional. Determines the max number of entities to associate per request</param>
        /// <remarks>To be used with N:N-relationships.</remarks>
        /// <exception cref="FaultException{TDetail}">
        /// <strong>TDetail</strong> may be typed as:
        /// <para>
        /// <see cref="OrganizationServiceFault" />: Thrown when any of the associations already exists.
        /// </para>
        /// </exception>
        public static void Disassociate(this IExecutionContainer container, Entity entity, EntityCollection relatedEntities, string intersect, int batchSize)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var role = default(EntityRole?);
            if (relatedEntities.Entities.Count > 0 && relatedEntities[0].LogicalName == entity.LogicalName)
            {
                // N:N-relation to the same entity, so role have to be specified.
                role = EntityRole.Referencing;
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be larger than zero.");
            }

            var entRefCollection = relatedEntities.ToEntityReferenceCollection();
            var processed = 0;
            while (processed < relatedEntities.Entities.Count)
            {
                var batch = new EntityReferenceCollection(entRefCollection.Skip(processed).Take(batchSize).ToList());
                processed += batch.Count();

                var req = new DisassociateRequest
                {
                    Target = entity.ToEntityReference(),
                    Relationship = new Relationship(intersect)
                    {
                        PrimaryEntityRole = role
                    },
                    RelatedEntities = batch
                };
                container.Service.Execute(req);
                container.Log("Disassociated {0} {1} with {2}", batch.Count, relatedEntities.Entities.Count > 0 ? relatedEntities[0].LogicalName : "", entity.LogicalName);
            }
        }

        /// <summary>
        /// Checks if a property exists in the encapsulated Entity
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="attribute">Name of property to check</param>
        /// <returns></returns>
        public static Entity Ensure(this IExecutionContainer container, Entity entity, string attribute) =>
            entity.Contains(attribute)
            ? entity
            : container.Reload(entity, attribute);

        /// <summary>Returns a list of states which indicate that a record of given entityname is active.</summary>
        /// <param name="container"></param>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns>List of active states</returns>
        public static List<int> GetActiveStates(this IExecutionContainer container, string entityName)
        {
            var result = new List<int>();
            if (!Constants.StatecodelessEntities.Contains(entityName))
            {
                switch (entityName)
                {
                    case "activitypointer":
                    case "appointment":
                        result.Add(0);
                        result.Add(3);
                        break;

                    case "quote":
                    case "salesorder":
                        result.Add(0);
                        result.Add(1);
                        break;

                    default:
                        // All other entities - active if state=0
                        result.Add(0);
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Version of currently connected CRM environment
        /// </summary>
        public static Version GetCrmVersion(this IExecutionContainer container)
        {
            var resp = (RetrieveVersionResponse)container.Execute(new RetrieveVersionRequest());

            return new Version(resp.Version);
        }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <returns></returns>
        public static Entity Merge(this IExecutionContainer container, Entity entity1, Entity entity2)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            container.Log($"Merging {entity1.LogicalName} {container.Entity(entity1).ToString()} with {entity2.LogicalName} {container.Entity(entity2).ToString()}");

            var merge = entity1.CloneAttributes();
            foreach (var prop in entity2.Attributes)
            {
                if (!merge.Attributes.Contains(prop.Key))
                {
                    merge.Attributes.Add(prop);
                }
            }

            container.Log($"Base entity had {entity1.Attributes.Count} attributes. Second entity {entity2.Attributes.Count}. Merged entity has {merge.Attributes.Count}");
            container.EndSection();
            return merge;
        }

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity) =>
            container.Reload(entity, new ColumnSet());

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity, params string[] columns) =>
            container.Reload(entity, new ColumnSet(columns));

        /// <summary>Reloads encapsulated Entity from database</summary>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="columns">Set of colums with which entity should be reloaded</param>
        /// <remarks>ToStringWithEntityName() is replaced with entity.LogicalName</remarks>
        public static Entity Reload(this IExecutionContainer container, Entity entity, ColumnSet columns)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            container.StartSection($"Reloading {container.Entity(entity).ToString()}.");

            foreach (var attr in entity.Attributes.Keys)
            {
                if (attr.Contains('.'))
                {
                    throw new InvalidPluginExecutionException($"Cannot reload entity {entity.LogicalName} with aliased attributes ({attr})");
                }
            }

            if (columns == null)
            {
                columns = new ColumnSet();
            }

            if (!columns.Columns.Any() && !columns.AllColumns)
            {
                foreach (var attr in entity.Attributes.Keys)
                {
                    columns.AddColumn(attr);
                }
            }

            entity = container.Retrieve(entity.ToEntityReference(), columns);
            container.EndSection();

            return entity;
        }

        /// <summary>Serialize a list of entities to file</summary>
        /// <param name="container"></param>
        /// <param name="entity">Entity to serialize</param>
        /// <param name="filename">Target file</param>
        /// <param name="formatting">Formatting, determines if indentation and line feeds are used in the file</param>
        public static void Serialize(this IExecutionContainer container, Entity entity, string filename, Formatting formatting)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            try
            {
                var serializer = new DataContractSerializer(typeof(Entity), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                var writer = new XmlTextWriter(filename, Encoding.UTF8)
                {
                    Formatting = formatting
                };
                serializer.WriteObject(writer, entity);
                writer.Close();
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>Serialize a list of entities to file</summary>
        /// <param name="container"></param>
        /// <param name="entities">List of entities to serialize</param>
        /// <param name="filename">Target file</param>
        /// <param name="formatting">Formatting, determines if indentation and line feeds are used in the file</param>
        public static void Serialize(this IExecutionContainer container, List<Entity> entities, string filename, Formatting formatting)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");

            var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
            var writer = new XmlTextWriter(filename, Encoding.UTF8)
            {
                Formatting = formatting
            };
            try
            {
                serializer.WriteObject(writer, entities);
            }
            catch (Exception ex)
            {
                container.Log(ex);
                throw;
            }
            finally
            {
                writer.Close();
                container.EndSection();
            }
        }

        /// <summary>Update state and status of current record</summary>
        /// <remarks>
        /// http://msdynamicscrmblog.wordpress.com/2013/10/26/status-and-status-reason-values-in-dynamics-crm-2013/comment-page-1/
        /// ToStringWithEntityName() is replaced with entity.LogicalName
        /// </remarks>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        /// <param name="state">Active=0 and Inactive=1</param>
        /// <param name="status">Active=1 and Inactive=2</param>
        public static SetStateResponse SetState(this IExecutionContainer container, Entity entity, int state, int status)
        {
            container.Log($"Setting state {state} {status} on {entity.LogicalName}");

            var response = container.Execute(new SetStateRequest()
            {
                EntityMoniker = entity.ToEntityReference(),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            }) as SetStateResponse;

            container.Log("SetState completed");

            return response;
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="strId"></param>
        /// <returns></returns>
        internal static Guid StringToGuidish(this IExecutionContainer container, string strId)
        {
            var id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(strId) &&
                !Guid.TryParse(strId, out id))
            {
                container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
                container.Log($"String: {strId}");

                var template = "FFFFEEEEDDDDCCCCBBBBAAAA99998888";

                if (Guid.TryParse(template.Substring(0, 32 - strId.Length) + strId, out id))
                {
                    container.Log($"Composed temporary guid from template + incomplete id: {id}");
                }
                else
                {
                    container.Log($"Failed to compose temporary guid from: {strId}");
                }
                container.EndSection();
            }
            return id;
        }

        #endregion Internal Methods

        #region Private Methods

        private static object AttributeToBaseType(object attribute)
        {
            if (attribute is AliasedValue)
            {
                return AttributeToBaseType(((AliasedValue)attribute).Value);
            }
            else if (attribute is EntityReference)
            {
                return ((EntityReference)attribute).Id;
            }
            else if (attribute is OptionSetValue)
            {
                return ((OptionSetValue)attribute).Value;
            }
            else if (attribute is Money)
            {
                return ((Money)attribute).Value;
            }
            else
            {
                return attribute;
            }
        }

        #endregion Private Methods
    }
}