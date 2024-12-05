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

    public float detectionRadius = 0.3f;
    private SpriteRenderer sprite;

    [SerializeField] float cornStalkDuration = 15f;
    [SerializeField] float popcornDuration = 15f;

    public bool isTossingCornstalk = false;
    public bool isThrowingPopcorn = false;
    [SerializeField] private int cornCollectLimit = 1;
    [SerializeField] private int popcornCollectLimit = 3;
    private int cornCount;
    public int CurrentCornCount => cornCount;
    private int popcornCount;
    public int CurrentPopcornCount => popcornCount;

    //Sound FX Clips
    [SerializeField] private AudioClip growthClip;
    [SerializeField] private AudioClip popcornClip;


    void Start()
    {

        sprite = GetComponent<SpriteRenderer>();
        cornCount = 0;
        popcornCount = 0;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cornCount >= cornCollectLimit)
        {
            isThrowingPopcorn = false;
            ToggleThrow(ref isTossingCornstalk);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && popcornCount >= popcornCollectLimit)
        {
            isTossingCornstalk = false;
            ToggleThrow(ref isThrowingPopcorn);
        }

        if(isTossingCornstalk) {
            LayerMask wallLayer = LayerMask.GetMask("Ground");
            Collider2D wallCollider = Physics2D.OverlapCircle(grainSpawnPoint.position, detectionRadius, wallLayer);
            Debug.Log(wallCollider);
            if(wallCollider == null) {
                HandleCornstalkThrow();
            }
        }

        if(isThrowingPopcorn) {
            LayerMask wallLayer = LayerMask.GetMask("Ground");
            Collider2D wallCollider = Physics2D.OverlapCircle(grainSpawnPoint.position, detectionRadius, wallLayer);
            if(wallCollider == null) {
                HandlePopcornThrow();
            } 
        }
    }

    void ToggleThrow(ref bool isThrowing)
    {
        isThrowing = !isThrowing;
    }

    void HandlePopcornThrow() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // click to throw
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 direction = (mousePosition - grainSpawnPoint.position).normalized;

            popcornInst = Instantiate(popcornPrefab, grainSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = popcornInst.GetComponent<Rigidbody2D>();
            //rb.velocity = direction * popcornTossForce;
            rb.AddForce(direction * popcornTossForce, ForceMode2D.Impulse);
            isThrowingPopcorn = false;  
            popcornCount -= popcornCollectLimit;
            StartCoroutine(DestroySpawnedObject(popcornInst, popcornDuration));
        }

        //flip player
        if (mousePosition.x - transform.position.x < 0) {
            // left
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        } else {
            // right
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void HandleCornstalkThrow() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // click to throw
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 direction = (mousePosition - grainSpawnPoint.position).normalized;

            miniCornInst = Instantiate(miniCornstalkPrefab, grainSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = miniCornInst.GetComponent<Rigidbody2D>();
            //rb.velocity = direction * cornTossForce;
            rb.AddForce(direction * cornTossForce, ForceMode2D.Impulse);
            isTossingCornstalk = false;  
            cornCount -= cornCollectLimit;
            StartCoroutine(HandleObjectDisappear(miniCornInst));
        }

        //flip player
        if (mousePosition.x - transform.position.x < 0) {
            // left
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        } else {
            // right
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    IEnumerator HandleObjectDisappear(GameObject grain)
    {
        Collider2D collider = grain.GetComponent<Collider2D>();

        bool isOnValidGround = false;
        float timeout = 8f; // Maximum wait time in seconds
        float elapsedTime = 0f;

        while (!isOnValidGround && elapsedTime < timeout)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait until the object touches something on the Ground layer
            while (!collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= timeout)
                    break;
            }
            // Check all contact points
            ContactPoint2D[] contacts = new ContactPoint2D[10]; // Buffer for contacts
            int contactCount = collider.GetContacts(contacts);

            isOnValidGround = false;

            for (int i = 0; i < contactCount; i++)
            {
                // Verify if the contact point normal is mostly upward
                if (contacts[i].normal.y > 0.7f) // Adjust the threshold as needed
                {
                    isOnValidGround = true;
                    break;
                }
            }

            if (!isOnValidGround && elapsedTime < timeout)
            {
                // If the ground isn't valid and timeout hasn't been reached, keep waiting and checking
                yield return null;
            }
        }

        // If the timeout was reached without landing on valid ground, destroy the object
        if (!isOnValidGround)
        {
            Destroy(grain);
            yield break; // Exit the coroutine
        }

        // Once valid ground is confirmed, wait briefly before proceeding
        yield return new WaitForSeconds(0.3f);

        StartCoroutine(DestroyObject(grain));

    }


    IEnumerator DestroyObject(GameObject grain) {
        Collider2D collider = grain.GetComponent<Collider2D>();
        RaycastHit2D[] hits;
        int count = 0;

        while (true) {
            if (collider.IsTouchingLayers(LayerMask.GetMask("Ground"))) {
                // Cast a box or ray to detect all surfaces the grain touches
                hits = Physics2D.RaycastAll(grain.transform.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
                
                if (hits.Length > 0) {
                    // Find the highest Y position among all hits
                    float maxGroundYPosition = float.MinValue;
                    foreach (var hit in hits) {
                        if (hit.collider != null && hit.point.y > maxGroundYPosition) {
                            maxGroundYPosition = hit.point.y;
                        }
                    }

                    if (maxGroundYPosition > float.MinValue) {
                        // Place the cornstalk on top of the highest ground
                        grain.transform.rotation = Quaternion.identity;

                        Vector3 grainPosition = new Vector3(
                            grain.transform.position.x,
                            maxGroundYPosition,
                            grain.transform.position.z
                        );

                        Destroy(grain);
                        var cornstalk = Instantiate(cornstalkPrefab, grainPosition, Quaternion.identity);

                        // Play Sound FX
                        SoundFXManager.instance.PlaySoundFXClip(growthClip, transform, 0.5f);

                        StartCoroutine(DestroySpawnedObject(cornstalk, cornStalkDuration));

                        yield break; // Exit the coroutine
                    }
                }
            }

            // Timeout check to avoid infinite loops
            count++;
            if (count >= 1200) {
                Destroy(grain);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator DestroySpawnedObject(GameObject gameObject, float duration)
    {
        yield return new WaitForSeconds(duration);

        //If player is in range, do this
        if (Vector2.Distance(transform.position, gameObject.transform.position) < 50)
        {
            //Flash sprite before destroy
            for (int i = 0; i < 3; i++)
            {
                Color tmp = gameObject.GetComponent<SpriteRenderer>().color;
                tmp.a = 0f;
                gameObject.GetComponent<SpriteRenderer>().color = tmp;
                yield return new WaitForSeconds(0.25f);
                tmp = gameObject.GetComponent<SpriteRenderer>().color;
                tmp.a = 1f;
                gameObject.GetComponent<SpriteRenderer>().color = tmp;
                yield return new WaitForSeconds(0.25f);
            }
            SoundFXManager.instance.PlaySoundFXClip(popcornClip, transform, 0.5f);
        }
        Destroy(gameObject);
    }

    public void AddCount(string tagName) {
        if(tagName == "CornCollectible") {
            cornCount++;
        } else if(tagName == "PopcornCollectible") {
            popcornCount++;
        }
    }

}
