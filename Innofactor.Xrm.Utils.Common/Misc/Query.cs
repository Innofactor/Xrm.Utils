namespace Innofactor.Xrm.Utils.Common.Misc
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>Various utils for retrieving and updating entities and attributes</summary>
    public static class Query
    {
        #region Public Methods

        /// <summary>Appends QueryExpression Condition to the FilterExpression</summary>
        /// <param name="filt">FilterExpression to add condition to</param>
        /// <param name="type">Operator for FilterExpression</param>
        /// <param name="attr">Attribut to apply condition to</param>
        /// <param name="oper">Operator for condition</param>
        /// <param name="val">Value to apply operation on</param>
        /// <returns></returns>
        public static ConditionExpression AppendCondition(FilterExpression filt, LogicalOperator type, string attr, ConditionOperator oper, object val)
        {
            filt.FilterOperator = type;
            var cond = new ConditionExpression
            {
                AttributeName = attr,
                Operator = oper
            };
            if (val != null)
            {
                cond.Values.Add(val);
            }
            filt.AddCondition(cond);
            return cond;
        }

        /// <summary>Appends QueryExpression Condition to verify StateCode=Active</summary>
        /// <param name="filt">FilterExpression to add condition to</param>
        /// <returns></returns>
        public static ConditionExpression AppendConditionActive(FilterExpression filt) =>
            AppendCondition(filt, LogicalOperator.And, "statecode", ConditionOperator.Equal, 0);

        /// <summary>Appends QueryExpression linked entity to the LinkEntities list</summary>
        /// <param name="linkentities">LinkEntities collection to add linked entity to</param>
        /// <param name="from">Link From entity</param>
        /// <param name="to">Link To entity</param>
        /// <param name="from_attr">Attribute name on From entity</param>
        /// <param name="to_attr">Attribute name on To entity</param>
        /// <returns></returns>
        public static LinkEntity AppendLink(DataCollection<LinkEntity> linkentities, string from, string to, string from_attr, string to_attr)
        {
            var link = new LinkEntity
            {
                LinkFromEntityName = from,
                LinkFromAttributeName = from_attr,
                LinkToEntityName = to,
                LinkToAttributeName = to_attr
            };
            linkentities.Add(link);
            return link;
        }

        /// <summary>Use to generate link between 1:M entities (for standard identifying field names)</summary>
        /// <param name="linkentities"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static LinkEntity AppendLink1M(DataCollection<LinkEntity> linkentities, string from, string to) =>
            AppendLink(linkentities, from, to, from + "id", from + "_id");

        /// <summary>Use to generate link between M:1 entities (for standard identifying field names)</summary>
        /// <param name="linkentities"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static LinkEntity AppendLinkM1(DataCollection<LinkEntity> linkentities, string from, string to) =>
            AppendLink(linkentities, from, to, to + "_id", to + "id");

        /// <summary>Use to generate link between N:N entities (for standard identifying field names and intersect table name)</summary>
        /// <param name="linkentities"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>LinkEntity for the To entity</returns>
        public static LinkEntity AppendLinkMM(DataCollection<LinkEntity> linkentities, string from, string to) =>
            AppendLinkMM(linkentities, to + "_" + from, from, to);

        /// <summary>Use to generate link between N:N entities (for standard identifying field names but non-standard intersect table name)</summary>
        /// <param name="linkentities"></param>
        /// <param name="intersect"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static LinkEntity AppendLinkMM(DataCollection<LinkEntity> linkentities, string intersect, string from, string to) =>
            AppendLinkMM(linkentities, intersect, from, from + "id", to, to + "id");

        /// <summary>Use to generate link between N:N entities (for non-standard identifying field names and intersect table name)</summary>
        /// <param name="linkentities"></param>
        /// <param name="intersect"></param>
        /// <param name="from"></param>
        /// <param name="from_attr"></param>
        /// <param name="to"></param>
        /// <param name="to_attr"></param>
        /// <returns>LinkEntity for the To entity</returns>
        public static LinkEntity AppendLinkMM(DataCollection<LinkEntity> linkentities, string intersect, string from, string from_attr, string to, string to_attr)
        {
            var NN = AppendLink(linkentities, from, intersect, from_attr, from_attr);
            return AppendLink(NN.LinkEntities, intersect, to, to_attr, to_attr);
        }

        /// <summary>Gets first found condition for attribute with given name</summary>
        /// <param name="filter">FilterExpression in which to look for condition, typically <c>myQuery.Criteria</c></param>
        /// <param name="attribute">Name of attribute on condition to return</param>
        /// <returns>First found <c>ConditionExpression</c>, or null if no condition is found</returns>
        /// <remarks>Iterated conditions on the filter itself, and any sub filter collections.</remarks>
        public static ConditionExpression GetFilterConditionByAttribute(FilterExpression filter, string attribute)
        {
            var result = GetConditionsCondition(filter, attribute);
            if (result == null)
            {
                if (filter == null)
                {
                    return null;
                }

                foreach (var subfilter in filter.Filters)
                {
                    result = GetConditionsCondition(subfilter, attribute);
                    if (result == null)
                    {
                        result = GetFilterConditionByAttribute(subfilter, attribute);
                    }
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        public static QueryExpression QueryExpressionByAttributes(string entity, string[] attributes, object[] values, ColumnSet columns, bool nolock)
        {
            var qx = new QueryExpression(entity);
            if (attributes.Length != values.Length)
            {
                throw new InvalidPluginExecutionException("QueryExpressionByAttributes invalid attribute/value count");
            }
            for (var i = 0; i < attributes.Length; i++)
            {
                qx.Criteria.AddCondition(attributes[i], ConditionOperator.Equal, values[i]);
            }
            qx.ColumnSet = columns;
            qx.NoLock = nolock;
            return qx;
        }

        #endregion Public Methods

        #region Private Methods

        private static ConditionExpression GetConditionsCondition(FilterExpression subfilter, string attribute)
        {
            if (subfilter == null)
            {
                return null;
            }

            foreach (var cond in subfilter.Conditions)
            {
                if (cond.AttributeName == attribute)
                {
                    return cond;
                }
            }

            return null;
        }

        #endregion Private Methods
    }
}