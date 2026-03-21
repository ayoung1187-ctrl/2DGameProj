/* 
 * Purpose: This class exists mostly as a helper so that the rigidbodies of the interactable objects can be easily accessed
 * 
 * Attached To: All buildable objects (rectangle, circle, square, triangle)
 * 
 * Last Edited: 3/19/26
 */

using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BuildItem : MonoBehaviour
{
    public Rigidbody2D RB { get; private set; }
    public int cost;
    private AudioSource placeSE;
    private bool isBought = false;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        placeSE = GetComponent<AudioSource>();
    }
    public Vector2 GetCurPosition()
    {
        return RB.position;
    }

    public bool GetIsBought()
    {
        return isBought;
    }

    public void SetIsBought(bool isBought)
    {
        this.isBought = isBought;
    }

    void OnCollisionEnter2D()
    {
        if (placeSE == null)
        {
            Debug.Log("null sound");
        }
        Debug.Log("entered collision");
        placeSE.Play();
    }
}
