using UnityEngine;
using System.Collections;
using AndroidTest2D;

// Sebastian Lague tutorial playlist https://www.youtube.com/playlist?list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz
namespace AndroidTest2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Controller : RaycastController
    {
        public CollisionInfo collisionInfo;
        public BoxCollider2D boxCollider;

        [HideInInspector] public Vector2 pInput; // Player input

        float maxClimbAngle   = 80;
        float maxDescendAngle = 80;

        /// Use this for initialization
        public override void Start()
        {
            base.Start();
            collisionInfo.faceDirection = 1;
            boxCollider = GetComponent<BoxCollider2D>();
        }

        public void Move(Vector2 moveAmount, bool onPlatform = false)
        {
            Move(moveAmount, Vector2.zero, onPlatform);
        }

        public void Move(Vector2 moveAmount, Vector2 input, bool onPlatform = false)
        {
            updateRaycastOrigins();
            collisionInfo.Reset();

            collisionInfo.oldmoveAmount = moveAmount;

            pInput = input;

            if (moveAmount.y < 0)
                descendSlope(ref moveAmount);
            if (moveAmount.x != 0)
                collisionInfo.faceDirection = (int)Mathf.Sign(moveAmount.x);

            horizontalCollisions(ref moveAmount);

            if (moveAmount.y != 0)
                verticalCollisions(ref moveAmount);
            if (onPlatform)
                collisionInfo.below = true;

            transform.Translate(moveAmount);
        }

        void climbSlope(ref Vector2 moveAmount, float slopeAngle)
        {
            float moveDistance = Mathf.Abs(moveAmount.x);
            float slopemoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            if (moveAmount.y <= slopemoveAmountY)
            {
                moveAmount.y = slopemoveAmountY;
                moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                collisionInfo.below = true;
                collisionInfo.climbingSlope = true;
                collisionInfo.slopeAngle = slopeAngle;
            }
        }

        void descendSlope(ref Vector2 moveAmount)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collisionInfo.slopeAngle = slopeAngle;
                            collisionInfo.descendingSlope = true;
                            collisionInfo.below = true;
                        }
                    }
                }
            }
        }

        void horizontalCollisions(ref Vector2 moveAmount)
        {
            float directionX = collisionInfo.faceDirection;
            float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

            if (Mathf.Abs(moveAmount.x) < skinWidth)
                rayLength = 2 * skinWidth;

            for (int i = 0; i < horizontalRays; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red); /// Start from the bottom left corner, draw them to the right

                if (hit)
                {
                    if (hit.distance == 0)
                        continue;
                    /// Slope climbing
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (i == 0 && slopeAngle <= maxClimbAngle)
                    {
                        if (collisionInfo.descendingSlope)
                        {
                            collisionInfo.descendingSlope = false;
                            moveAmount = collisionInfo.oldmoveAmount;
                        }
                        float distanceToSlope = 0;
                        if (slopeAngle != collisionInfo.prevSlopeAngle)
                        {
                            distanceToSlope = hit.distance - skinWidth;
                            moveAmount.x -= distanceToSlope * directionX;
                        }
                        climbSlope(ref moveAmount, slopeAngle);
                        moveAmount.x += distanceToSlope * directionX;
                    }

                    if (!collisionInfo.climbingSlope || slopeAngle > maxClimbAngle)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance; // changes ray length to first object it hits (so it doesn't calculate moveAmount of any objects behind the initial hit)

                        collisionInfo.left = directionX == -1;
                        collisionInfo.right = directionX == 1;
                    }
                }
            }
        }

        void verticalCollisions(ref Vector2 moveAmount)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

            for (int i = 0; i < verticalRays; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red); // Start from the bottom left corner, draw them to the right

                if (hit)
                {

                    if (hit.collider.tag == "Through")
                    {
                        if (collisionInfo.fallThruPlat)
                            continue;

                        // Pass through platform when jumping and continue checking for other collisions
                        if (directionY == 1 || hit.distance == 0)
                            continue;

                        // Pass through platform after falling through it and continue checking for other collisions
                        if (pInput.y == -1)
                        {
                            collisionInfo.fallThruPlat = true;
                            Invoke("ResetFallThruPlat", .25f);
                            continue;
                        }
                    }

                    moveAmount.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance; // changes ray length to first object it hits (so it doesn't calculate moveAmount of any objects below the initial hit)

                    collisionInfo.below = directionY == -1;
                    collisionInfo.above = directionY == 1;
                }
            }

            if (collisionInfo.climbingSlope)
            {
                float directionX = Mathf.Sign(moveAmount.x);
                rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != collisionInfo.slopeAngle)
                    {
                        moveAmount.x = (hit.distance - skinWidth) * directionX;
                        collisionInfo.slopeAngle = slopeAngle;
                    }
                }
            }
        }

        void ResetFallThruPlat()
        {
            collisionInfo.fallThruPlat = false;
        }

        public struct CollisionInfo
        {
            public bool above, below, left, right;
            public bool climbingSlope, descendingSlope;
            public float slopeAngle, prevSlopeAngle;
            public Vector2 oldmoveAmount;
            public int faceDirection;
            public bool fallThruPlat;

            public void Reset()
            {
                above = below = left = right = false;
                climbingSlope = descendingSlope = false;
                prevSlopeAngle = slopeAngle;
                slopeAngle = 0;
            }
        }
    }

}