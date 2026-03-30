/* 
 * Purpose: This class exists as a data sheet for each object. Also handles when the object makes a noise.
 * 
 * Attached To: All buildable objects with a unique footprint's child colliders (SteelBeam, Sheep)
 * 
 * Last Edited: 3/29/26
 */

using UnityEngine;

public class ObjectShapeCell : MonoBehaviour
{
    [SerializeField] private Vector2Int localCell;
    public Vector2Int LocalCell => localCell;
}