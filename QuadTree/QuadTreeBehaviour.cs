using UnityEngine;

namespace CCLBStudio.SpatialPartitioning.QuadTree
{
    public class QuadTreeBehaviour : MonoBehaviour
    {
        [SerializeField] private SpatialQuadTree quadTree;
        [SerializeField] private Transform quadTreeCenter;
        [Tooltip("If TRUE, the quad tree will initialize itself during the Start method.")]
        [SerializeField] private bool autoInitialize = true;
        
        private void Start()
        {
            if (autoInitialize)
            {
                quadTree.Initialize(quadTreeCenter.position);
            }
        }

        private void OnDrawGizmos()
        {
            if (!quadTree)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            quadTree.DebugDraw();
        }
    }
}