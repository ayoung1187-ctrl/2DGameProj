/* 
 * Purpose: This class exists as a data sheet for each object. Also handles when the object makes a noise.
 * 
 * Attached To: All buildable objects (SteelBeam, CosmicCircle, WoodenCrate, DecoritiveTriangle, and Sheep)
 * 
 * Last Edited: 3/29/26
 */

using System.Collections.Generic;
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
    public float scalingFactor;

    // Other variables
    private AudioSource placeSE;
    private bool isBought = false;
    private bool isOnGrid = false;
    public bool isCraftedItemOnGrid;
    public bool isCraftedItem;

    public Vector3 normalScale;

    [Header("Grid Shape")]
    public List<Vector2Int> shapeInCells = new List<Vector2Int>(); // So for the steel beam laying on it's side: (0,0), (1,0), (2,0) to describe its width of 3 units
    public List<Vector2Int> occupiedCells = new List<Vector2Int>();

    [SerializeField] private Sprite[] sprites = new Sprite[2];
    private SpriteRenderer spriteRenderer;

    public string hostComment;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        placeSE = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && !isCraftedItem)
        {
            spriteRenderer.sprite = sprites[0];
        }
        normalScale = gameObject.transform.localScale;
        if (placeSE == null)
        {
            Debug.LogWarning("No sound initialized");
        }
        if (isCraftedItem)
        {
            isCraftedItemOnGrid = true;
            isBought = true;
        }
    }

    // Rigidbody helper
    public Rigidbody2D RB { get; private set; }

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
        if (isBought)
        {
            ToBoughtSprite();
        }
    }

    // Getter and setter for checking if the object is on the grid
    public bool GetIsOnGrid() { return isOnGrid; }

    public void SetIsOnGrid(bool setter) { this.isOnGrid = setter; }

    // Audio player handler
    void OnCollisionEnter2D()
    {
        if (placeSE == null)
        {
            Debug.Log("null sound");
        }
        placeSE.Play();
    }

    // Helper to change the sprite if bought
    public void ToBoughtSprite()
    {
        if (spriteRenderer != null && !isCraftedItem)
        {
            spriteRenderer.sprite = sprites[1];
        }
    }
}
