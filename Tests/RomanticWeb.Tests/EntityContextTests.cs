﻿using System;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Moq;
using NUnit.Framework;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Model;
using RomanticWeb.Ontologies;
using RomanticWeb.TestEntities;
using RomanticWeb.Tests.Stubs;

namespace RomanticWeb.Tests
{
    [TestFixture]
    public class EntityContextTests
    {
        private IEntityContext _entityContext;
        private IOntologyProvider _ontologyProvider;
        private Mock<IMappingsRepository> _mappings;
        private Mock<IEntityStore> _entityStore;
        private Mock<IEntitySource> _store;
        private Mock<IEntityContextFactory> _factory;

        private IEnumerable<Lazy<IEntity>> TypedAndUntypedEntities
        {
            get
            {
                Setup();

                yield return new Lazy<IEntity>(() =>
                    {
                        _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
                        return _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"));
                    });
                yield return new Lazy<IEntity>(
                    () =>
                    {
                        _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
                        _mappings.Setup(m => m.MappingFor<IPerson>()).Returns(new EntityMapping());
                        return _entityContext.Load<IPerson>(new EntityId("http://magi/people/Tomasz"));
                    });
            }
        }

        [SetUp]
        public void Setup()
        {
            _factory=new Mock<IEntityContextFactory>();
            _factory.Setup(f => f.SatisfyImports(It.IsAny<EntityProxy>()));
            _ontologyProvider = new TestOntologyProvider();
            _mappings = new Mock<IMappingsRepository>(MockBehavior.Strict);
            _entityStore = new Mock<IEntityStore>(MockBehavior.Strict);
            _store = new Mock<IEntitySource>();
            var mappingContext=new MappingContext(_ontologyProvider,new Mock<IGraphSelectionStrategy>().Object);
            _entityContext=new EntityContext(_factory.Object,_mappings.Object,mappingContext,_entityStore.Object,_store.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _mappings.VerifyAll();
            _entityStore.VerifyAll();
            _store.VerifyAll();
        }

        [Test]
        [TestCaseSource("TypedAndUntypedEntities")]
        public void Creating_new_Entity_should_create_an_instance_with_id(Lazy<IEntity> lazyEntity)
        {
            // when
            dynamic entity = lazyEntity.Value;

            // when
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity, Is.InstanceOf<IEntity>());
            Assert.That(entity.Id, Is.EqualTo(new EntityId("http://magi/people/Tomasz")));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Creating_new_Entity_should_throw_when_EntityId_is_null()
        {
            _entityContext.Load<IEntity>((EntityId)null);
        }

        [Test]
        [TestCaseSource("TypedAndUntypedEntities")]
        public void Creating_new_Entity_should_add_getters_for_known_ontology_namespaces(Lazy<IEntity> lazyEntity)
        {
            // when
            dynamic entity = lazyEntity.Value.AsDynamic();

            // then
            Assert.That(entity.foaf, Is.Not.Null);
            Assert.That(entity.foaf, Is.InstanceOf<OntologyAccessor>());
        }

        [Test]
        [TestCaseSource("TypedAndUntypedEntities")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Creating_new_Entity_should_not_add_getters_for_any_other_ontology_namespaces(Lazy<IEntity> lazyEntity)
        {
            // given
            dynamic entity = lazyEntity.Value.AsDynamic();

            // when
            var accessor = entity.dcterms;
        }

        [Test]
        public void Created_typed_entity_should_be_equal_to_Entity_with_same_Id()
        {
            // given
            _mappings.Setup(m => m.MappingFor<IPerson>()).Returns(new EntityMapping());
            _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
            var entity = _entityContext.Load<IPerson>(new EntityId("http://magi/people/Tomasz"));
            var typed = new Entity(new EntityId("http://magi/people/Tomasz"));

            // then
            Assert.AreEqual(entity, typed);
            Assert.AreEqual(entity.GetHashCode(), typed.GetHashCode());
            _mappings.Verify(m => m.MappingFor<IPerson>(), Times.Once);
        }

        [Test]
        [TestCaseSource("TypedAndUntypedEntities")]
        public void Accessing_entity_id_should_not_trigger_lazy_load(Lazy<IEntity> lazyEntity)
        {
            // given
            IEntity entity = lazyEntity.Value;
            dynamic dynEntity = entity.AsDynamic();

            // when
            var id = entity.Id;
            id = dynEntity.Id;

            // then
            _store.Verify(s => s.LoadEntity(It.IsAny<IEntityStore>(), It.IsAny<EntityId>()), Times.Never);
        }

        [Test]
        public void Accessing_entity_member_for_the_first_time_should_trigger_lazy_load()
        {
            // when
            _entityStore.Setup(s => s.GetObjectsForPredicate(It.IsAny<EntityId>(), It.IsAny<Uri>(), It.IsAny<Uri>()))
                        .Returns(new Node[0]);
            _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
            dynamic entity = _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"));

            // when
            var id = entity.foaf.givenName;

            // then
            _store.Verify(s => s.LoadEntity(It.IsAny<IEntityStore>(), new EntityId("http://magi/people/Tomasz")), Times.Once);
        }

        [Test]
        public void Accessing_typed_entity_member_for_the_first_time_should_trigger_lazy_load()
        {
            // when
            var mockingMapping = new Mock<IEntityMapping>();
            mockingMapping.Setup(m => m.PropertyFor(It.IsAny<string>()))
                          .Returns((string prop) => GetMapping(prop));
            _mappings.Setup(m => m.MappingFor<IPerson>()).Returns(mockingMapping.Object);
            _entityStore.Setup(s => s.GetObjectsForPredicate(It.IsAny<EntityId>(), It.IsAny<Uri>(), It.IsAny<Uri>()))
                        .Returns(new Node[0]);
            _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
            var entity = _entityContext.Load<IPerson>(new EntityId("http://magi/people/Tomasz"));

            // when
            var name = entity.FirstName;
            var page = entity.Homepage;
            var interests = entity.Interests;

            // then
            _store.Verify(s => s.LoadEntity(It.IsAny<IEntityStore>(), new EntityId("http://magi/people/Tomasz")), Times.Once);
        }

        [Test]
        public void Loading_entity_twice_should_initialize_only_once()
        {
            // given
            _entityStore.Setup(s => s.GetObjectsForPredicate(It.IsAny<EntityId>(), It.IsAny<Uri>(), It.IsAny<Uri>()))
                        .Returns(new Node[0]);
            _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(true);
            var entity = _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"));
            var name=entity.AsDynamic().foaf.givenName;
            entity = _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"));

            // when
            var page = entity.AsDynamic().foaf.homePage;

            // then
            _store.Verify(s => s.LoadEntity(It.IsAny<IEntityStore>(), new EntityId("http://magi/people/Tomasz")), Times.Once);
        }

        [Test]
        public void New_entity_should_not_trigger_initialization()
        {
            // given
            _entityStore.Setup(s => s.GetObjectsForPredicate(It.IsAny<EntityId>(), It.IsAny<Uri>(), It.IsAny<Uri>()))
                        .Returns(new Node[0]);
            var entity = _entityContext.Create<IEntity>(new EntityId("http://magi/people/Tomasz"));

            // when
            var page = entity.AsDynamic().foaf.homePage;

            // then
            Assert.That(page,Is.Empty);
            _store.Verify(s => s.LoadEntity(It.IsAny<IEntityStore>(), It.IsAny<EntityId>()), Times.Never);
        }

        [Test]
        public void Load_should_return_null_if_entity_doesnt_exist()
        {
            // given
            _store.Setup(s => s.EntityExist(new EntityId("http://magi/people/Tomasz"))).Returns(false);

            // when
            var entity = _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"));

            // then
            Assert.That(entity,Is.Null);
        }

        [Test]
        public void Should_be_possible_to_load_entity_without_checking_for_existence()
        {
            // when
            var entity = _entityContext.Load<IEntity>(new EntityId("http://magi/people/Tomasz"), false);

            // then
            Assert.That(entity,Is.Not.Null);
            _store.Verify(s => s.EntityExist(It.IsAny<EntityId>()),Times.Never);
        }

        [Test]
        public void Should_apply_changes_to_underlying_store_when_committing()
        {
            // given
            var aChangeset=new DatasetChanges();
            _entityStore.Setup(store => store.Changes).Returns(aChangeset);
            
            // when
            _entityContext.Commit();

            // then
            _store.Verify(store=>store.ApplyChanges(aChangeset), Times.Once);
        }

        [Test]
        public void Deleting_entity_should_mark_for_deletion_in_changeset()
        {
            // given
            var entityId = new EntityId("urn:some:entityid");
            _entityStore.Setup(store => store.Delete(entityId));

            // when
            _entityContext.Delete(entityId);

            // then
            _entityStore.Verify(store => store.Delete(entityId), Times.Once);
        }

        private static IPropertyMapping GetMapping(string propertyName)
        {
            return new TestPropertyMapping
                       {
                           Name=propertyName,
                           Uri=new Uri("http://unittest/"+propertyName),
                           IsCollection=false,
                           GraphSelector = new DefaultGraphSelector()
                       };
        }
    }
}
