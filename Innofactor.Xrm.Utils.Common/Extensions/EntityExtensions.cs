namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.Text;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Light-weight features inspired by CintDynEntity
    /// </summary>
    public static partial class EntityExtensions
    {
        private static readonly ConcurrentDictionary<string, string> PrimaryIdAttributes = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> PrimaryNameAttributes = new ConcurrentDictionary<string, string>();

        /// <summary>Assigns current record to given assignee</summary>
        /// <param name="entity">The entity you'd like to assign to the given user/team in the parameter</param>
        /// <param name="container">container object</param>
        /// <param name="assignee">User/Team to assigne the record to</param>
        public static void Assign(this Entity entity, IExecutionContainer container, EntityReference assignee)
        {
            container.Service.Execute(new AssignRequest()
            {
                Assignee = assignee,
                Target = entity.ToEntityReference()
            });
            container.Logger.Log($"Assigned {entity.ToStringExt()} to {assignee.LogicalName} {assignee.Id}");
        }

        /// <summary>
        /// Clones entity instance to a new C# instance
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Cloned entity</returns>
        /// <remarks>Does NOT create a copy in database, just a new copy to work with in code.</remarks>
        public static Entity CloneAttributes(this Entity entity)
        {
            var clone = CloneId(entity);

            // Preparing all attributes except the one in which entity id is stored
            var attributes = entity.Attributes.Where(x => x.Key.ToLowerInvariant() != $"{clone.LogicalName}id".ToLowerInvariant() || (Guid)x.Value != clone.Id);

            foreach (var attribute in attributes)
            {
                if (!clone.Attributes.Contains(attribute.Key))
                {
                    clone.Attributes.Add(attribute);
                }
            }

            return clone;
        }

        /// <summary>
        /// Clones entity instance to a new C# instance
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Cloned entity</returns>
        /// <remarks>Does NOT create a copy in database, just a new copy to work with in code.</remarks>
        public static Entity CloneId(this Entity entity) =>
            new Entity(entity.LogicalName, entity.Id);

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="notnull"></param>
        /// <returns></returns>
        public static bool Contains(this Entity entity, string name, bool notnull) =>
            entity.Attributes.Contains(name) && (!notnull || entity.Attributes[name] != null);

        /// <summary>
        /// Generic method to retrieve property with name "name" of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="attribute"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Entity entity, string attribute, T @default) =>
            (T)(object)(entity.Contains(attribute) && entity[attribute] is T ? (T)entity[attribute] : @default);

        /// <summary>Gets bool indicating if record is active (writable) or inactive.</summary>
        /// <param name="entity"></param>
        /// <param name="default">Default value if statecode is missing in entity.</param>
        /// <returns></returns>
        public static bool IsActive(this Entity entity, bool @default)
        {
            try
            {
                return IsActive(entity);
            }
            catch (InvalidPluginExecutionException)
            {
                return @default;
            }
        }

        /// <summary>Returns true if entity has an active state. If statecode is not available in the attribute collection an exception is thrown.</summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsActive(this Entity entity)
        {
            var result = false;

            if (!entity.Attributes.Contains("statecode"))
            {
                throw new InvalidPluginExecutionException($"Querying statecode which is not currently available for {entity.LogicalName}");
            }
            else
            {
                if (((OptionSetValue)entity.Attributes["statecode"]).Value == 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <returns></returns>
        public static Entity Merge(this Entity entity1, Entity entity2)
        {
            var merge = entity1.CloneAttributes();
            foreach (var prop in entity2.Attributes)
            {
                if (!merge.Attributes.Contains(prop.Key))
                {
                    merge.Attributes.Add(prop);
                }
            }

            return merge;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <remarks>Was RemoveProperty before</remarks>
        public static void RemoveAttribute(this Entity entity, string name)
        {
            if (entity.Contains(name))
            {
                entity.Attributes.Remove(name);
            }
        }

        /// <summary>
        /// Generic method to add property with "name" and set its value of type "T" to "value"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetAttribute<T>(this Entity entity, string name, T value)
        {
            if (entity.Attributes.Contains(name))
            {
                entity.Attributes[name] = value;
            }
            else
            {
                entity.Attributes.Add(name, value);
            }
        }

        /// <summary>
        /// Attempts to set attribute on entity to the type mentioned. Removes the attribute in case of null
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <remarks>Previously called AddProperty/SetProperty. Used in Shuffle deserialization </remarks>
        public static void SetAttribute(this Entity entity, IExecutionContainer container, string attribute, string type, string value)
        {
            container.StartSection($@"{MethodBase.GetCurrentMethod().DeclaringType.Name}\{MethodBase.GetCurrentMethod().Name}");
            try
            {
                container.Log($@"{attribute} = ""{value}"" ({type})");
                switch (type)
                {
                    case "String":
                    case "Memo":
                        entity.SetAttribute(attribute, value);

                        break;

                    case "Int32":
                    case "Integer":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, int.Parse(value));
                        }
                        break;

                    case "Int64":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, long.Parse(value));
                        }
                        break;

                    case "OptionSetValue":
                    case "Picklist":
                    case "State":
                    case "Status":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, new OptionSetValue(int.Parse(value)));
                        }
                        break;

                    case "EntityReference":
                    case "Lookup":
                    case "Customer":
                    case "Owner":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var valueparts = value.Split(':');
                            var entityRef = valueparts[0];
                            value = valueparts[1];
                            var refId = container.StringToGuidish(value);
                            var entref = new EntityReference(entityRef, refId);
                            if (valueparts.Length > 2)
                            {
                                entref.Name = valueparts[2];
                            }
                            entity.SetAttribute(attribute, entref);
                        }
                        break;

                    case "DateTime":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));
                        }
                        break;

                    case "Boolean":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, StringToBool(value));
                        }
                        break;

                    case "Guid":
                    case "Uniqueidentifier":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var uId = container.StringToGuidish(value);
                            entity.SetAttribute(attribute, uId);
                        }
                        break;

                    case "Decimal":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, decimal.Parse(value));
                        }
                        break;

                    case "Money":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            entity.SetAttribute(attribute, new Money(decimal.Parse(value)));
                        }
                        break;

                    case "null":
                    case "<null>":
                        entity.Attributes.Remove(attribute);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("Type", type, "Cannot parse attibute type");
                }
            }
            finally
            {
                container.EndSection();
            }
        }

        /// <summary>
        /// Sets current user (from context) som owner på entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        public static void SetOwner(this Entity entity, Guid user) =>
            entity.SetAttribute("ownerid", new EntityReference("systemuser", user));

        /// <summary>Retrieves the entity instance from given relation attribute</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="related"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Entity GetRelated(this Entity entity, IExecutionContainer container, string related, params string[] columns) =>
            entity.GetRelated(container, related, new ColumnSet(columns));

        /// <summary>
        /// Will add given <paramref name="columns" /> to the operation
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Entity GetRelated(this Entity entity, IExecutionContainer container, string related, ColumnSet columns)
        {
            container.StartSection($"GetRelated {related} from {entity.ToStringExt()}");

            var result = default(Entity);

            if (entity.Attributes.Contains(related))
            {
                if (entity.Attributes[related] is EntityReference)
                {
                    result = container.Retrieve((EntityReference)entity.Attributes[related], columns);
                }
            }
            else
            {
                container.Log($"Record does not contain attribute {related}");
            }

            if (result == null)
            {
                container.Log("Could not load related record");
            }
            else
            {
                container.Log($"Loaded related {result.LogicalName} {result.ToStringExt()}");
            }

            container.EndSection();
            return result;
        }
        public static string ToStringExt(this Entity target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(target.LogicalName))
            {
                // Adding entity logical name
                result.Append(target.LogicalName);
            }

            if (!target.Id.Equals(Guid.Empty))
            {
                if (result.Length > 0)
                {
                    result.Append(":");
                }

                result.Append(target.Id.ToString());
            }

            foreach (var key in "name;fullname;title;subject".Split(';'))
            {
                if (target.Contains(key))
                {
                    if (result.Length > 0)
                    {
                        result.Append(" ");
                    }

                    result.Append("(");
                    result.Append(target.Attributes[key] as string);
                    result.Append(")");

                    break;
                }
            }

            return result.ToString();
        }

        /// <summary>Retrieves records (children) relating to current record (parent)</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="childEntityName">Name of child entity</param>
        /// <param name="referencingattribute">Name of attribute on child entity that relates to current entity</param>
        /// <param name="onlyactive">True to include statecode=0 for children</param>
        /// <param name="columns">Columns to return for retrieved children</param>
        /// <returns>Collection of children to parent</returns>
        public static EntityCollection GetRelating(this Entity entity, IExecutionContainer container, string childEntityName, string referencingattribute, bool onlyactive, params string[] columns) =>
            entity.GetRelating(container, childEntityName, referencingattribute, onlyactive, new ColumnSet(columns));

        /// <summary>Retrieves records (children) relating to current record (parent)</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="childEntityName">Name of child entity</param>
        /// <param name="referencingattribute">Name of attribute on child entity that relates to current entity</param>
        /// <param name="onlyactive">True to include statecode=0 for children</param>
        /// <param name="columns">Columns to return for retrieved children</param>
        /// <returns>Collection of children to parent</returns>
        public static EntityCollection GetRelating(this Entity entity, IExecutionContainer container, string childEntityName, string referencingattribute, bool onlyactive, ColumnSet columns) =>
            entity.GetRelating(container, childEntityName, referencingattribute, onlyactive, null, null, columns, false);

        /// <summary>Retrieves records (children) relating to current record (parent)</summary>
        /// <param name="container"></param>
        /// <param name="childEntityName"></param>
        /// <param name="entity">Name of child entity</param>
        /// <param name="referencingattribute">Name of attribute on child entity that relates to current entity</param>
        /// <param name="onlyactive">True to include statecode=0 for children</param>
        /// <param name="columns">Columns to return for retrieved children</param>
        /// <param name="nolock"></param>
        /// <returns>Collection of children to parent</returns>
        public static EntityCollection GetRelating(this Entity entity, IExecutionContainer container, string childEntityName, string referencingattribute, bool onlyactive, ColumnSet columns, bool nolock) =>
            entity.GetRelating(container, childEntityName, referencingattribute, onlyactive, null, null, columns, nolock);

        /// <summary>Retrieves records (children) relating to current record (parent)</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="childEntityName">Name of child entity</param>
        /// <param name="referencingattribute">Name of attribute on child entity that relates to current entity</param>
        /// <param name="onlyactive">True to include statecode=0 for children</param>
        /// <param name="extrafilter">Extra filters to use when retrieving children</param>
        /// <param name="orders">Order by definition</param>
        /// <param name="columns">Columns to return for retrieved children</param>
        /// <returns>Collection of children to parent</returns>
        public static EntityCollection GetRelating(this Entity entity, IExecutionContainer container, string childEntityName, string referencingattribute, bool onlyactive, FilterExpression extrafilter, OrderExpression[] orders, ColumnSet columns) =>
            entity.GetRelating(container, childEntityName, referencingattribute, onlyactive, extrafilter, orders, columns, false);

        /// <summary>Retrieves records (children) relating to current record (parent)</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="childEntityName">Name of child entity</param>
        /// <param name="referencingattribute">Name of attribute on child entity that relates to current entity</param>
        /// <param name="onlyactive">True to include statecode=0 for children</param>
        /// <param name="extrafilter">Extra filters to use when retrieving children</param>
        /// <param name="orders">Order by definition</param>
        /// <param name="columns">Columns to return for retrieved children</param>
        /// <param name="nolock">If 'true' query is executed with NoLock attribute = true, otherwise NoLock is "false" (by default)</param>
        /// <returns>Collection of children to parent</returns>
        public static EntityCollection GetRelating(this Entity entity, IExecutionContainer container, string childEntityName, string referencingattribute, bool onlyactive, FilterExpression extrafilter, OrderExpression[] orders, ColumnSet columns, bool nolock = false)
        {
            container.StartSection($"GetRelating {childEntityName} where {referencingattribute}={entity.Id} and active={onlyactive}");
            var qry = new QueryExpression(childEntityName);
            Query.AppendCondition(qry.Criteria, LogicalOperator.And, referencingattribute, ConditionOperator.Equal, entity.Id);
            if (onlyactive)
            {
                Query.AppendConditionActive(qry.Criteria);
            }

            qry.ColumnSet = columns;
            if (extrafilter != null)
            {
                container.Logger.Log($"Adding filter with {extrafilter.Conditions.Count} conditions");
                qry.Criteria.AddFilter(extrafilter);
            }

            if (orders != null && orders.Length > 0)
            {
                container.Logger.Log($"Adding orders ({orders.Length})");
                qry.Orders.AddRange(orders);
            }

            qry.NoLock = nolock;
            var result = container.RetrieveMultiple(qry);
            container.Logger.Log($"Got {result.Count()} records");
            container.EndSection();
            return result;
        }

        /// <summary>Get associated records from a N:N-relationship</summary>
        /// <param name="entity1"></param>
        /// <param name="container"></param>
        /// <param name="entity">Other entity to retrieve</param>
        /// <param name="intersect">Name of the relationship / intersect table</param>
        /// <param name="onlyactive">Specifies if only active records of other entity shall be returned</param>
        /// <param name="columns">ColumnSet of the other entity to retrieve</param>
        /// <returns>Associated other entity records</returns>
        public static EntityCollection GetAssociated(this Entity entity1, IExecutionContainer container, string entity, string intersect, bool onlyactive, ColumnSet columns) =>
            entity1.GetAssociated(container, entity, intersect, onlyactive, false, columns);

        /// <summary>Get associated records from a N:N-relationship</summary>
        /// <param name="entity1"></param>
        /// <param name="container"></param>
        /// <param name="entity">Other entity to retrieve (logicalName)</param>
        /// <param name="intersect">Name of the relationship / intersect table</param>
        /// <param name="onlyactive">Specifies if only active records of other entity shall be returned</param>
        /// <param name="nolock">If 'true' query is executed with NoLock attribute</param>
        /// <param name="columns">ColumnSet of the other entity to retrieve</param>
        /// <returns>Associated other entity records</returns>
        public static EntityCollection GetAssociated(this Entity entity1, IExecutionContainer container, string entity, string intersect, bool onlyactive, bool nolock, ColumnSet columns)
        {
            container.StartSection($"Slim: GetAssociated({entity}, {intersect}, {onlyactive})");
            var thisIdAttribute = entity1.GetPrimaryIdAttribute(container); //container.Service.IdAttribute(entity1.LogicalName, container.Logger);
            var otherIdAttribute = new Entity(entity).GetPrimaryIdAttribute(container); //container.Service.IdAttribute(entity, container.Logger);
            var qry = new QueryExpression(entity);
            if (entity != entity1.LogicalName)
            {   // N:N mellan olika entiteter
                var leSource = Query.AppendLinkMM(qry.LinkEntities, intersect, entity, otherIdAttribute, entity1.LogicalName, thisIdAttribute);
                Query.AppendCondition(leSource.LinkCriteria, LogicalOperator.And, thisIdAttribute, ConditionOperator.Equal, entity1.Id);
            }
            else
            {   // N:N till samma enititet
                var leSource = Query.AppendLink(qry.LinkEntities, entity, intersect, otherIdAttribute, thisIdAttribute + "two");
                Query.AppendCondition(leSource.LinkCriteria, LogicalOperator.And, thisIdAttribute + "one", ConditionOperator.Equal, entity1.Id);
            }
            if (onlyactive)
            {
                Query.AppendConditionActive(qry.Criteria);
            }

            qry.ColumnSet = columns;
            qry.NoLock = nolock;
            var fetchXml = container.ConvertToFetchXml(qry);
            container.Logger.Log(fetchXml);
            var result = container.RetrieveMultiple(qry);
            container.EndSection();
            return result;
        }

        /// <summary>
        /// Get the primary id attribute of the target entity
        /// </summary>
        /// <remarks>Metadata request that is time consuming, but results are cached.</remarks>
        /// <returns></returns>
        public static string GetPrimaryIdAttribute(this Entity entity, IExecutionContainer container)
        {
            var result = string.Empty;

            if (PrimaryIdAttributes.ContainsKey(entity.LogicalName))
            {
                result = PrimaryIdAttributes[entity.LogicalName];
            }
            else
            {
                try
                {
                    var request = new RetrieveEntityRequest()
                    {
                        LogicalName = entity.LogicalName,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };

                    var response = (RetrieveEntityResponse)container.Service.Execute(request);

                    var metabase = response.EntityMetadata;

                    container.Logger.Log("Metadata retrieved");
                    if (metabase is EntityMetadata meta)
                    {
                        result = meta.PrimaryIdAttribute;
                        PrimaryIdAttributes.TryAdd(entity.LogicalName, result);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            $"Unable to retrieve metadata/primaryattribute for entity: {entity.LogicalName}. Metadata is: {metabase}");
                    }
                    container.Logger.Log($"Returning {result}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Message.Contains("Could not find an entity with specified entity name"))
                    {
                        container.Logger.Log("Slim: GetPrimaryIdAttribute: Entity not found");
                        PrimaryNameAttributes.TryRemove(entity.LogicalName, out result);
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    container.Logger.EndSection();
                }
            }
            return result;
        }

        /// <summary>
        /// Gets primary name attribute of the entity
        /// </summary>
        /// <returns></returns>
        public static string GetPrimaryNameAttribute(this Entity entity, IExecutionContainer container)
        {
            var result = string.Empty;

            if (PrimaryNameAttributes.ContainsKey(entity.LogicalName))
            {
                result = PrimaryNameAttributes[entity.LogicalName];
            }
            else
            {
                container.Logger.StartSection($"Getting name of primary attribute on '{entity.LogicalName}'");
                try
                {
                    var request = new RetrieveEntityRequest()
                    {
                        LogicalName = entity.LogicalName,
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true
                    };

                    var response = (RetrieveEntityResponse)container.Service.Execute(request);

                    var metabase = response.EntityMetadata;

                    container.Logger.Log("Metadata retrieved");
                    if (metabase is EntityMetadata meta)
                    {
                        result = meta.PrimaryNameAttribute;
                        PrimaryNameAttributes.TryAdd(entity.LogicalName, result);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            $"Unable to retrieve metadata/primaryattribute for entity: {entity.LogicalName}. Metadata is: {metabase.ToString()}");
                    }
                    container.Logger.Log($"Returning {result}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Message.Contains("Could not find an entity with specified entity name"))
                    {
                        container.Logger.Log("Slim: GetPrimaryNameAttribute: Entity not found");
                        PrimaryNameAttributes.TryRemove(entity.LogicalName, out result);
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    container.Logger.EndSection();
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool StringToBool(string value)
        {
            if (value == "0")
            {
                return false;
            }
            else if (value == "1")
            {
                return true;
            }
            else
            {
                return bool.Parse(value);
            }
        }

        /// <summary>Gives "rights" to "principal" for current record
        /// Details: https://docs.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.grantaccessrequest
        /// </summary>
        /// <param name="entity">The entity on which the access needs to be granted</param>
        /// <param name="container"></param>
        /// <param name="principal">User or Team to grant access to</param>
        /// <param name="rights">Rights to grant to user/team</param>
        public static void GrantAccessTo(this Entity entity, IExecutionContainer container, EntityReference principal, AccessRights rights)
        {
            container.Service.Execute(new GrantAccessRequest()
            {
                PrincipalAccess = new PrincipalAccess()
                {
                    Principal = principal,
                    AccessMask = rights
                },
                Target = entity.ToEntityReference()
            });
            container.Logger.Log($"Granted {rights} on {entity.ToStringExt()} to {principal.LogicalName} {principal.Id}");
        }
        /// <summary>Retrieves current AccessRights for given principal on current record</summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="principal">User or Team to read access for</param>
        /// <returns>Current access</returns>
        public static AccessRights GetAccessFor(this Entity entity, IExecutionContainer container, EntityReference principal)
        {
            var accessResponse = (RetrievePrincipalAccessResponse)container.Service.Execute(new RetrievePrincipalAccessRequest()
            {
                Principal = principal,
                Target = entity.ToEntityReference()
            });
            var result = accessResponse.AccessRights;
            container.Logger.Log($"Read access {result} on {entity.ToStringExt()} for {principal.LogicalName} {principal.Id}");
            return result;
        }
        /// <summary>Removes access from revokee on current record
        /// Details: https://docs.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.revokeaccessrequest
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="container"></param>
        /// <param name="principal">User or Team to revoke access from</param>
        public static void RevokeAccessFrom(this Entity entity, IExecutionContainer container, EntityReference principal)
        {
            container.Service.Execute(new RevokeAccessRequest()
            {
                // Note: "user" är inte "systemuser" - jag förstår idag 2012-04-16 inte varför denna kodrad finns. Incheckad 2011-02-14.
                Revokee = principal,
                Target = entity.ToEntityReference()
            });
            container.Logger.Log($"Revoked {principal.LogicalName} {principal.Id} from {entity.ToStringExt()}");
        }
    }
}