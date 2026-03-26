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
 * Last Edited: 3/25/26
 *             
 * Edit 3/19/26: I realized I don't know if I want joint physics for dragging, so I updated this class to have drag be kinematic.
 *               The plan is to make the objects kinematic whilst on the conveyor belt and during drag, and dynamic otherwise.
 */

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

    // Real object variables
    private Collider2D selectedCollider;
    private ObjectData item;

    // Ghost variables
    private GameObject ghostInstance;
    private Rigidbody2D ghostRb;
    private Collider2D ghostCollider;

    [Header("Object Rotation Speed")]
    [SerializeField] private float rotationSpeed = 45f;

    // Other variables
    private Camera mainCam;
    private Vector2 mouseWorldCoords;
    private Vector2 mouseOffset;
    private Vector2 trackItem;
    private bool isDragging = false;

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

        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        ContinueDrag();
    }

    /*
     * StartDrag(): Find the object that the mouse is clicking on and create a "ghost" of it.
     */
    public void StartDrag()
    {
        // Convert from screen mouse position, to mouse position within the world
        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        LayerMask objectLayer = LayerMask.GetMask("Objects"); // Make it so that this script will only detect objects
        selectedCollider = Physics2D.OverlapPoint(mouseWorldCoords, objectLayer);

        // If no object exists here
        if (selectedCollider == null) return;

        item = selectedCollider.GetComponent<ObjectData>();
        if (item == null || item.RB == null) return;

        isDragging = true;

        ghostInstance = Instantiate<GameObject>(item.gameObject);
        SpriteRenderer sr = ghostInstance.GetComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 1f, 0.5f);

        ObjectData ghostBI = ghostInstance.GetComponent<ObjectData>();
        ghostCollider = ghostBI.GetComponent<Collider2D>();

        ghostRb = ghostBI.RB;

        // This helps prevent unwanted movement when dragging
        ghostRb.angularVelocity = 0;
        ghostRb.linearVelocity = Vector2.zero;

        Vector2 ghostItemPos2D = new Vector2(ghostBI.RB.transform.position.x, ghostBI.RB.transform.position.y);
        mouseOffset = ghostItemPos2D - mouseWorldCoords;

        // This is used in the case that you release a bought item to the conveyor belt, it rejects it
        trackItem = ghostBI.GetCurPosition();

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
            ghostRb.rotation += rotationSpeed * Time.deltaTime;
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
            if (mouseWorldCoords.y > ConveyorHandling.boundFloor) // If you moved ghost to an area within the building range
            {
                if (!item.GetIsBought()) item.SetIsBought(true); // If item is not already bought, make it so

                item.RB.transform.position = ghostRb.transform.position; // Move real object to ghost object position, rotation, and scale
                item.RB.transform.rotation = ghostRb.transform.rotation;
                item.RB.transform.localScale = ghostRb.transform.localScale;
                item.RB.bodyType = RigidbodyType2D.Dynamic; // Make real object dynamic
            }
            else if (item.GetIsBought()) // If you moved ghost to conveyor belt
            { 
                item.RB.transform.position = trackItem; // If it is bought and you drag it back, force it back to build range (you cannot re-place items on conveyor)
                item.RB.bodyType = RigidbodyType2D.Dynamic; // Make real object dynamic

            }

            Destroy(ghostInstance); // Destroy the ghost
        }

        isDragging = false;
        ghostRb = null;
        selectedCollider = null;
        item = null;
    }

    private void OnDestroy()
    {
        press.Dispose();
        pointer.Dispose();
        rightHold.Dispose();
    }
}