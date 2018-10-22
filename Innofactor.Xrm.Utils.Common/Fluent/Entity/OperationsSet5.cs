namespace Innofactor.Xrm.Utils.Common.Fluent.Entity
{
    using System.Collections.Generic;
    using System.Linq;
    using Innofactor.Xrm.Utils.Common.Extensions;
    using Innofactor.Xrm.Utils.Common.Interfaces;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

    public class OperationsSet5 : InformationBase
    {
        #region Private Fields

        private readonly string intersectionName;
        private readonly Entity target;

        #endregion Private Fields

        #region Internal Constructors

        internal OperationsSet5(IExecutionContainer container, Entity target, string intersectionName)
            : base(container)
        {
            this.target = target;
            this.intersectionName = intersectionName;
        }

        #endregion Internal Constructors

        #region Public Methods

        public bool LinkTo(Entity related) =>
            LinkTo(new EntityReference[] { related.ToEntityReference() });

        public bool LinkTo(EntityCollection related) =>
            LinkTo(related.Entities.Select(x => x.ToEntityReference()));

        public bool LinkTo(IEnumerable<Entity> related) =>
            LinkTo(related.Select(x => x.ToEntityReference()));

        public bool LinkTo(EntityReference related) =>
            LinkTo(new EntityReference[] { related });

        public bool LinkTo(EntityReferenceCollection related) =>
            LinkTo(related.ToArray());

        public bool LinkTo(IEnumerable<EntityReference> related)
        {
            try
            {
                var batchSize = (related.Count() >= 1000)
                    ? 1000
                    : related.Count();

                var role = default(EntityRole?);
                if (related.Count() > 0 && related.First().LogicalName == target.LogicalName)
                {
                    // N:N-relation till samma entitet, då måste man ange en roll, tydligen.
                    role = EntityRole.Referencing;
                }

                var processed = 0;
                while (processed < related.Count())
                {
                    var batch = new EntityReferenceCollection(related.Skip(processed).Take(batchSize).ToList());
                    processed += batch.Count();

                    var req = new AssociateRequest
                    {
                        Target = target.ToEntityReference(),
                        Relationship = new Relationship(intersectionName)
                        {
                            PrimaryEntityRole = role
                        },
                        RelatedEntities = batch
                    };
                    container.Service.Execute(req);
                    container.Log("Associated {0} {1} with {2}", batch.Count, related.Count() > 0 ? related.First().LogicalName : string.Empty, target.LogicalName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UnlinkFrom(Entity related) =>
            UnlinkFrom(new EntityReference[] { related.ToEntityReference() });

        public bool UnlinkFrom(EntityCollection related) =>
            UnlinkFrom(related.Entities.Select(x => x.ToEntityReference()));

        public bool UnlinkFrom(IEnumerable<Entity> related) =>
            UnlinkFrom(related.Select(x => x.ToEntityReference()));

        public bool UnlinkFrom(EntityReference related) =>
            UnlinkFrom(new EntityReference[] { related });

        public bool UnlinkFrom(EntityReferenceCollection related) =>
            UnlinkFrom(related.ToArray());

        public bool UnlinkFrom(IEnumerable<EntityReference> related)
        {
            try
            {
                var batchSize = (related.Count() >= 1000)
                    ? 1000
                    : related.Count();

                var processed = 0;
                while (processed < related.Count())
                {
                    var batch = new EntityReferenceCollection(related.Skip(processed).Take(batchSize).ToList());
                    processed += batch.Count();

                    var req = new DisassociateRequest
                    {
                        Target = target.ToEntityReference(),
                        Relationship = new Relationship(intersectionName),
                        RelatedEntities = batch
                    };
                    container.Service.Execute(req);
                    container.Log("Disassociated {0} {1} from {2}", batch.Count, related.Count() > 0 ? related.First().LogicalName : string.Empty, target.LogicalName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Public Methods
    }
}