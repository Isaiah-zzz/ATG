using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float playerScale;
    [SerializeField] private float grav;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask cam1Layer;
    [SerializeField] private LayerMask cam2Layer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    [SerializeField] private GameObject cam1;
    [SerializeField] private GameObject cam2;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // flip player direction
        if(horizontalInput > 0.01f){
            transform.localScale = new Vector3(playerScale, playerScale, playerScale);
        } else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-playerScale, playerScale, playerScale);
        }

        if(transform.position.x < 13)
        {
            print("CAM 1");
            cam1.SetActive(true);
            cam2.SetActive(false);
        }

        if(!isGrounded() && body.velocity.y < 2)
        {
            body.gravityScale = grav * 2;
        }

        if(isGrounded())
        {
            body.gravityScale = grav;
        }

        if(transform.position.x > 13)
        {
            cam2.SetActive(true);
            cam1.SetActive(false);
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded())
        {
            Jump();
        }
        
        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        
    }

    private void Jump(){
        body.velocity = new Vector2(body.velocity.x, jumpSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision){
    }

    private bool isGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool inCam1()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, cam1Layer);
        return raycastHit.collider != null;
    }

    private bool inCam2()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, cam2Layer);
        return raycastHit.collider != null;
    }
}
