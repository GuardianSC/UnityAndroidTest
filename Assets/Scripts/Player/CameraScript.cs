using UnityEngine;
using System.Collections;

namespace AndroidTest2D
{
    public class CameraScript : MonoBehaviour
    {
        public Controller target;
        public Vector2 focusAreaSize;

        public float verticalOffset;
    
        public float lookAheadDistanceX;
        public float lookSmoothTimeX;
        public float verticalSmoothTime;

        focusArea fArea;
    
        float currentlookAheadX;
        float targetLookAheadX;
        float lookAheadDirectionX;
        float smoothLookmoveAmountX;
        float smoothmoveAmountY;
    
        bool lookAheadStopped;
    
        void Start()
        {
            fArea = new focusArea(target.GetComponent<BoxCollider2D>().bounds, focusAreaSize);
        }
    
        void LateUpdate()
        {
            fArea.Update(target.GetComponent<BoxCollider2D>().bounds);
            Vector2 focusPosition = fArea.center + Vector2.up * verticalOffset;

            if (fArea.moveAmount.x != 0)
            {
                lookAheadDirectionX = Mathf.Sign(fArea.moveAmount.x);
                if (Mathf.Sign(target.pInput.x) == Mathf.Sign(fArea.moveAmount.x) && target.pInput.x != 0)
                {
                    lookAheadStopped = false;
                    targetLookAheadX = lookAheadDirectionX * lookAheadDistanceX;
                }
                else
                {
                    if (!lookAheadStopped)
                    {
                        lookAheadStopped = true;
                    }
                    targetLookAheadX = currentlookAheadX + (lookAheadDirectionX * lookAheadDistanceX - currentlookAheadX) / 4;
                }
            }

            targetLookAheadX = lookAheadDirectionX * lookAheadDistanceX;

            currentlookAheadX = Mathf.SmoothDamp(currentlookAheadX, targetLookAheadX, ref smoothLookmoveAmountX, lookSmoothTimeX);

            focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothmoveAmountY, verticalSmoothTime);

            focusPosition += Vector2.right * currentlookAheadX;        

            transform.position = (Vector3)focusPosition + Vector3.forward * -10;
        }

        void OnDrawGizmos()
        { 
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawCube(fArea.center, focusAreaSize);
        }
     
        struct focusArea
        {
            public Vector2 center;
            public Vector2 moveAmount;
            float left, right, top, bottom;

            public focusArea(Bounds targetBounds, Vector2 size)
            {
                left   = targetBounds.center.x - size.x / 2;
                right  = targetBounds.center.x + size.x / 2;
                top    = targetBounds.min.y + size.y;
                bottom = targetBounds.min.y;

                center = new Vector2((left + right) / 2, (top + bottom) / 2);
                moveAmount = Vector2.zero;
            }

            public void Update(Bounds targetBounds)
            {
                float shiftX = 0;
                float shiftY = 0;

                if (targetBounds.min.x < left)
                    shiftX = targetBounds.min.x - left;
                else if (targetBounds.max.x > right)
                    shiftX = targetBounds.max.x - right;

                left  += shiftX;
                right += shiftX;

                if (targetBounds.min.y < bottom)
                    shiftY = targetBounds.min.y - bottom;
                else if (targetBounds.max.y > top)
                    shiftY = targetBounds.max.y - top;

                top += shiftY;
                bottom += shiftY;

                center = new Vector2((left + right) / 2, (top + bottom) / 2);
                moveAmount = new Vector2(shiftX, shiftY);
            }
        }
    }
}