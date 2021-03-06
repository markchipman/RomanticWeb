﻿using System;
using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Linq.Model;
using RomanticWeb.Model;
using RomanticWeb.Updates;

namespace RomanticWeb
{
    /// <summary>A source for triples, loaded from physical triple stores.</summary>
    public interface IEntitySource : IDisposable
    {
        /// <summary>Gets or sets the meta graph URI.</summary>
        Uri MetaGraphUri { get; set; }

        /// <summary>Loads an entity into the given <see cref="IEntityStore"/></summary>
        IEnumerable<IEntityQuad> LoadEntity(EntityId entityId);

        /// <summary>Checks if an Entity with a given Id exists</summary>
        bool EntityExist(EntityId entityId);

        /// <summary>Executes a query and returns resulting quads.</summary>
        /// <param name="queryModel">Query model to be executed.</param>
        /// <param name="resultingEntities">Enumeration of entity identifiers beeing in fact the resulting ones.</param>
        /// <returns>Enumeration of entity quads beeing a result of the query.</returns>
        IEnumerable<IEntityQuad> ExecuteEntityQuery(IQuery queryModel, out IEnumerable<EntityId> resultingEntities);

        /// <summary>Executes a query with scalar result.</summary>
        /// <param name="queryModel">Query model to be executed.</param>
        /// <returns>Scalar value beeing a result of the query.</returns>
        int ExecuteScalarQuery(IQuery queryModel);

        /// <summary>Executes an 'ask' query.</summary>
        /// <param name="queryModel">Query model to be executed.</param>
        /// <returns><b>true</b> in case a given query has solution, otherwise <b>false</b>.</returns>
        bool ExecuteAskQuery(IQuery queryModel);

        /// <summary>Gets the underlying command text for given query.</summary>
        /// <param name="queryModel">Query model to be executed.</param>
        /// <returns>String containing a command text</returns>
        string GetCommandText(IQuery queryModel);

        /// <summary>
        /// Applies changes to the underlaying triple store
        /// </summary>
        /// <param name="changes"></param>
        void Commit(IEnumerable<IDatasetChange> changes);
    }
}