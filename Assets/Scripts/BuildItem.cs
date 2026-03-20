/* 
 * Purpose: This class exists mostly as a helper so that the rigidbodies of the interactable objects can be easily accessed
 * 
 * Attached To: All buildable objects (rectangle, circle, square, triangle)
 * 
 * Last Edited: 3/19/26
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BuildItem : MonoBehaviour
{
    public Rigidbody2D RB { get; private set; }

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
    }

}
