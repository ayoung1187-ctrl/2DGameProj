/* 
 * Purpose: This MonoBehavior class has the purpose of deciding how the handling of all interactable, buildable objects works in this game.
 * 
 * Attached To: Main Camera
 *             
 * Class Function: This class will respond to a left-click by finding what object, if any, is overlapping with the map at that point.
 *                 If such an object exists, it'll change it's body type to kinematic and change its position to match the mouse as it moves.
 *                 Additionally, if right-click or R is held during this, the object will rotate at a constant speed.
 *                 When the left-click is released, the object's body type will change back to dynamic and the reference will be cleared.
 *             
 * Last Edited: 3/19/26
 *             
 * Edit 3/19/26: I realized I don't know if I want joint physics for dragging, so I updated this class to have drag be kinematic.
 *               The plan is to make the objects kinematic whilst on the conveyor belt and during drag, and dynamic otherwise
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class InteractObjHandling : MonoBehaviour
{
    /* 
     * Field Initializations
     */

    // Actions, buttons declared in inspector
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction pointer;
    [SerializeField] private InputAction rightHold;

    // Other variables
    private Camera mainCam;
    private Rigidbody2D selectedRb;
    private Collider2D selectedCollider;
    private BuildItem item;

    [SerializeField] private float rotationSpeed = 45f;
    private bool isDragging = false;
    private Vector2 mouseWorldCoords;
    private Vector2 mouseOffset;
    private Vector2 trackItem;

    /*
     * Awake(): This function enables input actions and defines responses to clicking and letting go.
     */
    private void Awake()
    {
        mainCam = Camera.main;

        press.Enable();
        pointer.Enable();
        rightHold.Enable();

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
     * StartDrag(): Find the object that the mouse is clicking on and associated components, change necessary parameters, and call ContinueDrag().
     *              StartDrag() and ContinueDrag() are separated to prevent contrived reassignments every frame.
     */
    public void StartDrag()
    {
        // Convert from screen mouse position, to mouse position within the world
        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        selectedCollider = Physics2D.OverlapPoint(mouseWorldCoords);

        // If no object exists here
        if (selectedCollider == null) return;

        item = selectedCollider.GetComponent<BuildItem>();
        if (item == null) return;
        if (item.RB == null) return;

        isDragging = true;
        selectedRb = item.RB;

        // This helps prevent unwanted movement when dragging
        selectedRb.angularVelocity = 0;
        selectedRb.linearVelocity = Vector2.zero;

        Vector2 itemPos2D = new Vector2(item.RB.transform.position.x, item.RB.transform.position.y);
        mouseOffset = itemPos2D - mouseWorldCoords;

        trackItem = item.GetCurPosition();

        ContinueDrag();
    }

    /*
     * ContinueDrag(): During the drag, make the object kinematic if it isn't already, and appropriately change its position.
     *                 Also, if the player holds right-click or R during this, rotate the object.
     */
    private void ContinueDrag()
    {
        if (selectedRb == null || selectedCollider == null)
        {
            return;
        }

        // Make the object kinematic if it isn't already
        if (selectedRb.bodyType != RigidbodyType2D.Kinematic)
        {
            selectedRb.bodyType = RigidbodyType2D.Kinematic;
        }

        selectedCollider.enabled = false;

        selectedRb.transform.position = mouseWorldCoords + mouseOffset;

        // Right-click or R is pressed
        if (rightHold.inProgress)
        {
            selectedRb.rotation += rotationSpeed * Time.deltaTime;
        }
    }

    /*
     * StopDrag(): When the left-click is lifted, if it is not already dynamic and is within the building range, change the body type to dynamic and empty selectedRb.
     */
    private void StopDrag()
    {
        if (selectedRb != null)
        {
            if (/*selectedRb.transform.position.y*/ mouseWorldCoords.y > ConveyorHandling.boundFloor)
            {
                selectedRb.bodyType = RigidbodyType2D.Dynamic;
            }
            else if (item != null && item.GetIsBought())
            {
                selectedRb.transform.position = trackItem;
            }
        }

        if (selectedCollider != null && !selectedCollider.enabled)
        {
            selectedCollider.enabled = true;
        }

        isDragging = false;
        selectedRb = null;
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