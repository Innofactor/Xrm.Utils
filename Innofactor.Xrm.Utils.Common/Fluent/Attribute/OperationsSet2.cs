namespace Innofactor.Xrm.Utils.Common.Fluent.Attribute
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;

    public class OperationsSet2 : Information
    {
        #region Internal Constructors

        internal OperationsSet2(IExecutionContainer container, string name, Entity target)
            : base(container, name, target)
        {
        }

        #endregion Internal Constructors

        #region Public Methods

        /// <summary>
        /// Will add given <paramref name="columns" /> to the operation
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Entity Expand(ColumnSet columns)
        {
            container.StartSection($"Slim: GetRelated {name} from {container.Entity(target).ToString()}");

            var result = default(Entity);
  
            if (target.Attributes.Contains(name))
            {
                if (target.Attributes[name] is EntityReference)
                {
                    result = container.Retrieve((EntityReference)target.Attributes[name], columns);
                }
            }
            else
            {
                container.Log($"Record does not contain attribute {name}");
            }

            if (result == null)
            {
                container.Log("Could not load related record");
            }
            else
            {
                container.Log($"Loaded related {result.LogicalName} {container.Entity(result).ToString()}");
            }

            container.EndSection();
            return result;
        }

        /// <summary>
        /// Will add given <paramref name="columns" /> to the operation
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Entity Expand(params string[] columns) =>
            Expand(new ColumnSet(columns));

        /// <summary>
        /// Gets a readable string representation of given attribute
        /// </summary>
        /// <returns>Formatted value of the attribute</returns>
        public override string ToString()
        {
            container.StartSection("ToString");

            try
            {
                if (!string.IsNullOrWhiteSpace(name) && target.Contains(name))
                {
                    if (target.FormattedValues.Contains(name) && !string.IsNullOrEmpty(target.FormattedValues[name]))
                    {
                        return target.FormattedValues[name];
                    }
                    else
                    {
                        return ToString(name, target[name]);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            finally
            {
                container.EndSection();
            }
        }

        #endregion Public Methods

        #region Private Methods

        private string ToString(string attributeName, object attributeValue)
        {
            if (attributeValue is AliasedValue)
            {
                container.Log("Attribute is of type `AliasedValue`");

                return ToString(((AliasedValue)attributeValue).AttributeLogicalName, ((AliasedValue)attributeValue).Value);
            }
            else if (attributeValue is EntityReference reference)
            {
                container.Log("Attribute is of type `EntityReference`");

                if (!string.IsNullOrEmpty(reference.Name))
                {
                    container.Log("Reference name was given");
                    return reference.Name;
                }
                else if (container.Service != null)
                {
                    var primaryAttribute = ((RetrieveEntityResponse)container.Service.Execute(new RetrieveEntityRequest
                    {
                        LogicalName = reference.LogicalName,
                        EntityFilters = EntityFilters.Attributes,
                        RetrieveAsIfPublished = true
                    })).EntityMetadata.PrimaryNameAttribute;

                    var entity = container.Service.Retrieve(reference.LogicalName, reference.Id, new ColumnSet(primaryAttribute));
                    if (entity.Contains(primaryAttribute))
                    {
                        return entity.Attributes[primaryAttribute].ToString();
                    }
                }
            }
            else if (attributeValue is EntityCollection && ((EntityCollection)attributeValue).EntityName == "activityparty")
            {
                container.Log("Attribute is of type `EntityCollection`");

                var result = new StringBuilder();
                if (((EntityCollection)attributeValue).Entities.Count > 0)
                {
                    var partyAdded = false;
                    foreach (var activityparty in ((EntityCollection)attributeValue).Entities)
                    {
                        var party = string.Empty;
                        if (activityparty.Contains("partyid") && activityparty["partyid"] is EntityReference)
                        {
                            party = ((EntityReference)activityparty["partyid"]).Name;
                        }

                        if (string.IsNullOrEmpty(party) && activityparty.Contains("addressused"))
                        {
                            party = activityparty["addressused"].ToString();
                        }

                        if (string.IsNullOrEmpty(party))
                        {
                            party = activityparty.Id.ToString();
                        }

                        if (partyAdded)
                        {
                            result.Append(", ");
                        }

                        result.Append(party);
                        partyAdded = true;
                    }
                }

                return result.ToString();
            }
            else if (attributeValue is OptionSetValue)
            {
                container.Log("Attribute is of type `OptionSetValue`");

                if (container.Service != null)
                {
                    var optatt = (OptionSetValue)attributeValue;
                    var retrieveAttributeResponse = (RetrieveAttributeResponse)container.Service.Execute(new RetrieveAttributeRequest()
                    {
                        EntityLogicalName = target.LogicalName,
                        LogicalName = attributeName,
                        RetrieveAsIfPublished = true
                    });
                    var plAMD = (EnumAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                    foreach (var oMD in plAMD.OptionSet.Options)
                    {
                        if (oMD.Value == optatt.Value)
                        {
                            return oMD.Label?.UserLocalizedLabel?.Label ?? oMD.Label.LocalizedLabels[0]?.Label;
                        }
                    }

                    return ""; // OptionSet value not found!
                }
            }
            else if (attributeValue is DateTime)
            {
                container.Log("Attribute is of type `DateTime`");

                return ((DateTime)attributeValue).ToString("G");
            }
            else if (attributeValue is Money)
            {
                container.Log("Attribute is of type `Money`");

                return ((Money)attributeValue).Value.ToString("C");
            }

            if (attributeValue != null)
            {
                container.Log("Attribute will be automatically converted to string");

                return attributeValue.ToString();
            }
            else
            {
                container.Log("Attribute will be evaluated to null");

                return null;
            }
        }

        #endregion Private Methods
    }
}