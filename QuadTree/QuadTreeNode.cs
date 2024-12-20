using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CCLBStudio.SpatialPartitioning.QuadTree
{
    public class QuadTreeNode
    {
        private QuadTreeRect _bounds;
        private readonly int _maxCapacity;
        private QuadTreeNode[] _children;
        private bool _isDivided = false;
        private Func<Vector3, Vector2> _getCoordinates;
        private readonly SpatialAxis _axis;
        private readonly Vector3[] _corners;
        private readonly HashSet<IQuadTreeEntity> _elements;
        private readonly Dictionary<IQuadTreeEntity, QuadTreeNode> _entityNodeMap;

        public QuadTreeNode(QuadTreeRect bounds, int maxCapacity, SpatialAxis axis, Dictionary<IQuadTreeEntity, QuadTreeNode> entityNodeMap)
        {
            _axis = axis;
            _bounds = bounds;
            _maxCapacity = maxCapacity;
            _entityNodeMap = entityNodeMap;
            _elements = new HashSet<IQuadTreeEntity>();
            
            ComputeCoordinatesDelegate(axis);
            
            _corners = new Vector3[4];
            _corners[0] = new Vector3(bounds.Center.x - bounds.Size.x / 2f, 0f, bounds.Center.y - bounds.Size.y / 2f);
            _corners[1] = new Vector3(bounds.Center.x - bounds.Size.x / 2f, 0f, bounds.Center.y + bounds.Size.y / 2f);
            _corners[2] = new Vector3(bounds.Center.x + bounds.Size.x / 2f, 0f, bounds.Center.y + bounds.Size.y / 2f);
            _corners[3] = new Vector3(bounds.Center.x + bounds.Size.x / 2f, 0f, bounds.Center.y - bounds.Size.y / 2f);
        }

        private void ComputeCoordinatesDelegate(SpatialAxis axis)
        {
            switch (axis)
            {
                case SpatialAxis.XZ:
                    _getCoordinates = vector3 => new Vector2(vector3.x, vector3.z);
                    break;
                
                case SpatialAxis.XY:
                    _getCoordinates = vector3 => new Vector2(vector3.x, vector3.y);
                    break;
                
                case SpatialAxis.YZ:
                    _getCoordinates = vector3 => new Vector2(vector3.y, vector3.z);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public bool ContainsEntity(IQuadTreeEntity entity)
        {
            var position = _getCoordinates(entity.GetPosition());
            return _bounds.Contains(position);
        }

        public bool InsertEntity(IQuadTreeEntity entity)
        {
            var position = _getCoordinates(entity.GetPosition());
            
            if (!_bounds.Contains(position))
            {
                return false;
            }

            if (_elements.Count < _maxCapacity && !_isDivided)
            {
                _elements.Add(entity);
                _entityNodeMap[entity] = this;
                return true;
            }

            if (!_isDivided)
            {
                Subdivide();
                
                foreach (var e in _elements)
                {
                    foreach (var child in _children)
                    {
                        if (child.InsertEntity(e))
                        {
                            break;
                        }
                    }
                }
                
                //_elements.Clear();
            }
            
            foreach (var child in _children)
            {
                if (child.InsertEntity(entity))
                {
                    return true;
                }
            }

            //Debug.LogError("This should never happen !");
            return false;
        }

        public bool RemoveEntity(IQuadTreeEntity entity)
        {
            if (_elements.Remove(entity))
            {
                _entityNodeMap.Remove(entity);

                if (_isDivided)
                {
                    foreach (var child in _children)
                    {
                        child.RemoveEntity(entity);
                    }
                }
                
                CheckForMerge();
                return true;
            }

            if (!_isDivided)
            {
                return false;
            }
            
            foreach (var child in _children)
            {
                if (child.RemoveEntity(entity))
                {
                    CheckForMerge();
                    return true;
                }
            }

            return false;
        }

        private void CheckForMerge()
        {
            if (!_isDivided)
            {
                return;
            }
            
            int totalEntities = _elements.Count;
            
            foreach (var child in _children)
            {
                totalEntities += child._elements.Count;
            }

            if (totalEntities > _maxCapacity)
            {
                return;
            }
            
            foreach (var child in _children)
            {
                foreach (var entity in child._elements)
                {
                    _elements.Add(entity);
                    _entityNodeMap[entity] = this;
                }
            }

            _children = null;
            _isDivided = false;
        }

        private void Subdivide()
        {
            Vector2 newSize = _bounds.Size / 2;
            Vector2 center = _bounds.Center;

            _children = new QuadTreeNode[4];
            _children[0] = new QuadTreeNode(new QuadTreeRect(center + new Vector2(-newSize.x / 2, newSize.y / 2), newSize), _maxCapacity, _axis, _entityNodeMap);
            _children[1] = new QuadTreeNode(new QuadTreeRect(center + new Vector2(newSize.x / 2, newSize.y / 2), newSize), _maxCapacity, _axis, _entityNodeMap);
            _children[2] = new QuadTreeNode(new QuadTreeRect(center + new Vector2(-newSize.x / 2, -newSize.y / 2), newSize), _maxCapacity, _axis, _entityNodeMap);
            _children[3] = new QuadTreeNode(new QuadTreeRect(center + new Vector2(newSize.x / 2, -newSize.y / 2), newSize), _maxCapacity, _axis, _entityNodeMap);

            _isDivided = true;
        }
        
        public QuadTreeNode FindNode(IQuadTreeEntity entity)
        {
            if (_elements.Contains(entity))
            {
                return this;
            }

            if (!_isDivided)
            {
                return null;
            }
            
            foreach (var child in _children)
            {
                var found = child.FindNode(entity);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public List<IQuadTreeEntity> Query(QuadTreeRect range, List<IQuadTreeEntity> found = null)
        {
            found ??= new List<IQuadTreeEntity>();

            if (!_bounds.Intersects(range))
            {
                return found;
            }

            foreach (var entity in _elements)
            {
                if (range.Contains(_getCoordinates(entity.GetPosition())))
                {
                    found.Add(entity);
                }
            }

            if (_isDivided)
            {
                foreach (var child in _children)
                {
                    child.Query(range, found);
                }
            }

            return found;
        }

        public List<IQuadTreeEntity> Query(Vector2 center, float range, List<IQuadTreeEntity> found = null)
        {
            QuadTreeRect circularRange = new QuadTreeRect(center, new Vector2(range * 2, range * 2));
            return Query(circularRange, found);
        }

        public void DebugDraw()
        {
            if (_isDivided)
            {
                foreach (var child in _children)
                {
                    child.DebugDraw();
                }

                return;
            }
            
            Gizmos.DrawLine(_corners[0], _corners[1]);
            Gizmos.DrawLine(_corners[1], _corners[2]);
            Gizmos.DrawLine(_corners[2], _corners[3]);
            Gizmos.DrawLine(_corners[3], _corners[0]);


        }
    }
}