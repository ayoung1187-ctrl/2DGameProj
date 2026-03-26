/* 
 * Purpose: This class exists as a data sheet for each object. Also handles when the object makes a noise.
 * 
 * Attached To: All buildable objects (SteelBeam, CosmicCircle, WoodenCrate, DecoritiveTriangle, and Sheep)
 * 
 * Last Edited: 3/25/26
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ObjectData : MonoBehaviour
{
    /*
     * Field Variables
     */

    [Header("Item Parameters")]
    public string objectID;
    public int cost;

    // Other variables
    private AudioSource placeSE;
    private bool isBought = false;

    public Rigidbody2D RB { get; private set; }
    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        placeSE = GetComponent<AudioSource>();
    }

    // Used in InteractObjHandling() so that the conveyor will reject a bought item
    public Vector2 GetCurPosition()
    {
        return RB.position;
    }

    // Getter and setter for whether or not this instance has been bought.
    public bool GetIsBought()
    {
        return isBought;
    }

    public void SetIsBought(bool isBought)
    {
        this.isBought = isBought;
    }

    // Audio player handler
    void OnCollisionEnter2D()
    {
        if (placeSE == null)
        {
            Debug.Log("null sound");
        }
        placeSE.Play();
    }
}
