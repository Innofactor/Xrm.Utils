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
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extension methods for IContainable classes
    /// </summary>
    public static partial class ContainerExtensions
    {
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

            var response = container.Service.Execute(new SetStateRequest()
            {
                EntityMoniker = entity.ToEntityReference(),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            }) as SetStateResponse;

            container.Log("SetState completed");

            return response;
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
            var id = StringToGuidish(container, strId);
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
                    result.Attributes.Add(attribute, value); 
                }
            }
            container.EndSection();
            return result;
        }
        internal static Guid StringToGuidish(IExecutionContainer container, string strId)
        {
            var id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(strId) &&
                !Guid.TryParse(strId, out id))
            {
                container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
                container.Log("String: {0}", strId);

                var template = "FFFFEEEEDDDDCCCCBBBBAAAA99998888";

                if (Guid.TryParse(template.Substring(0, 32 - strId.Length) + strId, out id))
                {
                    container.Log("Composed temporary guid from template + incomplete id: {0}", id);
                }
                else
                {
                    container.Log("Failed to compose temporary guid from: {0}", strId);
                }
                container.EndSection();
            }
            return id;
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
    }
}