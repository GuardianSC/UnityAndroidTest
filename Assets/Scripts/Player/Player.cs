using UnityEngine;
using System.Collections;
using AndroidTest2D;
using Lean.Touch;

[RequireComponent(typeof(Controller))]
//[RequireComponent(typeof(PlayerProjectile))]
public class Player : MonoBehaviour
{
    Controller controller;
    PlayerInput pInput;

    Vector2 moveAmount;
    Vector2 dirInput;
    float moveAmountXSmoothing;
    float moveSpeed = 6;
    float gravity;

    float maxJumpmoveAmount;
    float minJumpmoveAmount;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 0.4f;

    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    public float wallSpeedMax = 3; // Maximum wall slide speed
    public float wallStick = .25f; // Time player sticks to a wall
    float wallUnstick; // Time until player falls off wall
    bool  wallSlide;
    int   wallDirX;
    public Vector2 wallJumpClimb; // Input towards the current wall when jumping
    public Vector2 wallJump; // No input when jumping (short hop off current wall)
    public Vector2 wallLeap; // Input to opposite direction of current wall when jumping

    public bool facingRight = true;

#region LeanTouch
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
#endregion

    /// Use this for initialization
    void Start()
    {
        controller = GetComponent<Controller>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        minJumpmoveAmount = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        maxJumpmoveAmount = Mathf.Abs(gravity) * timeToJumpApex;
    }

    /// Update is called once per frame
    void FixedUpdate()
    {
        calculatemoveAmount();
        wallSliding();

        controller.Move(moveAmount * Time.deltaTime, dirInput);

        if (controller.collisionInfo.above || controller.collisionInfo.below)
            moveAmount.y = 0;
        if (swipeLeft)
        {
            
        }
    }

    public void setDirectionalInput(Vector2 input)
    {
        dirInput = input;
    }

    // On jump swipe
    public void onJumpSwipe()
    {
        // Wall jumping
        if (wallSlide)
        {
            if (wallDirX == dirInput.x) // Input towards the current wall when jumping
            {
                moveAmount.x = -wallDirX * wallJumpClimb.x;
                moveAmount.y = wallJumpClimb.y;
            }
            else if (dirInput.x == 0) // No input when jumping (short hop off current wall)
            {
                moveAmount.x = -wallDirX * wallJump.x;
                moveAmount.y = wallJump.y;
            }
            else // Input to opposite direction of current wall when jumping
            {
                moveAmount.x = -wallDirX * wallLeap.x;
                moveAmount.y = wallLeap.y;
            }
        }

        // Normal jump, just touching the ground/a platform
        if (controller.collisionInfo.below)
            moveAmount.y = maxJumpmoveAmount;
    }

    // On jump input up
    public void onJumpUp()
    {
        if (moveAmount.y > minJumpmoveAmount)
            moveAmount.y = minJumpmoveAmount;
    }

    public void calculatemoveAmount()
    {
        float targetmoveAmountX = dirInput.x * moveSpeed;
        moveAmount.x = Mathf.SmoothDamp(moveAmount.x, targetmoveAmountX, ref moveAmountXSmoothing, (controller.collisionInfo.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        moveAmount.y += gravity * Time.deltaTime;
    }

    void wallSliding()
    {
        wallDirX = (controller.collisionInfo.left) ? -1 : 1;


        wallSlide = false;
        if ((controller.collisionInfo.left || controller.collisionInfo.right) && !controller.collisionInfo.below && moveAmount.y < 0)
        {
            wallSlide = true;

            if (moveAmount.y < -wallSpeedMax)
                moveAmount.y = -wallSpeedMax;

            if (wallUnstick > 0)
            {
                moveAmountXSmoothing = 0;
                moveAmount.x = 0;
                if (dirInput.x != wallDirX && dirInput.x != 0)
                    wallUnstick -= Time.deltaTime;
            }
            else
                wallUnstick = wallStick;
        }
    }
}