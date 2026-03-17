// This script helps prevent reduntandly calling GetComponent<RigidBody2D> after the object is found in InteractObjHandling

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
