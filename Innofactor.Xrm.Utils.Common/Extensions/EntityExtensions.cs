﻿namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Light-weight features inspired by CintDynEntity
    /// </summary>
    public static partial class EntityExtensions
    {
        #region Public Methods

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
        /// Sätter current user (from context) som owner på entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        public static void SetOwner(this Entity entity, Guid user) =>
            entity.SetAttribute("ownerid", new EntityReference("systemuser", user));

        #endregion Public Methods

        #region Private Methods

        private static object Entity(object p)
        {
            throw new NotImplementedException();
        }

        private static List<string> extractPrefixes(string entityName)
        {
            var result = new List<string>();
            var prefix = new StringBuilder();
            while (entityName.Contains("_"))
            {
                prefix.Append(entityName.Split('_')[0] + "_");
                entityName = entityName.Substring(entityName.IndexOf('_') + 1);
                result.Add(prefix.ToString());
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
        
        #endregion Private Methods
    }
}