using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    // movement speed variables
    [SerializeField] float moveSpeed = 3f;
    private float curMoveSpeed;

    // toggleable bool for enemy to chase player on x axis
    [SerializeField] bool targetsPlayer = false;

    // Player object to chase
    GameObject player;

    // toggleable bool to stop enemy from moving
    [SerializeField] bool active = true;

    // distance from player at which enemy becomes active 
    [SerializeField] float activeDistance = 50f;

    // variables for when enemy is hit by popcorn
    [SerializeField] float knockback = 10f;
    private float stunTime = 0f;

    // fine tuning variables
    private float directionFlipGrace = 0f;
    private float initializeTime = 1f;
    private bool onWall = false;

    [SerializeField] private LayerMask groundLayer;
    Rigidbody2D body;
    BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        curMoveSpeed = moveSpeed;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.FindWithTag("Player");
        if (player == null) print("Could not find player");
    }

    // Update is called once per frame
    void Update()
    {
        if (!active || !PlayerInRange())
        {
            body.velocity = new Vector2(0, 0);
            return;
        }

        // enemy will stop moving if with a certain distance of player or at a wall
        if ((IsCloseEnough() || onWall) && targetsPlayer && initializeTime <= 0)
        {
            curMoveSpeed = 0f;
        } else { curMoveSpeed = moveSpeed; }

        // flip enemy to chase player on x axis if not already doing so
        if (!IsFacingPlayer() && targetsPlayer)
        {
            //transform.localScale = new Vector2(-(Mathf.Sign(body.velocity.x) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            Flip();
            onWall = false;
        }

        // update movement depending on which way enemy is facing
        if (IsGrounded() && stunTime <= 0)
        {
            if (IsFacingRight())
            {
                body.velocity = new Vector2(curMoveSpeed, 0f);
            }
            else
            {
                body.velocity = new Vector2(-curMoveSpeed, 0f);
            }
        }

        // update grace time between flipping direction
        directionFlipGrace = Mathf.Max(directionFlipGrace - Time.deltaTime, 0f);

        // update stunTime
        stunTime = Mathf.Max(stunTime - Time.deltaTime, 0f);

        if (initializeTime > 0)
        {
            onWall = false;
            initializeTime -= Time.deltaTime;
        }
    }

    // Checks distance between this enemy and player
    private bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, player.transform.position) < activeDistance;
    }

    private void Flip()
    {
        if (directionFlipGrace == 0)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(body.velocity.x) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            directionFlipGrace = 1f;
        }
    }

    // return true if enemy is facing right
    private bool IsFacingRight()
    {
        return transform.localScale.x > Mathf.Epsilon;
    }

    // return true if enemy is facing player
    private bool IsFacingPlayer()
    {
        if (player.transform.position.x > transform.position.x && IsFacingRight())
        {
            return true;
        }
        
        if (player.transform.position.x < transform.position.x && !IsFacingRight())
        {
            return true;
        }

        return false;
    }

    // return true if player and enemy x positions are within .05
    private bool IsCloseEnough()
    {
        return Math.Abs(player.transform.position.x - transform.position.x) < .05;
    }

    // If enemy is about to walk off ledge
    private void OnTriggerExit2D(Collider2D other)
    {
        if (ColliderIsGround(other))
        {
            //If targets player, stop
            if (targetsPlayer && IsFacingPlayer())
            {
                onWall = true;
            }
            //Else flip
            else
            {
                Flip();
            }
        }
    }

    // handle collision with terrain
    private void OnCollisionEnter2D(Collision2D other)
    {
        var normal = other.GetContact(0).normal;
        //If not from the side, do nothing
        if (Mathf.Abs(normal.y) == 1f)
        {
            return;
        }
        //If ground or cornstalk, either flips or stops depending on tracking
        else if (ColliderIsGround(other.collider) || other.gameObject.CompareTag("Cornstalk"))
        {
            // if hitting terrain and still trying to chase player, halt movement
            if (targetsPlayer && IsFacingPlayer())
            {
                onWall = true;
            }
            else
            {
                Flip();
            }
        }
        // if popcorn, knockback and stun
        else if (other.gameObject.CompareTag("Popcorn"))
        {
            float direction = other.gameObject.GetComponent<Rigidbody2D>().velocity.normalized.x;
            body.velocity = new Vector2(knockback * direction, knockback);
            stunTime = 5f;
        }
    }

    // check if enemy is grounded
    private bool IsGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    // checks if layer attached to collider is groundLayer
    private bool ColliderIsGround(Collider2D other) {
        return (groundLayer & (1 << other.gameObject.layer)) != 0;
    }
}