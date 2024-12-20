using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CCLBStudio.SpatialPartitioning.Demo
{
    public class SimpleQuadTreeEntitySpawner : MonoBehaviour
    {
        [SerializeField] private Vector2 minMaxX;
        [SerializeField] private Vector2 minMaxY;
        [SerializeField] private SimpleQuadTreeEntity entityPrefab;
        [SerializeField] private int initialCount = 1;

        private void Start()
        {
            for (int i = 0; i < initialCount; i++)
            {
                float x = Random.Range(minMaxX.x, minMaxX.y);
                float y = Random.Range(minMaxY.x, minMaxY.y);
                var entity = Instantiate(entityPrefab);
                entity.transform.position = new Vector3(x, 0f, y);
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                float x = Random.Range(minMaxX.x, minMaxX.y);
                float y = Random.Range(minMaxY.x, minMaxY.y);
                var entity = Instantiate(entityPrefab);
                entity.transform.position = new Vector3(x, 0f, y);
            }
        }
    }
}