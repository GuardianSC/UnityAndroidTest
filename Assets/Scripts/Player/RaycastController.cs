using UnityEngine;
using System.Collections;

namespace AndroidTest2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : MonoBehaviour
    {
        public RaycastOrigins raycastOrigins;

        public LayerMask collisionMask;

        public const float skinWidth = .015f; /// How far inside the player object raycasts will start (they will not start directly at the edges) 
        
        const float distanceBetweenRays = .25f;

        [HideInInspector] public int   horizontalRays; /// How many horizontal rays are going to be sent
        [HideInInspector] public int   verticalRays;   /// How many vertical rays are going to be sent
        [HideInInspector] public float horizontalRaySpacing; /// The space between each horizontal ray
        [HideInInspector] public float verticalRaySpacing;   /// The space between each vertical ray

        public virtual void Awake()
        {
            //col = GetComponent<BoxCollider2D>();
        }

        public virtual void Start()
        {
            calculateRaySpacing();
        }

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight, bottomLeft, bottomRight;
        }

        public void updateRaycastOrigins()
        {
            Bounds bounds = GetComponent<BoxCollider2D>().bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft  = new Vector2(bounds.min.x, bounds.min.y); // Origin is at the bottom left corner (the minimum x and y values of the player object size)
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y); // Origin is at the bottom right corner (the maximum x and minimum y values of the player object size)
            raycastOrigins.topLeft     = new Vector2(bounds.min.x, bounds.max.y); // Origin is at the top left corner (the minimum x and maximum y values of the player object size)
            raycastOrigins.topRight    = new Vector2(bounds.max.x, bounds.max.y); // Origin is at the top right corner (the maximum x and y values of the player object size)
        }

        public void calculateRaySpacing()
        {
            Bounds bounds = GetComponent<BoxCollider2D>().bounds;
            bounds.Expand(skinWidth * -2);
            float boundsHeight = bounds.size.y;
            float boundsWidth  = bounds.size.x;

            horizontalRays = Mathf.RoundToInt(boundsHeight / distanceBetweenRays); // Horizontal rays, mimimum of 2, max of maxvalue(which was set to 4 at declaration)
            verticalRays   = Mathf.RoundToInt(boundsWidth / distanceBetweenRays); ; // Vertical rays, mimimum of 2, max of maxvalue(which was set to 4 at declaration)

            horizontalRaySpacing = bounds.size.y / (horizontalRays - 1); // Y value of player object size divided by the number of rays - 1
            verticalRaySpacing   = bounds.size.x / (verticalRays - 1); // X value of player object size divided by the number of rays - 1
        }
    }

}