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
    [SerializeField] GameObject player;

    // toggleable bool to stop enemy from moving
    [SerializeField] bool active = true;

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
    }

    // Update is called once per frame
    void Update()
    {

        if (!active)
        {
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
            transform.localScale = new Vector2(-(Mathf.Sign(body.velocity.x) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            onWall = false;
        }

        // update movement depending on which way enemy is facing
        if (IsGrounded())
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
        if (directionFlipGrace > 0)
        {
            directionFlipGrace -= Time.deltaTime;
        } else { directionFlipGrace = 0f; }

        if (initializeTime > 0)
        {
            onWall = false;
            initializeTime -= Time.deltaTime;
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

    // swap the direction if about to walk off an edge
    private void OnTriggerExit2D(Collider2D other)
    {
        if (/*other.CompareTag("Ground")*/(groundLayer & (1 << other.gameObject.layer)) != 0 && directionFlipGrace == 0)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(body.velocity.x) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            directionFlipGrace = 1f;
        }
    }

    // handle collision with terrain
    private void OnTriggerEnter2D(Collider2D other)
    {
        // if hitting terrain and still trying to chase player, halt movement
        if ((ColliderIsGround(other) || other.CompareTag("Popcorn") || other.CompareTag("Cornstalk")) && targetsPlayer && IsFacingPlayer())
        {
            onWall = true;
        }
        else if ((ColliderIsGround(other) || other.CompareTag("Popcorn") || other.CompareTag("Cornstalk")) && directionFlipGrace == 0)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(body.velocity.x) * Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            directionFlipGrace = 1f;
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