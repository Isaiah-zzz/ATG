using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VoleScript : MonoBehaviour
{
    // Unity stuff
    [SerializeField] private LayerMask groundLayer;
    private int groundNum;
    Rigidbody2D body;
    BoxCollider2D boxCollider;
    GameObject player;

    // Movement speed variables
    [SerializeField] float moveSpeed = 3f;
    private float curMoveSpeed;

    // Variables for movement
    private float direction = 1;
    private bool onWall = false;
    [SerializeField] float interval = 5f;
    [SerializeField] float jumpPower = 15f;
    private float timeCount = 0f;

    // Start is called before the first frame update
    void Start()
    {
        curMoveSpeed = moveSpeed;
        groundNum = LayerMask.NameToLayer("Ground");
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

        // every [interval] seconds, walk in a direction, pause, or hop at player
        if (timeCount >= interval)
        {
            System.Random rnd = new();

            int chance = rnd.Next(1, 101);

            if (chance <= 30)   // walk right
            {
                direction = 1;
                body.velocity = new Vector2(curMoveSpeed * direction, body.velocity.y);
            }
            else if (chance <= 60)  // walk left
            {   
                direction = -1;
                body.velocity = new Vector2(curMoveSpeed * direction, body.velocity.y);
            }
            else if (chance <= 80)  // pause
            {
                body.velocity = new Vector2(0f, body.velocity.y);
            }
            else    // jump at player
            {
                print("hop time");
            }

            timeCount = 0f;
        } 
        else 
        {
            timeCount += Time.deltaTime;
        }

        if (onWall)
        {
            direction *= -1;
            body.velocity = new Vector2(curMoveSpeed * direction, body.velocity.y);
            timeCount = 0f;
            onWall = false;
        }

    }

    private void Jump()
    {
        print("player jumped");

        System.Random rand = new();
        int jumpChance = rand.Next(1, 101);

        if ( jumpChance <= 25)
        {
            // add jump force and reset coyote/jump buffer times
            body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);

            print("Vole jumped");
        }
    }

    // handle collision with terrain
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If on a wall, stop walking in that direction
        if (other.gameObject.layer == groundNum)
        {
            onWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == groundNum)
        {
            onWall = false;
        }
    }

    // checks if layer attached to collider is groundLayer
    private bool ColliderIsGround(Collider2D other)
    {
        return (groundLayer & (1 << other.gameObject.layer)) != 0;
    }
}
