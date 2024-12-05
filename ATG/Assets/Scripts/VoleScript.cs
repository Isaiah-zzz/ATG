using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class VoleScript : MonoBehaviour
{
    // Unity stuff
    [SerializeField] private LayerMask groundLayer;
    private int groundNum;
    Rigidbody2D body;
    CircleCollider2D circleCollider;
    GameObject player;

    // Variables for movement
    [SerializeField] float moveSpeed = 3f;
    private float curMoveSpeed;
    [SerializeField] float jumpPower = 8f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float velPower = 1.1f;
    private float direction = 1;
    private bool onWall = false;

    // Variabeles for behavior timing
    [SerializeField] float interval = 5f;
    private float timeCount = 0f;
    Animator animator;
    // distance from player at which enemy becomes active 
    [SerializeField] float activeDistance = 50f;

    // Start is called before the first frame update
    void Start()
    {
        curMoveSpeed = moveSpeed;
        groundNum = LayerMask.NameToLayer("Ground");
    }

    // TODO: Make the boss only active once the player is within a certain range

    void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        player = GameObject.FindWithTag("Player");
        if (player == null) print("Could not find player");

        // movement variables need to be scaled relative to mass because he is chunky
        jumpPower *= body.mass;
        curMoveSpeed *= body.mass;
        acceleration *= body.mass;
        deceleration *= body.mass;
    }

    // Update is called once per frame
    void Update()
    {

        if (!PlayerInRange())
        {
            body.velocity = new Vector2(0, 0);
            return;
        }

        // every [interval] seconds, walk in a direction, pause, or hop at player
        if (timeCount >= interval)
        {
            if (!onWall)
            {
                // generate chance 1-100%
                System.Random rnd = new();
                int chance = rnd.Next(1, 101);

                if (chance <= 30)   // walk right
                {
                    direction = 1;
                    animator.SetInteger("int", 1);
                }
                else if (chance <= 60)  // walk left
                {   
                    direction = -1;
                    animator.SetInteger("int", -1);
                }
                else if (chance <= 80)  // pause
                {
                    // halt x velocity to reduce unexpected behaviors
                    body.velocity = new Vector2(0, body.velocity.y);
                    direction = 0;
                    animator.SetInteger("int", 0);
                }
                else    // jump at player
                {
                    // TODO: implement jump in an arc toward player
                    // make boulders fall when hitting the ground?
                }
            }

            // reset time count back to 0 to start next interval
            timeCount = 0f;
        } 
        else 
        {
            // increment timeCount if interval has not been completed
            timeCount += Time.deltaTime;
        }

        // bounce off wall instead of walking into it repeatedly
        if (onWall)
        {
            // body.velocity = new Vector2(0, body.velocity.y);
            onWall = false;
            direction *= -1;
            timeCount = 0;
        }

        // add force to move vole in appropriate direction
        float targetSpeed = direction * curMoveSpeed;
        float speedDif = targetSpeed - body.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        body.AddForce(movement * Vector2.right);

    }

    // this function is called whenever the player jumps
    private void Jump()
    {
        // generate chance 1-100%
        System.Random rand = new();
        int jumpChance = rand.Next(1, 101);

        // 25% chance to also jump whenever the player does
        if ( jumpChance <= 25 && IsGrounded())
        {
            Debug.Log("Playing attack animation");
            // animator.Play("attack");
            animator.SetTrigger("attack");
            // reset y velocity to reduce unexpected behaviors
            body.velocity = new Vector2(body.velocity.x, 0);
            // add jump force
            body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
        }
    }

    // update onWall variable when hitting a wall on either side
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If on a wall, stop walking in that direction
        if (other.gameObject.layer == groundNum)
        {
            onWall = true;
        }
    }

    // Checks distance between this enemy and player
    private bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, player.transform.position) < activeDistance;
    }

    // NOTE: probably don't need this
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.gameObject.layer == groundNum)
    //     {
    //         onWall = false;
    //     }
    // }

    // checking if vole is grounded with raycast
    private bool IsGrounded()
    {
        float Distance = circleCollider.bounds.extents.y + 0.1f;
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, Distance, groundLayer);
        return raycastHit.collider != null;
    }
}
