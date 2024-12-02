using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VoleScript : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    private int groundNum;
    Rigidbody2D body;
    BoxCollider2D boxCollider;
    GameObject player;

    // movement speed variables
    [SerializeField] float moveSpeed = 3f;
    private float curMoveSpeed;

    private float direction = 1;
    private bool onWall = false;
    [SerializeField] float interval = 5f;
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

        // every 5s walk in a direction or hop
        if (timeCount >= interval)
        {
            System.Random rnd = new System.Random();

            int chance = rnd.Next(1, 101);

            if (chance <= 40)
            {
                direction = 1;
                body.velocity = new Vector2(curMoveSpeed * direction, 0f);
            }
            else if (chance <= 80)
            {   
                direction = -1;
                body.velocity = new Vector2(curMoveSpeed * direction, 0f);
            }
            else
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
            print("On a wall now");
            print("Direction is " + direction + ", flipping to " + direction * -1);
            direction *= -1;
            body.velocity = new Vector2(curMoveSpeed * direction, body.velocity.y);
            timeCount = 0f;
            onWall = false;
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
