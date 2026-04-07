/* 
 * Purpose: This MonoBehavior class has the purpose of deciding how the handling of all interactable, buildable objects works in this game.
 * 
 * Attached To: Main Camera
 *             
 * Class Function: This class will respond to a left-click by finding what object, if any, is overlapping with the map at that point.
 *                 If such an object exists, it'll change it's body type to kinematic and use a ghost to change its position to match the mouse as it moves.
 *                 Additionally, if right-click or R is held during this, the object will rotate at a constant speed. If F is pressed, the item will invert horizontally.
 *                 When the left-click is released, the object's body type will change back to dynamic, its position, scale and rotation will match the ghost's, and the ghost will be destroyed.
 *             
 * Edit 3/19/26: I realized I don't know if I want joint physics for dragging, so I updated this class to have drag be kinematic.
 *               The plan is to make the objects kinematic whilst on the conveyor belt and during drag, and dynamic otherwise.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractObjHandling : MonoBehaviour
{
    /* 
     * Field Variables
     */

    [Header("Input Actions")]
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction pointer;
    [SerializeField] private InputAction rightHold;
    [SerializeField] private InputAction fKey;

    [Header("Game Host Script")]
    [SerializeField] private GameHost GH;

    // Real object variables
    private Collider2D selectedCollider;
    private ObjectData item;
    private Vector2Int grabbedCell;

    // Ghost variables
    private GameObject ghostInstance;
    private Rigidbody2D ghostRb;
    private Collider2D ghostCollider;

    [Header("Object Rotation Speed")]
    [SerializeField] private float rotationSpeed = 45f;

    // Other variables
    private Camera mainCam;
    [SerializeField] private CraftingHandling craft;
    public List<Vector2Int> copyOfOldCells = new List<Vector2Int>();

    public Vector2 mouseWorldCoords;
    private Vector2 mouseOffset;
    private Vector2 trackItem;

    private bool isDragging = false;

    // Craft button
    private Collider2D buttonCollider;

    // Used to prevent dragging during room placement
    [SerializeField] private RoomDetectionHandling RDH;

    /*
     * Awake(): This function enables input actions and defines responses to clicking and letting go.
     */
    private void Awake()
    {
        mainCam = Camera.main;

        press.Enable();
        pointer.Enable();
        rightHold.Enable();
        fKey.Enable();

        press.performed += _ => StartDrag();
        press.canceled += _ => StopDrag();
    }

    /*
     * Update(): If nothing is being dragged, do nothing. Else, redefine the world coordinates of the mouse and continue to drag.
     */
    private void Update()
    {
        if (isDragging == false) return;
        if (RDH != null && RDH.isPlacing) return;

        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        ContinueDrag();
    }

    /*
     * StartDrag(): Find the object that the mouse is clicking on and create a "ghost" of it. !!!!CHANGE THIS!!!!
     */
    public void StartDrag()
    {
        if (RDH != null && RDH.isPlacing) return;

        // Convert from screen mouse position, to mouse position within the world
        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        LayerMask objectLayer = LayerMask.GetMask("Objects"); // Make it so that this script will only detect building objects
        selectedCollider = Physics2D.OverlapPoint(mouseWorldCoords, objectLayer);

        LayerMask buttonLayer = LayerMask.GetMask("CraftButton");
        buttonCollider = Physics2D.OverlapPoint(mouseWorldCoords, buttonLayer);

        if (buttonCollider != null && selectedCollider == null)
        {
            craft.CraftButtonIsPressed();
            return;
        }

        // If no object exists here, do nothing
        if (selectedCollider == null) return;

        // Now find which child collider, if any, the mouse is clicking on and relate that to the object's personal grid
        LayerMask childLayer = LayerMask.GetMask("ChildColliders");
        Collider2D childCollider = Physics2D.OverlapPoint(mouseWorldCoords, childLayer);

        // If childCollider == null, move on-- it doesn't matter

        item = selectedCollider.GetComponent<ObjectData>();
        if (item == null || item.RB == null) return; // If collider has no ObjectData script or Rigidbody2D components, do nothing

        isDragging = true;


        // If you're moving an object that was already on the grid, then clear its original position
        if (item.GetIsOnGrid())
        {
            copyOfOldCells = new List<Vector2Int>(item.occupiedCells);
            // clear the cells that match with item.occupiedCells
            for (int i = 0; i < item.occupiedCells.Count; i++)
            {
                craft.ClearSlots(item.occupiedCells[i]);
            }
            item.occupiedCells.Clear();
        }

        // Find where on this item's grid you are grabbing
        if (childCollider != null)
        {
            ObjectShapeCell clickedCell = childCollider.GetComponent<ObjectShapeCell>(); // Find the point of the object that the player is clicking on. Ex: (1,0) would be the middle portion of the horizontal beam.
            grabbedCell = clickedCell != null ? clickedCell.LocalCell : Vector2Int.zero; // If this object has relative grid data, find it's local cell. Else, it's local cell is (0,0)
        }
        else
        {
            grabbedCell = Vector2Int.zero; // If this object doesn't even have child colliders, set local cell to (0,0)
        }


        // Create ghost instance and decrease its alpha value
        ghostInstance = Instantiate<GameObject>(item.gameObject);
        SpriteRenderer sr = ghostInstance.GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 1f, 0.5f);

        
        ObjectData ghostBI = ghostInstance.GetComponent<ObjectData>();
        ghostCollider = ghostBI.GetComponent<Collider2D>();


        // Just prevents the ghost from having the price tag if the object is bought
        if (item.GetIsBought())
        {
            ghostBI.ToBoughtSprite();
        }

        ghostRb = ghostBI.RB;

        if (item.GetIsOnGrid())
        {
            ghostRb.transform.localScale = item.normalScale;
        }

        // This helps prevent unwanted movement when dragging
        ghostRb.angularVelocity = 0;
        ghostRb.linearVelocity = Vector2.zero;

        Vector2 ghostItemPos2D = new Vector2(ghostBI.RB.transform.position.x, ghostBI.RB.transform.position.y);
        mouseOffset = ghostItemPos2D - mouseWorldCoords;

        // This is used in the case that you release a bought item to the conveyor belt, it rejects it
        trackItem = ghostBI.transform.position;

        ContinueDrag();
    }

    /*
     * ContinueDrag(): During the drag, make the object kinematic if it isn't already, and make the ghost kinematic as well.
     *                 The ghost moves with the mouse and the object may rotate with the R key or flip with the F key.
     */
    private void ContinueDrag()
    {
        if (item.RB == null || ghostRb == null || selectedCollider == null)
        {
            return;
        }

        // Make the original item kinematic so that it "freezes"
        if (item.RB.bodyType != RigidbodyType2D.Static)
        {
            item.RB.bodyType = RigidbodyType2D.Static;
        }

        // Make the ghost kinematic if it isn't already
        if (ghostRb.bodyType != RigidbodyType2D.Kinematic)
        {
            ghostRb.bodyType = RigidbodyType2D.Kinematic;
        }

        ghostCollider.enabled = false;

        ghostRb.transform.position = mouseWorldCoords + mouseOffset;

        // Right-click or R is pressed
        if (rightHold.inProgress)
        {
            //ghostRb.rotation += rotationSpeed * Time.deltaTime;
            float angle = rotationSpeed * Time.deltaTime;
            mouseOffset = Quaternion.Euler(0f, 0f, angle) * mouseOffset;
            ghostRb.transform.position = mouseWorldCoords + mouseOffset;
            ghostRb.rotation += angle;
        }

        // F key is pressed
        if (fKey.triggered)
        {
            Vector2 ghostInvertScale = ghostRb.transform.localScale;
            ghostInvertScale.x = -ghostRb.transform.localScale.x;
            ghostRb.transform.localScale = ghostInvertScale;
        }
    }

    /*
     * StopDrag(): Described by comments below (a bit easier to understand).
     */
    private void StopDrag()
    {
        if (ghostRb != null && item != null)
        {
            // See if the object is dropping on the grid's collider
            LayerMask craftMask = LayerMask.GetMask("CraftGrid");
            Collider2D gridCollider = Physics2D.OverlapPoint(mouseWorldCoords, craftMask);

            if (gridCollider != null) // If item was dropped on grid, find its script component
            {
                OnGridPlacement(craft);
            }

            else if (mouseWorldCoords.y > ConveyorHandling.boundFloor) // If you moved ghost to an area within the building range
            {
                if (!item.GetIsBought())
                {
                    item.SetIsBought(true); // If item is not already bought, make it so()
                }

                if (item.isCraftedItemOnGrid)
                {
                    craft.ResetGrid();
                    item.isCraftedItemOnGrid = false;
                }

                item.RB.transform.position = ghostRb.transform.position; // Move real object to ghost object position, rotation, and scale
                item.RB.transform.rotation = ghostRb.transform.rotation;
                item.RB.transform.localScale = ghostRb.transform.localScale;
                if (item.objectID != "CosmicCircle") item.RB.bodyType = RigidbodyType2D.Dynamic; // Make real object dynamic
                else item.RB.bodyType = RigidbodyType2D.Kinematic;
                if (selectedCollider.isTrigger) selectedCollider.isTrigger = false;
                if (item.GetIsOnGrid()) item.SetIsOnGrid(false);
            }

            else if (item.GetIsBought()) // If you moved ghost to conveyor belt
            {
                item.RB.transform.position = trackItem; // If it is bought and you drag it back, force it back to build range (you cannot re-place items on conveyor)
                if (item.GetIsOnGrid()) 
                {
                    if (item.RB.bodyType != RigidbodyType2D.Kinematic) item.RB.bodyType = RigidbodyType2D.Kinematic;
                }
                else if (item.objectID != "CosmicCircle") item.RB.bodyType = RigidbodyType2D.Dynamic; // Make real object dynamic
                else item.RB.bodyType = RigidbodyType2D.Kinematic; // Make real object dynamic
            }

            else item.RB.bodyType = RigidbodyType2D.Kinematic;

            copyOfOldCells.Clear();
            Destroy(ghostInstance); // Destroy the ghost
        }

        isDragging = false;
        ghostRb = null;
        selectedCollider = null;
        item = null;
    }

    // Helper function in all cases of you're trying to place an object on the grid
    private void OnGridPlacement(CraftingHandling craft)
    {
        if (craft != null) // If grid has script, try placing it on the grid using CraftingHandling class, which returns a bool saying whether or not the placement succeeded
        {
            bool tryPlace = craft.TryPlaceObject(mouseWorldCoords, ghostRb.transform.rotation, item, grabbedCell);

            if (!tryPlace && item.GetIsBought() && !item.GetIsOnGrid())
            {
                item.RB.transform.position = trackItem;
                if (item.objectID != "CosmicCircle") item.RB.bodyType = RigidbodyType2D.Dynamic; // Make real object dynamic
                else item.RB.bodyType = RigidbodyType2D.Kinematic;
            }
            else if (!tryPlace && !item.GetIsOnGrid())
            {
                item.RB.bodyType = RigidbodyType2D.Kinematic;
                item.SetIsOnGrid(false);
            }
            else if (tryPlace && !item.GetIsOnGrid()) // If the placement succeeded, check if it wasn't already bought, then place the real object where the ghost was. AND the item was not already on the grid
            {
                if (!item.GetIsBought()) item.SetIsBought(true); // should check if item wasn't rejected
                Vector2 centered = craft.FindCenterSnap();
                item.RB.transform.position = new Vector3(centered.x, centered.y, 0.0f); // Snap to center based on how many slots the item takes up
                item.RB.transform.rotation = Quaternion.Euler(0f, 0f, (float)craft.GetAxis()); // Snap to the closest axis
                item.RB.transform.localScale = new Vector3(ghostRb.transform.localScale.x * item.scalingFactor, ghostRb.transform.localScale.y * item.scalingFactor, 1f);
                item.SetIsOnGrid(true);
                if (!selectedCollider.isTrigger) selectedCollider.isTrigger = true;
                if (item.RB.bodyType != RigidbodyType2D.Kinematic) item.RB.bodyType = RigidbodyType2D.Kinematic;
                craft.CheckAllRecipes();
                // Keep as kinematic
            }
            else if (!tryPlace && item.GetIsOnGrid()) // If placement failed and item was already on grid
            {
                item.RB.transform.position = trackItem;
                for (int i = 0; i < item.shapeInCells.Count; i++)
                {
                    craft.ForceOccupySlots(copyOfOldCells[i], item);
                }
                item.occupiedCells = copyOfOldCells;
                if (!selectedCollider.isTrigger) selectedCollider.isTrigger = true;
                if (item.RB.bodyType != RigidbodyType2D.Kinematic) item.RB.bodyType = RigidbodyType2D.Kinematic;
            }
            else if (tryPlace && item.GetIsOnGrid()) // If placement succeeded and item was already on grid
            {
                Vector2 centered = craft.FindCenterSnap();
                item.RB.transform.position = new Vector3(centered.x, centered.y, 0.0f); // Snap to center based on how many slots the item takes up
                item.RB.transform.rotation = Quaternion.Euler(0f, 0f, (float)craft.GetAxis()); // Snap to the closest axis
                item.RB.transform.localScale = new Vector3(ghostRb.transform.localScale.x * item.scalingFactor, ghostRb.transform.localScale.y * item.scalingFactor, 1f);
                if (!selectedCollider.isTrigger) selectedCollider.isTrigger = true;
                if (item.RB.bodyType != RigidbodyType2D.Kinematic) item.RB.bodyType = RigidbodyType2D.Kinematic;
                craft.CheckAllRecipes();
            }
        }
        else Debug.LogWarning("CraftingHandling DNE on grid");
    }

    private void OnDestroy()
    {
        press.Dispose();
        pointer.Dispose();
        rightHold.Dispose();
        fKey.Dispose();
    }

    // Helper function for getting mouse coords
    public Vector2 GetMouseCoords() {  return mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>()); }
}


