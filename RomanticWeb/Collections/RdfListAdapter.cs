using System;
using System.Collections;
using System.Collections.Generic;
using NullGuard;
using RomanticWeb.Entities;

namespace RomanticWeb.Collections
{
    [NullGuard(ValidationFlags.All)]
    internal class RdfListAdapter<T>:IEntity,IList<T>
    {
        private readonly IEntityContext _context;
        private IRdfListNode _head;
        private IRdfListNode _tail;

        public RdfListAdapter(IEntityContext context,IEntity rdfListHead)
            :this(context,rdfListHead.AsEntity<IRdfListNode>())
        {
        }

        public RdfListAdapter(IEntityContext context)
            :this(context,context.Load<IRdfListNode>(Vocabularies.Rdf.nil,false))
        {
        }

        private RdfListAdapter(IEntityContext context,IRdfListNode head)
        {
            Count=0;
            _context=context;
            _head=_tail=head;

            Initialize();
        }
            
        EntityId IEntity.Id
        {
            get
            {
                return _head.Id;
            }
        }

        IEntityContext IEntity.Context
        {
            get
            {
                return _context;
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                var rdfListNode=GetNodeAt(index);

                if (rdfListNode.First==null)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return (T)rdfListNode.First;
            }

            set
            {
                Insert(index,value);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (_head.Id==Vocabularies.Rdf.nil)
            {
                InsertToEmptyList(item);
            }
            else
            {
                var newTail=InsertNodeAfter(_tail,item);
                _tail=newTail;
            }
        }

        public void Clear()
        {
            var currentNode=_head;
            while (currentNode.Id!=Vocabularies.Rdf.nil)
            {
                _context.Delete(currentNode.Id);
                currentNode=currentNode.Rest;
            }

            _head=_tail=_context.Load<IRdfListNode>(Vocabularies.Rdf.nil,false);
        }

        public bool Contains(T item)
        {
            return GetNodeForItemWithPredecessor(item).Item1.Id!=Vocabularies.Rdf.nil;
        }

        public void CopyTo(T[] array,int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(T item)
        {
            var nodeWithPredecessor=GetNodeForItemWithPredecessor(item);

            if (nodeWithPredecessor.Item1.Id==Vocabularies.Rdf.nil)
            {
                return false;
            }

            DeleteNode(nodeWithPredecessor.Item1,nodeWithPredecessor.Item2);
            
            return true;
        }

        public int IndexOf(T item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index,T item)
        {
            if ((index<0)||(index>Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index==0)
            {
                InsertFirstNode(item);
            }
            else if (index==Count)
            {
                Add(item);
            }
            else
            {
                var nodeToReplace=GetNodeAt(index-1);
                InsertNodeAfter(nodeToReplace,item);
            }
        }

        public void RemoveAt(int index)
        {
            if (index==0)
            {
                DeleteNode(_head,null);
            }
            else
            {
                IRdfListNode previousNode=GetNodeAt(index-1);
                DeleteNode(previousNode.Rest, previousNode);
            }
        }

        private void DeleteNode(IRdfListNode nodeToDelete,[AllowNull]IRdfListNode previousNode)
        {
            if (previousNode!=null)
            {
                previousNode.Rest = nodeToDelete.Rest;
                if (nodeToDelete.Rest.Id == Vocabularies.Rdf.nil)
                {
                    _tail = previousNode;
                }
            }
            else
            {
                _head=nodeToDelete.Rest;
            }

            _context.Delete(nodeToDelete.Id);
        }

        private Tuple<IRdfListNode,IRdfListNode> GetNodeForItemWithPredecessor(T item)
        {
            IRdfListNode previousNode=null;
            var currentNode=_head;
            while (currentNode.Id!=Vocabularies.Rdf.nil)
            {
                if (Equals(currentNode.First,item))
                {
                    break;
                }

                previousNode=currentNode;
                currentNode=currentNode.Rest;
            }

            return Tuple.Create(currentNode,previousNode);
        }

        private IRdfListNode GetNodeAt(int index)
        {
            if (index<0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            IRdfListNode currentNode=_head;
            int currentIndex=0;

            while ((currentIndex<index)&&(currentNode.Id!=Vocabularies.Rdf.nil))
            {
                currentNode=currentNode.Rest;
                currentIndex++;
            }

            return currentNode;
        }

        private IRdfListNode InsertNodeAfter(IRdfListNode existingNode,T item)
        {
            var newNode=_context.Create<IRdfListNode>(new BlankId(_context.BlankIdGenerator.Generate()));
            newNode.First=item;
            newNode.Rest = existingNode.Rest;
            existingNode.Rest=newNode;
            Count++;

            return newNode;
        }

        private void InsertToEmptyList(T item)
        {
            var newNode=InsertFirstNode(item);
            _tail=newNode;
        }

        private IRdfListNode InsertFirstNode(T item)
        {
            var newNode=_context.Create<IRdfListNode>(new BlankId(_context.BlankIdGenerator.Generate()));
            newNode.First = item;
            newNode.Rest = _head;
            _head = newNode;
            Count++;

            return newNode;
        }

        private void Initialize()
        {
            foreach (var element in this)
            {
                Count++;
            }
        }

        private class Enumerator:IEnumerator<T>
        {
            private readonly IRdfListNode _firstNode;
            private IRdfListNode _currentNode;

            public Enumerator(IRdfListNode actualEntity)
            {
                _firstNode=actualEntity;
            }

            public T Current
            {
                get
                {
                    if ((_currentNode==null)||(_currentNode.First==null))
                    {
                        return default(T);
                    }

                    return (T)_currentNode.First;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_currentNode==null)
                {
                    _currentNode=_firstNode;
                }
                else
                {
                    _currentNode = _currentNode.Rest;
                }

                if (_currentNode==null || _currentNode.Id == Vocabularies.Rdf.nil)
                {
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                _currentNode=null;
            }
        }
    }
}