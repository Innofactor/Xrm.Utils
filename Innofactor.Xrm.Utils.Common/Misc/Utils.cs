namespace Innofactor.Xrm.Utils.Common.Misc
{
    /// <summary>
    /// Generic utilities not associated with any other specific area
    /// </summary>
    public static class Utils
    {
        /// <summary>Returns last part of the class name of the given object</summary>
        /// <example>
        /// System.String returns String
        /// Microsoft.Xrm.Sdk.OptionSetValue returns OptionSetValue
        /// </example>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string LastClassName(object obj)
        {
            var result = obj == null ? "null" : obj.GetType().ToString();
            result = result.Split('.')[result.Split('.').Length - 1];
            return result;
        }
    }
    /// <summary>Type of serialization</summary>
    public enum SerializationStyle
    {
        /// <summary>Serialized EntityCollection</summary>
        Full = 1,

        /// <summary>Innofactor proprietary XML serialization</summary>
        Simple = 2,

        /// <summary>Same as Simple but including clear text representation of EntityReference and OptionSetValue values</summary>
        SimpleWithValue = 3,

        /// <summary>Same as SimpleWithValue but without actual ID of EntityReference calues, used for ID independent comparison</summary>
        SimpleNoId = 4,

        /// <summary>XML tags get actual entity names and attribute names, instead of names in name property</summary>
        Explicit = 5,

        /// <summary>Text file generation from Entities</summary>
        Text = 11
    }
}