namespace Innofactor.Xrm.Utils.Common.Misc
{
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>A class used to contain the sorting attributes for the comparer class.</summary>
    public class SortAttribute
    {
        /// <summary>Property returning the sort attribute.</summary>
        /// <returns>The string that is the sort attribute.</returns>
        public string Attribute;

        /// <summary>Property returning the sorting type.</summary>
        /// <returns>The OrderType value.</returns>
        public OrderType Type;

        /// <summary>Constructor of SortAttribute class. Takes an attribute name and an OrderType as arguments.</summary>
        /// <param name="attribute">The sort attribute</param>
        /// <param name="type">The sort attribute sorting type (OrderType)</param>
        public SortAttribute(string attribute, OrderType type)
        {
            Attribute = attribute;
            Type = type;
        }
    }
}