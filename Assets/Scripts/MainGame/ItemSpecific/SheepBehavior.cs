using System.Collections;
using UnityEngine;

public class SheepBehavior : MonoBehaviour
{
    [SerializeField] private float hopForce = 3f;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private ObjectData objectData;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        objectData = GetComponent<ObjectData>();
        StartCoroutine(HopRoutine());
    }

    private IEnumerator HopRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            if (rb.bodyType != RigidbodyType2D.Dynamic) continue;
            if (objectData.GetIsOnGrid()) continue;
            if (!objectData.GetIsBought()) continue; // don't hop on conveyor

            // randomly decide direction
            float direction = Random.value > 0.5f ? 1f : -1f;
            sr.flipX = direction < 0; // flip sprite to face direction

            rb.linearVelocity = new Vector2(direction * hopForce, hopForce);
        }
    }
}