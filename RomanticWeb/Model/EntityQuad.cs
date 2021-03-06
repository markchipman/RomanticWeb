﻿using System;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    /// <summary>Represents a triple (subject, predicate, object).</summary>
    public sealed class EntityQuad : Triple, IEntityQuad
    {
        private readonly int _hashCode;
        private readonly INode _graph;
        private readonly EntityId _entityId;

        /// <summary>Creates a new instance of <see cref="EntityQuad"/> from given <see cref="ITriple"/>.</summary>
        public EntityQuad(EntityId entityId, ITriple triple) : this(entityId, triple.Subject, triple.Predicate, triple.Object)
        {
        }

        /// <summary>Creates a new instance of <see cref="EntityQuad"/> in named graph.</summary>
        public EntityQuad(EntityId entityId, INode s, INode p, INode o, INode graph) : this(entityId, s, p, o)
        {
            if ((graph != null) && (!graph.IsUri) && (!graph.IsBlank))
            {
                throw new ArgumentOutOfRangeException("graph", "Graph must not be a literal.");
            }

            if (graph != null)
            {
                _hashCode ^= (_graph = graph).GetHashCode();
            }
        }

        /// <summary>Creates a new instance of <see cref="EntityQuad"/> in default graph.</summary>
        public EntityQuad(EntityId entityId, INode s, INode p, INode o) : base(s, p, o)
        {
            _entityId = entityId;
            _hashCode = ComputeHashCode();
        }

        /// <summary>Gets the named graph node or null, if triple is in named graph.</summary>
        public INode Graph { get { return _graph; } }

        /// <summary>Gets entity id, which defines this triple.</summary>
        public EntityId EntityId { get { return _entityId; } }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="o">Object <see cref="Uri" />.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, Uri o)
        {
            return new EntityQuad(entityId, Node.ForUri(s), Node.ForUri(p), Node.ForUri(o));
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, object value)
        {
            return new EntityQuad(entityId, Node.ForUri(s), Node.ForUri(p), Node.ForLiteral(value.ToString()));
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, string value)
        {
            return For(entityId, s, p, value, Vocabularies.Xsd.String);
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, int value)
        {
            return For(entityId, s, p, value.ToString(), Vocabularies.Xsd.Int);
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, float value)
        {
            return For(entityId, s, p, value.ToString("R", System.Globalization.CultureInfo.InvariantCulture), Vocabularies.Xsd.Float);
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, bool value)
        {
            return For(entityId, s, p, value.ToString().ToLower(), Vocabularies.Xsd.Boolean);
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <param name="dataType">Optional Uri of the datatype of the value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, string value, Uri dataType)
        {
            return new EntityQuad(entityId, Node.ForUri(s), Node.ForUri(p), Node.ForLiteral(value, dataType ?? Vocabularies.Xsd.String));
        }

        /// <summary>Creates a quad.</summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="s">Sbject <see cref="Uri" />.</param>
        /// <param name="p">Predicate <see cref="Uri" />.</param>
        /// <param name="value">Value.</param>
        /// <param name="language">Language of the value.</param>
        /// <returns><see cref="EntityQuad" /> created.</returns>
        public static EntityQuad For(EntityId entityId, Uri s, Uri p, string value, string language)
        {
            return new EntityQuad(entityId, Node.ForUri(s), Node.ForUri(p), Node.ForLiteral(value, language));
        }

#pragma warning disable 1591
        public static bool operator ==(EntityQuad left, EntityQuad right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityQuad left, EntityQuad right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj is EntityQuad) && (Equals((EntityQuad)obj));
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", Subject, Predicate, Object, Graph);
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <param name="nQuadFormat">if set to <c>true</c> the string will be a valid NQuad.</param>
        public string ToString(bool nQuadFormat)
        {
            if (!nQuadFormat)
            {
                return ToString();
            }

            return String.Format("{0} {1} {2} {3} . ", Subject.ToString(true), Predicate.ToString(true), Object.ToString(true), Graph.ToString(true));
        }

        int IComparable<IEntityQuad>.CompareTo(IEntityQuad other)
        {
            return ((IComparable)this).CompareTo(other);
        }

        int IComparable.CompareTo(object other)
        {
            return FluentCompare<EntityQuad>
                .Arguments(this, other)
                .By(t => t.EntityId)
                .By(t => t.Graph)
                .By(t => t.Subject)
                .By(t => t.Predicate)
                .By(t => t.Object)
                .End();
        }
#pragma warning restore

        internal EntityQuad InGraph(Uri graphUri)
        {
            if (graphUri != null)
            {
                return new EntityQuad(EntityId, Subject, Predicate, Object, Node.ForUri(graphUri));
            }

            return this;
        }

        private bool Equals(EntityQuad other)
        {
            return (base.Equals(other)) && (Equals(_graph, other._graph)) && (_entityId.Equals(other._entityId));
        }

        private int ComputeHashCode()
        {
            unchecked
            {
                var result = base.GetHashCode();
                result = (result * 397) ^ _entityId.GetHashCode();
                if (_graph != null)
                {
                    result ^= _graph.GetHashCode();
                }

                return result;
            }
        }
    }
}