using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

// Saved for future reference https://forum.unity3d.com/threads/simple-swipe-and-tap-mobile-input.376160/

namespace AndroidTest2D
{
    public class MyPlayerController : MonoBehaviour
    {
        public MyPlayerController player;
        public Rigidbody2D rb2d;
        public BoxCollider2D bc2d;
        //public Transform groundCheck;
        //public Animator anim;
        public float speed = 15;
        public float jumpForce = 15;

        public bool swipeRight, swipeLeft, jump, grounded, onWall;

        // Use this for initialization
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            bc2d = GetComponent<BoxCollider2D>();
            //anim = GetComponent<Animator>();
        }

        void OnEnable()
        {
            LeanTouch.OnFingerSwipe += OnFingerSwipe;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }

        public void OnFingerSwipe(LeanFinger finger)
        {
            var swipe = finger.SwipeScreenDelta;

            // Swipe left
            if (swipe.x < -Mathf.Abs(swipe.y))
            {
                swipeLeft = true;
                if (swipeRight)
                    swipeRight = false;
            }
            // Swipe right
            if (swipe.x > Mathf.Abs(swipe.y))
            {
                swipeRight = true;
                if (swipeLeft)
                    swipeLeft = false;
            }
            // Swipe up
            if (swipe.y > Mathf.Abs(swipe.x) && grounded)
            {
                jump = true;
            }
        }

        void FixedUpdate()
        {
            //grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
            
            if (swipeRight && grounded) // Move right            
                rb2d.velocity = new Vector2(speed, rb2d.velocity.y);
            
            else if (swipeLeft && grounded) // Move left            
                rb2d.velocity = new Vector2(-speed, rb2d.velocity.y);
            // Walljump right
            if (swipeRight && onWall)
            {
                rb2d.velocity = new Vector2(speed, rb2d.velocity.y);
                rb2d.AddForce(Vector2.up *  (jumpForce * 0.75f));
            }
            // Walljump left
            if (swipeLeft && onWall)
            {
                rb2d.velocity = new Vector2(-speed, rb2d.velocity.y);
                rb2d.AddForce(Vector2.up * (jumpForce * 0.75f));
            }            
            // Jump
            if (jump) 
            {
                rb2d.AddForce(Vector2.up * jumpForce);
                jump = false;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);
            if (hit.collider.tag == "Ground")
            {
                grounded = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.tag == "Ground")
                grounded = true;
            if (collision.collider.tag == "Wall")
            {
                if (swipeRight)
                    swipeRight = false;
                if (swipeLeft)
                    swipeLeft = false;
                onWall = true;
            }               
            //if (collision.collider.tag == "Win")

        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.tag == "Ground")
                grounded = false;
            if (collision.collider.tag == "Wall")
                onWall = false;
        }
    }
}