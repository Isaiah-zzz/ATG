using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor.Callbacks;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float playerScale;
    [SerializeField] private float grav;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask cam1Layer;
    [SerializeField] private LayerMask cam2Layer;
    [SerializeField] private float dashSpeed;
    [SerializeField] private TrailRenderer tr;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    private bool dashReady = true;
    private bool dashing;
    private float dashTime = 0.3f;
    private float dashRefresh = .5f;



    [SerializeField] private GameObject cam1;
    [SerializeField] private GameObject cam2;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {

        if(dashing)
        {
            return;
        }

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

        if (Input.GetKey(KeyCode.LeftShift) && dashReady)
        {
            StartCoroutine(Dash());
            // Dash();
        }

        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        
    }

    private void Jump(){
        body.velocity = new Vector2(body.velocity.x, jumpSpeed);
    }

    private IEnumerator Dash()
    {
        dashReady = false;
        dashing = true;
        float originalGravity = body.gravityScale;
        body.gravityScale = 0f;
        body.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashTime);
        tr.emitting = false;
        body.gravityScale = originalGravity;
        dashing = false;
        yield return new WaitForSeconds(dashRefresh);
        dashReady = true;

    }

    private void OnCollisionEnter2D(Collision2D collision){
    }

    private bool isGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

}
