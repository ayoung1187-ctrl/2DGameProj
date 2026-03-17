using UnityEngine;
using System.Collections;

public class ConvItem : MonoBehaviour
{
    
    private Vector3 screenPoint;
    private Vector3 offset;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    // Runs when object is clicked
    void OnMouseDown()
    {
        // Stores coordinates of object on screen to screenPoint
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        // Stores the difference between object coordinates and mouse coordinates such that the object won't snap to center
        offset = (transform.position) - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    // Runs every fram while holding mouse button
    void OnMouseDrag()
    {
        // Every frame, get mouse position and store
        Vector3 currScreenPoint = new Vector3(
            Input.mousePosition.x, 
            Input.mousePosition.y,
            screenPoint.z);

        // Convert mouse position to world space + offset
        Vector3 currPosition = Camera.main.ScreenToWorldPoint(currScreenPoint) + offset;
        
        // Move the object based on mouse in world space using rb s.t. it follows physics
        rb.MovePosition(currPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
