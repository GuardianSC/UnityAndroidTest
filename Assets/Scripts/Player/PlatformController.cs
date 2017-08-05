using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AndroidTest2D;

namespace AndroidTest2D
{
    public class PlatformController : RaycastController
    {
        public LayerMask passengerMask;

        public Vector3[] localWaypoints;
        Vector3[] globalWaypoints;

        public float speed;
        public float waitTime;
        float nextMoveTime;
        float percentBetweenWaypoints;
        int fromWaypointIndex;
        public bool cyclic;
        [Range(0, 2)] // Keep ease amount between 0 and 2
        public float easeAmount;

        List<passengerMovement> passMovement;
        Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

        // Use this for initialization
        public override void Start()
        {
            base.Start();

            globalWaypoints = new Vector3[localWaypoints.Length];
            for (int i = 0; i < localWaypoints.Length; i++)
                globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            updateRaycastOrigins();
            Vector3 moveAmount = platformMovement();
            calculatePassengers(moveAmount);
            movePassengers(true);
            transform.Translate(moveAmount);
            movePassengers(false);
        }

        float ease(float x)
        {
            float a = easeAmount + 1;
            return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
        }

        Vector3 platformMovement() // Calculates platform movement
        {
            if (Time.time < nextMoveTime)
                return Vector3.zero;

            fromWaypointIndex %= globalWaypoints.Length;
            int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
            float distance = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
            percentBetweenWaypoints += Time.deltaTime * speed / distance;
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
            float easePercent = ease(percentBetweenWaypoints);

            Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easePercent);

            // If a waypoint has been reached, move on to the next one
            if (percentBetweenWaypoints >= 1)
            {
                percentBetweenWaypoints = 0;
                fromWaypointIndex++;
                if (!cyclic)
                {
                    if (fromWaypointIndex >= globalWaypoints.Length - 1)
                    {
                        fromWaypointIndex = 0;
                        System.Array.Reverse(globalWaypoints);
                    }
                }
                nextMoveTime = Time.time + waitTime;
            }

            return newPos - transform.position;
        }

        void movePassengers(bool beforeMovePlatform)
        {
            foreach (passengerMovement passenger in passMovement)
            {
                if (!passengerDictionary.ContainsKey(passenger.transform))
                    passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller>());

                if (passenger.moveBeforePlatform == beforeMovePlatform)
                    passengerDictionary[passenger.transform].Move(passenger.moveAmount, passenger.onPlatform);
            }
        }

        void calculatePassengers(Vector3 moveAmount) // Calculates passenger movement
        {
            // Helps with determining how many passengers are on a moving platform
            HashSet<Transform> movedPassengers = new HashSet<Transform>();
            passMovement = new List<passengerMovement>();
            float directionX = Mathf.Sign(moveAmount.x);
            float directionY = Mathf.Sign(moveAmount.y);

            /// Vertically moving platform
            if (moveAmount.y != 0)
            {
                float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

                for (int i = 0; i < verticalRays; i++)
                {
                    Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                    if (hit && hit.distance != 0)
                    {
                        if (!movedPassengers.Contains(hit.transform)) // Helps with determining how many passengers are on a moving platform
                        {
                            float pushX = (directionY == 1) ? moveAmount.x : 0;
                            float pushY = moveAmount.y - (hit.distance - skinWidth) * directionY; // hit.distance = distance between passenger and platform

                            passMovement.Add(new passengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                        }
                    }
                }
            }
            /// Horizontally moving platform
            if (moveAmount.x != 0)
            {
                float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

                for (int i = 0; i < verticalRays; i++)
                {
                    Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topRight;
                    rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                    if (hit && hit.distance != 0)
                    {
                        if (!movedPassengers.Contains(hit.transform)) // Helps with determining how many passengers are on a moving platform
                        {
                            float pushX = moveAmount.x - (hit.distance - skinWidth) * directionX;
                            float pushY = -skinWidth; // hit.distance = distance between passenger and platform

                            passMovement.Add(new passengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                        }
                    }
                }
            }
            /// Passenger on top of a horizontally or downward moving platformF
            if (directionY == -1 || moveAmount.y == 0 && moveAmount.x != 0)
            {
                float rayLength = skinWidth * 2;

                for (int i = 0; i < verticalRays; i++)
                {
                    Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                    if (hit && hit.distance != 0)
                    {
                        if (!movedPassengers.Contains(hit.transform))
                        {
                            movedPassengers.Add(hit.transform);
                            float pushX = moveAmount.x;
                            float pushY = moveAmount.y;

                            passMovement.Add(new passengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                        }
                    }
                }
            }
        }

        struct passengerMovement
        {
            public Transform transform;
            public Vector3 moveAmount;
            public bool onPlatform;
            public bool moveBeforePlatform;

            public passengerMovement(Transform _transform, Vector3 _moveAmount, bool _onPlatform, bool _moveBeforePlatform)
            {
                transform = _transform;
                moveAmount = _moveAmount;
                onPlatform = _onPlatform;
                moveBeforePlatform = _moveBeforePlatform;
            }
        }

        void OnDrawGizmos()
        {
            if (localWaypoints != null)
            {
                Gizmos.color = Color.red;
                float size = .3f;

                for (int i = 0; i < localWaypoints.Length; i++)
                {
                    Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                    Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                    Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
                }
            }
        }
    }

}