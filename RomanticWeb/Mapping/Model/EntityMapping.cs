﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RomanticWeb.Mapping.Model
{
    [DebuggerDisplay("Entity {EntityType}")]
    [DebuggerTypeProxy(typeof(DebuggerViewProxy))]
    internal class EntityMapping : IEntityMapping
    {
        private readonly Type _entityType;
        private readonly List<ClassMapping> _classes;
        private readonly List<PropertyMapping> _properties;

        public EntityMapping(Type entityType,IEnumerable<ClassMapping> classes,IEnumerable<PropertyMapping> properties)
        {
            _entityType=entityType;
            _properties=properties.ToList();
            _classes=classes.ToList();

            _properties.ForEach(p => p.EntityMapping=this);
        }

        internal EntityMapping(Type entityType):this(entityType,new ClassMapping[0],new PropertyMapping[0])
        {
        }

        public Type EntityType
        {
            get
            {
                return _entityType;
            }
        }

        public IEnumerable<IClassMapping> Classes { get { return _classes; } }

        public IEnumerable<IPropertyMapping> Properties { get { return _properties; } }

        public IPropertyMapping PropertyFor(string propertyName)
        {
            var propertyMapping=Properties.SingleOrDefault(p => p.Name==propertyName);

            if (propertyMapping==null)
            {
                throw new MappingException(string.Format("No mapping found for property {0}",propertyName));
            }

            return propertyMapping;
        }

        private class DebuggerViewProxy
        {
            private readonly EntityMapping _mapping;

            public DebuggerViewProxy(EntityMapping mapping)
            {
                _mapping=mapping;
            }

            public Type EntityType
            {
                get
                {
                    return _mapping.EntityType;
                }
            }

            public IList<IClassMapping> Classes
            {
                get
                {
                    return _mapping.Classes.ToList();
                }
            }

            public IList<IPropertyMapping> Properties
            {
                get
                {
                    var propertyMappings=_mapping.Properties.ToList();
                    propertyMappings.Sort(CompareProperty);
                    return propertyMappings;
                }
            }

            private static int CompareProperty(IPropertyMapping left,IPropertyMapping right)
            {
                if (left.GetType()==right.GetType())
                {
                    return string.Compare(left.Name,right.Name,StringComparison.CurrentCulture);
                }

                if (left is CollectionMapping)
                {
                    return 1;
                }

                return -1;
            }
        }
    }
}