using UnityEngine;
using System.Collections;
using Lean.Touch;

namespace AndroidTest2D
{
    [RequireComponent(typeof(Player))]
    public class PlayerInput : MonoBehaviour
    {
        Player player;
        Controller controller;

        public bool swipeRight, swipeLeft, jump, grounded;

        /// Lean touch function
        void OnEnable()
        {
            LeanTouch.OnFingerSwipe += LeanTouch.OnFingerSwipe;
        }

        /// Lean touch function
        void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= LeanTouch.OnFingerSwipe;
        }

        /// Use this for initialization
        void Start()
        {
            player = GetComponent<Player>();
        }

        public void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;

            // Swipe left
            if (swipe.x < -Mathf.Abs(swipe.y))
            {
                // Set swipeLeft to true and swipeRight to false if it is already true
                swipeLeft = true;
                if (swipeRight)
                    swipeRight = false;
            }
            // Swipe right
            if (swipe.x > Mathf.Abs(swipe.y))
            {
                // Set swipeRight to true and swipeLeft to false if it is already true
                swipeRight = true;
                if (swipeLeft)
                    swipeLeft = false;
            }
            // Swipe up
            if (swipe.y > Mathf.Abs(swipe.x))
            {
                jump = true;
                //grounded = false;
            }
        }

        void flipPlayer()
        {
            player.facingRight = !player.facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}