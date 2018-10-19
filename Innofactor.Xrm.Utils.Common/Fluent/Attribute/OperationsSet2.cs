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
        internal OperationsSet2(IExecutionContainer container, string name, Entity target)
            : base(container, name, target)
        {
        }

        /// <summary>
        /// Will add given <paramref name="columns"/> to the operation
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Entity Expand(ColumnSet columns)
        {
            container.Logger.StartSection($"Slim: GetRelated {name} from {target.LogicalName} {target.ToStringExt()}");
            var result = default(Entity);
            var refname = name;
            var refatt = string.Empty;
            if (refname.Contains("."))
            {
                refatt = refname.Substring(refname.IndexOf('.') + 1);
                refname = refname.Substring(0, refname.IndexOf('.'));
            }
            if (target.Attributes.Contains(refname))
            {
                var reference = default(EntityReference);
                if (target.Attributes[refname] is EntityReference)
                {
                    reference = (EntityReference)target.Attributes[refname];
                }
                else if (target.Attributes[refname] is Guid && refname.EndsWith("id"))
                {
                    reference = new EntityReference(string.Empty, (Guid)target.Attributes[refname]);
                }
                if (string.IsNullOrEmpty(reference.LogicalName))
                {
                    reference.LogicalName = CintDynEntity.GetRelatedEntityNameFromLookupAttributeName(refname);
                }
                if (reference != null)
                {
                    if (refatt != string.Empty)
                    {
                        var nextref = refatt;
                        if (nextref.Contains("."))
                        {
                            nextref = nextref.Substring(0, nextref.IndexOf('.'));
                        }

                        container.Logger.Log($"Loading {reference.LogicalName} {reference.Id} column {nextref}");
                        var cdNextRelated = container.Retrieve(reference, new ColumnSet(new string[] { nextref }));
                        if (cdNextRelated != null)
                        {
                            result = container
                                .Attribute(refatt)
                                .On(cdNextRelated)
                                .Expand(columns);
                        }
                    }
                    else
                    {
                        result = container.Retrieve(reference, columns);
                    }
                }
            }
            else
            {
                container.Logger.Log($"Record does not contain attribute {refname}");
            }
            if (result == null)
            {
                container.Logger.Log("Could not load related record");
            }
            else
            {
                container.Logger.Log($"Loaded related {result.LogicalName} {result.ToStringExt()}");
            }

            container.Logger.EndSection();
            return result;
        }

        /// <summary>
        /// Will add given <paramref name="columns"/> to the operation
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Entity Expand(params string[] columns) =>
            Expand(new ColumnSet(columns));

        /// <summary>
        /// Will perform the operation with all columns available
        /// </summary>
        /// <returns></returns>
        public Entity ExpandAll() =>
            Expand(new ColumnSet(true));

        /// <summary>
        /// Gets a readable string representation of given attribute
        /// </summary>
        /// <returns>Formatted value of the attribute</returns>
        public override string ToString()
        {
            container.Logger.StartSection("ToString");

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
                container.Logger.EndSection();
            }
        }

        /// <summary>
        /// Gets a readable string representation of given attribute using  <paramref name="format"/>
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Formatted value of the attribute</returns>
        public string ToString(string format)
        {
            container.Logger.StartSection($"ToString with format '{format}'");

            try
            {
                var hasValueFormat = false;

                if (!target.Contains(name))
                {
                    container.Reload(target, name);

                    if (!target.Contains(name))
                    {
                        container.Logger.Log($"'{name}' not found in entity {target.LogicalName}:{target.Id}");
                        container.Logger.Log($"It is impossible to perform formatting — returning empty string instead");

                        return string.Empty;
                    }
                }

                // Extrahera eventuella egna implementerade formatsträngar, t.ex. "<MaxLen=20>"
                var extraFormats = new List<string>();
                format = PPH_Utils.ExtractExtraFormatTags(format, extraFormats);

                string result = null;
                var oAttrValue = target.Contains(name) ? target[name] : null;

                if (oAttrValue != null && format?.StartsWith("<value>") == true)
                {
                    hasValueFormat = true;
                    format = format.Replace("<value>", string.Empty);

                    oAttrValue = CintEntity.AttributeToBaseType(oAttrValue);
                }

                if (oAttrValue != null && !string.IsNullOrWhiteSpace(format))
                {
                    if (oAttrValue is AliasedValue)
                    {
                        oAttrValue = CintEntity.AttributeToBaseType(((AliasedValue)oAttrValue).Value);
                    }

                    if (oAttrValue is Money)
                    {
                        var dAttrValue = ((Money)oAttrValue).Value;
                        result = dAttrValue.ToString(format);
                    }
                    else if (oAttrValue is int)
                    {
                        result = ((int)oAttrValue).ToString(format);
                    }
                    else if (oAttrValue is decimal)
                    {
                        result = ((decimal)oAttrValue).ToString(format);
                    }
                }

                container.Logger.Log($"Resulting value is '{result}'");

                if (result == null)
                {
                    if (oAttrValue != null && oAttrValue is EntityReference)
                    {
                        container.Logger.Log($"Attribute is of 'EntityReference' type it needed to be treated differently.");

                        // Introducerat för att nyttja metadata- och entitetscache på CrmServiceProxy
                        var related = container
                            .Attribute(name)
                            .On(target)
                            .Expand(container.Entity((oAttrValue as EntityReference).LogicalName).PrimaryNameAttribute);

                        result = container.Entity(related).ToString();
                    }
                    else if (hasValueFormat)
                    {
                        result = oAttrValue.ToString();
                    }
                    else
                    {
                        result = container.Attribute(name).On(target).ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(format))
                    {
                        if (DateTime.TryParse(result, out var tmpDateTime))
                        {
                            result = tmpDateTime.ToString(format);
                        }
                        else if (int.TryParse(result, out var tmpInt))
                        {
                            result = tmpInt.ToString(format);
                        }
                        else if (decimal.TryParse(result.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var tmpDecimal))
                        {
                            result = tmpDecimal.ToString(format);
                        }
                        else
                        {
                            if (!format.Contains("{0}"))
                            {
                                format = "{0:" + format + "}";
                            }
                            result = string.Format(format, result);
                        }
                    }
                }
                // Applicera eventuella egna implementerade formatsträngar
                foreach (var extraFormat in extraFormats)
                {
                    result = PPH_Utils.FormatByTag(result, extraFormat);
                }
                return result;
            }
            finally
            {
                container.Logger.EndSection();
            }
        }

        private string ToString(string attributeName, object attributeValue)
        {
            if (attributeValue is AliasedValue)
            {
                container.Logger.Log("Attribute is of type `AliasedValue`");

                return ToString(((AliasedValue)attributeValue).AttributeLogicalName, ((AliasedValue)attributeValue).Value);
            }
            else if (attributeValue is EntityReference reference)
            {
                container.Logger.Log("Attribute is of type `EntityReference`");

                if (!string.IsNullOrEmpty(reference.Name))
                {
                    container.Logger.Log("Reference name was given");
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
                container.Logger.Log("Attribute is of type `EntityCollection`");

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
                container.Logger.Log("Attribute is of type `OptionSetValue`");

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

                    return "";  // OptionSet value not found!
                }
            }
            else if (attributeValue is DateTime)
            {
                container.Logger.Log("Attribute is of type `DateTime`");

                return ((DateTime)attributeValue).ToString("G");
            }
            else if (attributeValue is Money)
            {
                container.Logger.Log("Attribute is of type `Money`");

                return ((Money)attributeValue).Value.ToString("C");
            }

            if (attributeValue != null)
            {
                container.Logger.Log("Attribute will be automatically converted to string");

                return attributeValue.ToString();
            }
            else
            {
                container.Logger.Log("Attribute will be evaluated to null");

                return null;
            }
        }
    }
}