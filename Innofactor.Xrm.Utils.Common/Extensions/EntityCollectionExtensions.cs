namespace Innofactor.Xrm.Utils.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Innofactor.Xrm.Utils.Common.Misc;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Extension methods for EntityCollection class
    /// </summary>
    public static class EntityCollectionExtensions
    {
        #region Public Methods

        /// <summary>
        /// Adds entity to current collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="entity"></param>
        public static void Add(this EntityCollection collection, Entity entity) =>
            collection.Entities.Add(entity);

        /// <summary>
        /// Adds range of entities to current collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="entities"></param>
        public static void AddRange(this EntityCollection collection, EntityCollection entities) =>
            collection.Entities.AddRange(entities.Entities);

        /// <summary>Checks if specified item is available in the collection. Comparison is strictly by Id.</summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool Contains(this EntityCollection collection, Entity item) =>
            Contains(collection, item.Id);

        /// <summary>
        /// Checks if a record with specified id is available in the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Contains(this EntityCollection collection, Guid id) =>
            collection.Entities.Any(x => x.Id == id);

        /// <summary>
        /// Counts number of record in current collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static int Count(this EntityCollection collection) =>
            collection.Entities.Count;

        /// <summary>
        /// Returns record with specified id if it's available in the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Entity Get(this EntityCollection collection, Guid id) =>
            collection.Entities.Where(x => x.Id == id).FirstOrDefault();

        /// <summary>Sort the collection based on given attribute names</summary>
        /// <param name="collection"></param>
        /// <param name="attributes">Array of attribute names to sort by. Prepend attribute name with ! to sort DESC.</param>
        public static void Sort(this EntityCollection collection, params string[] attributes)
        {
            var temp = new List<Entity>(collection.Entities);
            temp.Sort(new EntityComparer(attributes));

            collection = new EntityCollection(temp);
        }

        /// <summary>
        /// Turns collection of <see cref="Entity" /> into collection of <see cref="EntityReference" />
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static EntityReferenceCollection ToEntityReferenceCollection(this EntityCollection collection) =>
            new EntityReferenceCollection(collection.Entities.Select(x => x.ToEntityReference()).ToList());

        #endregion Public Methods
    }
}