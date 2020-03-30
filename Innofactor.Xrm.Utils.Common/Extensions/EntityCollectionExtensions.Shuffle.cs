namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;
    
    /// <summary>
    /// This class contains methods related to Innofactor Shuffle 
    /// </summary>
    public static partial class EntityCollectionExtensions
    {
        /// <summary>Serialize collection, using specified formatting</summary>
        /// <param name="collection"></param>
        /// <param name="container"></param>
        /// <param name="style">Requested serialization style</param>
        /// <returns></returns>
        public static XmlDocument Serialize(this EntityCollection collection, IExecutionContainer container, SerializationStyle style)
        {
            var result = new XmlDocument();
            switch (style)
            {
                case SerializationStyle.Full:
                    var serializer = new DataContractSerializer(typeof(List<Entity>), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                    var sw = new StringWriter();
                    var xw = new XmlTextWriter(sw);
                    serializer.WriteObject(xw, new List<Entity>(collection.Entities));
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
                    collection.Entities.ToList<Entity>().ForEach(e => e.Serialize(container, style, root));
                    result.AppendChild(root);
                    break;
            }
            return result;
        }
    }
}