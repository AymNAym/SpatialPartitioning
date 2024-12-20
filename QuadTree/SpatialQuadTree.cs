using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.SpatialPartitioning.QuadTree
{
    [CreateAssetMenu(fileName = "NewSpatialQuadTree", menuName = "CCLB Studio/Spatial Partitioning/Spatial Quad Tree")]
    public class SpatialQuadTree : ScriptableObject
    {
        [Tooltip("The quad tree axis, determining how the spatial checks will be performed.")]
        [SerializeField] private SpatialAxis axis = SpatialAxis.XZ;
        [SerializeField] private Vector2 initialSize = new Vector2(100f, 100f);
        [Tooltip("Maximum entities inside a quad before performing a subdivision.")]
        [Min(2)][SerializeField] private int maxNodeCapacity = 4;
        
        private QuadTreeNode _rootNode;
        private Dictionary<IQuadTreeEntity, QuadTreeNode> _entityNodeMap;

        public void Initialize(Vector3 center)
        {
            Vector2 quadCenter = axis switch
            {
                SpatialAxis.XZ => new Vector2(center.x, center.z),
                SpatialAxis.XY => new Vector2(center.x, center.y),
                SpatialAxis.YZ => new Vector2(center.y, center.z),
            };

            _entityNodeMap = new Dictionary<IQuadTreeEntity, QuadTreeNode>();
            _rootNode = new QuadTreeNode(new QuadTreeRect(quadCenter, initialSize), maxNodeCapacity, axis, _entityNodeMap);
        }

        public void InsertEntity(IQuadTreeEntity entity)
        {
            _rootNode?.InsertEntity(entity);
        }

        public bool RemoveEntity(IQuadTreeEntity entity)
        {
            return _rootNode?.RemoveEntity(entity) ?? false;
        }
        
        public void UpdateEntity(IQuadTreeEntity entity)
        {
            if (_entityNodeMap.TryGetValue(entity, out QuadTreeNode currentNode))
            {
                if (!currentNode.ContainsEntity(entity))
                {
                    currentNode.RemoveEntity(entity);
                    InsertEntity(entity);
                }
            }
            else
            {
                InsertEntity(entity);
            }
        }

        public List<IQuadTreeEntity> Query(QuadTreeRect range)
        {
            return _rootNode?.Query(range) ?? new List<IQuadTreeEntity>();
        }
        
        public List<IQuadTreeEntity> Query(Vector3 center, float range)
        {
            return _rootNode?.Query(center, range) ?? new List<IQuadTreeEntity>();
        }
        
        public void DebugDraw()
        {
            _rootNode?.DebugDraw();
        }
    }
}