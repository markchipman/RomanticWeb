﻿using System;

namespace RomanticWeb.Mapping.Model
{
    /// <summary>
    /// A RDF type mapping fr an Entity
    /// </summary>
    public interface IClassMapping
	{
        /// <summary>
        /// Gets the Entity's RDF class URI
        /// </summary>
		Uri Uri { get; }
	}
}