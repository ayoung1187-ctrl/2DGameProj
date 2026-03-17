/* 
 * Purpose: This MonoBehavior class has the purpose of deciding how the handling of all interactable, buildable objects works in this game.
 * Attached To: Main Camera
 * 
 * Macro-Scale: The objects (at this point in development) should be click-and-draggable using the left-mouse button.
 *              When clicked on, the selected object should pivot from the cursor. Appropriate collision should be handled by Unity.
 * 
 * Code-Scale: This class takes advantage of Unity's input system package and uses it alongside Collider2D and TargetJoint2D objects.
 *             Add to this
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class InteractObjHandling : MonoBehaviour
{
    /* 
     * Field Initializations
     */

    // Buttons declared in inspector
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction pointer;

    private Camera mainCam;

    private Rigidbody2D selectedRb;
    private TargetJoint2D selectedJoint;

    private Vector2 grabOffset;
    private bool isDragging = false;



    /*
     * Awake() is called after a prefab is instantiated.
     * Desc: This function enables input actions and defines responses to clicking and letting go.
     */
    private void Awake()
    {
        mainCam = Camera.main;

        press.Enable();
        pointer.Enable();

        press.performed += _ => StartDrag();
        press.canceled += _ => StopDrag();
    }



    /*
     * Update() is called once per frame.
     * Desc: If nothing is being dragged, do nothing. Else, redefine the joint target every frame to match mouse coords.
     */
    private void Update()
    {
        if (selectedJoint == null) return;

        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        selectedJoint.target = mouseWorld;
    }



    /*
     * StartDrag() is called when a left-click has been performed.
     * Desc: Finds the 2D collider that intersects with the current mouse position on a pressed left-click.
     *       It appropriately checks if a collider at this point exists or if it has a BuildItem script attached.
     *       If so, it extracts the Rigidbody2D component from the grabbed object and creates a TargetJoint2D component for it.
     *       It will then set the anchor and target point equal to the mouse position and joint attributes are defined.
     */
    private void StartDrag()
    {
        // Convert from screen mouse position, to mouse position within the world
        Vector2 mouseWorld = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null) return;

        BuildItem item = hit.GetComponent<BuildItem>();
        if (item == null) return;

        selectedRb = item.RB;

        // Since the joints are added and destroyed during left-clicks, this won't usually do anything, but I'll keep it for safety to avoid multiple joints
        selectedJoint = selectedRb.GetComponent<TargetJoint2D>();
        if (selectedJoint == null)
        {
            selectedJoint = selectedRb.gameObject.AddComponent<TargetJoint2D>();
        }

        // Make it so the joint is customizable
        selectedJoint.autoConfigureTarget = false;
        selectedJoint.target = mouseWorld; // in world coords

        Vector2 grabPoint = selectedRb.transform.InverseTransformPoint(mouseWorld);
        selectedJoint.anchor = grabPoint; // in screen coords

        // Basically, these determine how stiff or loose the "spring" will be
        selectedJoint.maxForce = 1000f;
        selectedJoint.dampingRatio = 1.0f;
        selectedJoint.frequency = 5f;
    }



    /*
     * StopDrag() is called when the left-click has been released.
     * Desc: When the left-click is lifted, destroy the created joint if applicable. Get rid of that object's info.
     */
    private void StopDrag()
    {
        if (selectedJoint != null)
        {
            Destroy(selectedJoint);
            selectedJoint = null;
        }

        selectedRb = null;
    }

    /*
     * OnDestroy() is called after an object is destroyed.
     */
    private void OnDestroy()
    {
        press.Dispose();
        pointer.Dispose();
    }
}