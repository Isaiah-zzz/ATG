using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTossGrowth : MonoBehaviour
{
    public GameObject miniCornstalkPrefab;
    [SerializeField] private GameObject cornstalkPrefab;
    [SerializeField] private GameObject popcornPrefab;
    [SerializeField] private Transform grainSpawnPoint;
    private GameObject popcornInst;
    private GameObject miniCornInst;

    public float popcornTossForce = 20f;
    public float cornTossForce = 15f;


    private SpriteRenderer sprite;

    public bool isTossingCornstalk = false;
    public bool isThrowingPopcorn = false;


    void Start()
    {

        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isThrowingPopcorn = false;
            ToggleThrow(ref isTossingCornstalk);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            isTossingCornstalk = false;
            ToggleThrow(ref isThrowingPopcorn);
        }

        if(isTossingCornstalk) {
            HandleCornstalkThrow();
        }

        if(isThrowingPopcorn) {
            HandlePopcornThrow();    
        }
    }

    void ToggleThrow(ref bool isThrowing)
    {
        isThrowing = !isThrowing;
    }

    void HandlePopcornThrow() {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePosition - grainSpawnPoint.position).normalized;

            popcornInst = Instantiate(popcornPrefab, grainSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = popcornInst.GetComponent<Rigidbody2D>();
            rb.velocity = direction * popcornTossForce;
            isThrowingPopcorn = false;  
        }
    }

    void HandleCornstalkThrow() {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 direction = (mousePosition - grainSpawnPoint.position).normalized;

            miniCornInst = Instantiate(miniCornstalkPrefab, grainSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = miniCornInst.GetComponent<Rigidbody2D>();
            rb.velocity = direction * cornTossForce;
            isTossingCornstalk = false;  
            StartCoroutine(HandleObjectDisappear(miniCornInst));
        }
    }

    IEnumerator HandleObjectDisappear(GameObject grain)
    {
        // Wait until the object hits the ground
        Collider2D collider = grain.GetComponent<Collider2D>();
        
        while (collider != null && !collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            yield return null;
        }


        yield return new WaitForSeconds(0.3f); // Optional delay for effect

        SpriteRenderer grainRenderer = grain.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = grainRenderer.bounds.size;
        Vector3 grainPosition = new Vector3(grain.transform.position.x, grain.transform.position.y - spriteSize.y / 2, grain.transform.position.z);  

        Destroy(grain);
        Instantiate(cornstalkPrefab, grainPosition, Quaternion.identity);
    }
}
