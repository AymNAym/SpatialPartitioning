using System;
using CCLBStudio.SpatialPartitioning.QuadTree;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CCLBStudio.SpatialPartitioning.Demo
{
    public class SimpleQuadTreeEntity : MonoBehaviour, IQuadTreeEntity
    {
        [SerializeField] private SpatialQuadTree quadTree;
        [SerializeField] private Vector2 minMaxX;
        [SerializeField] private Vector2 minMaxY;
        [SerializeField] private float speed = 10f;

        private Vector3 _targetPos;

        private void Start()
        {
            _targetPos = GetRandomPosition();
        }

        private void Update()
        {
            if (Vector3.Distance(transform.position, _targetPos) <= .1f)
            {
                _targetPos = GetRandomPosition();
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            quadTree.UpdateEntity(this);
        }

        private void OnDestroy()
        {
            quadTree.RemoveEntity(this);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        private Vector3 GetRandomPosition()
        {
            return new Vector3(Random.Range(minMaxX.x, minMaxX.y), 0f, Random.Range(minMaxY.x, minMaxY.y));
        }
    }
}