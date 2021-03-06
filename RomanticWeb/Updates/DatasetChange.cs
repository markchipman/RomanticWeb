using RomanticWeb.Entities;

namespace RomanticWeb.Updates
{
    /// <summary>Represents a change to the triple store.</summary>
    public abstract class DatasetChange : IDatasetChange
    {
        private readonly EntityId _entity;
        private readonly EntityId _graph;

        /// <summary>Initializes a new instance of the <see cref="DatasetChange"/> class, which affects a single graph.</summary>
        /// <param name="entity">The changed entity.</param>
        /// <param name="graph">The changed graph.</param>
        protected DatasetChange(EntityId entity, EntityId graph) : this(entity)
        {
            _graph = graph;
        }

        /// <summary>Initializes a new instance of the <see cref="DatasetChange"/> class, which affects multiple graphs.</summary>
        /// <param name="entity">The changed entity.</param>
        protected DatasetChange(EntityId entity)
        {
            var blankId = entity as BlankId;
            while (blankId != null)
            {
                entity = blankId.RootEntityId;
                blankId = blankId.RootEntityId as BlankId;
            }

            _entity = entity;
        }

        /// <summary>Gets the entity, which was changed.</summary>
        public EntityId Entity { get { return _entity; } }

        /// <summary>Gets the graph, which was changed.</summary>
        /// <returns>null if change affects multiple graphs</returns>
        public EntityId Graph { get { return _graph; } }

        /// <summary>Gets a value indicating whether this instance actually represents a change to the store.</summary>
        public virtual bool IsEmpty { get { return false; } }

        /// <summary>Determines whether this instance can be merged with another.</summary>
        /// <param name="other">The other change.</param>
        public virtual bool CanMergeWith(IDatasetChange other)
        {
            return Graph == other.Graph;
        }

        /// <summary>Merges this change the with another change.</summary>
        public abstract IDatasetChange MergeWith(IDatasetChange other);
    }
}