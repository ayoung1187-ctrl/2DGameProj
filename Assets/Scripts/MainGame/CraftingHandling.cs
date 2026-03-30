/* 
 * Purpose: Holds data on if this cell is occupied, and if so by what.
 * 
 * Attached To: CraftingGrid
 * 
 * Last Edited: 3/29/26
 */

using System.Collections.Generic;
using UnityEngine;

public class CraftingHandling : MonoBehaviour
{
    private ObjectData[,] gridMat = new ObjectData[3,3];

    //private int objHeight;
    //private int objWidth;

    [SerializeField] private RectTransform grid;
    [SerializeField] private float gridBoundTolerance = 0.2f;

    private int grabbedCellEquivalent = -1;
    private int axis;
    private int placeCol;
    private int placeRow;


    void Start ()
    {
        grid = GetComponent<RectTransform>();
    }

    public bool TryPlaceObject(Vector2 mouseCoords, Quaternion objRotation, ObjectData inputObj, Vector2Int grabbedCell)
    {
        if (inputObj == null) return false;

        Debug.Log("Entered TryPlaceObject");

        // Find the primary slot which the mouse is hovering over
        placeCol = (int)(mouseCoords.x - grid.anchoredPosition.x); // 2.405 -> 2
        Debug.Log(mouseCoords.x + "-" + grid.anchoredPosition.x + "=" + placeCol);
        placeRow = (int)(mouseCoords.y - grid.anchoredPosition.y); // 1.205 -> 1
        Debug.Log(mouseCoords.y + "-" + grid.anchoredPosition.y + "=" + placeRow);

        if (placeRow < 0 || placeRow > 2 || placeCol < 0 || placeCol > 2)
        {
            Debug.Log("Anchor slot is out of bounds.");
            return false;
        }


        // If the object you're trying to place has a dimension of 1x1, simply check the primary slot
        if (inputObj.shapeInCells.Count <= 1)
        { 
            if (gridMat[placeCol, placeRow] == null)
            {
                Debug.Log("Placement Successful");
                gridMat[placeCol, placeRow] = inputObj;
                return true;
            }
            else
            {
                return false;
            }
        }

        if (inputObj.shapeInCells == null) return false;

        List<Vector2Int> shapeCells = new List<Vector2Int>(inputObj.shapeInCells);

        // If the object has a dimension different than 1x1...

        // The object can only be rotated on the axes for the grid, so 90 degree angles
        axis = (int)Mathf.Round(objRotation.eulerAngles.z / 90f) * 90; // should return 0, 90, 180, or 270
        Debug.Log("Axis:" + axis);

        for (int i = 0; i < shapeCells.Count; i++)
        {
            if (shapeCells[i] == grabbedCell)
            {
                grabbedCellEquivalent = i;
            }
        }

        if (grabbedCellEquivalent == -1)
        {
            Debug.LogError("grabbedCell was not found in shapeCells");
            return false;
        }

        // For steel beam that originally (0,0), (1,0), (2,0). 90 -> (0,0), (0,1), (0,2). 180 -> (2,0), (1,0), (0,0). 270 -> (0,2), (0,1), (0,0)
        // For sheep with 2x2 grid, since it's symmetrical, you don't need to do anything
        switch (axis)
        {
            case 90:
                for (int i = 0; i < shapeCells.Count; i++)
                {
                    shapeCells[i] = new Vector2Int(shapeCells[i].y, shapeCells[i].x);
                }
                grabbedCell = shapeCells[grabbedCellEquivalent];
                Debug.Log("new: grabbedCell -> " + grabbedCell);
                break;
            case 180:
                shapeCells.Reverse();
                grabbedCell = shapeCells[grabbedCellEquivalent];
                break;
            case 270:
                for (int i = 0; i < shapeCells.Count; i++)
                {
                    shapeCells[i] = new Vector2Int(shapeCells[i].y, shapeCells[i].x);
                }
                shapeCells.Reverse();
                grabbedCell = shapeCells[grabbedCellEquivalent];
                break;
        }



        // Figure out, based on your primary slot and the relative cells of the shape, what other cells the program should be checking for vacancy
        List<Vector2Int> revisedCells = GetRelativeCells(GetHoveredCell(), grabbedCell, shapeCells);

        // For each slot that needs to be checked
        foreach (Vector2Int cell in revisedCells)
        {
            // If the object goes out of grid bounds, return failure
            if (cell.x < 0 || cell.x > 2 || cell.y < 0 || cell.y > 2)
            {
                Debug.Log("Out of bounds at {" + cell.x + ", " + cell.y + "}");
                return false;
            }

            // If the necessary space is already occupied, return failure
            if (gridMat[cell.x, cell.y] != null)
            {
                Debug.Log("Occupied at  " + cell.x + cell.y);
                return false;
            }
        }

        // If placement is "success," then for each cell checked, make it occupied by current object
        foreach (Vector2Int cell in revisedCells)
        {
            gridMat[cell.x, cell.y] = inputObj;
        }

        Debug.Log("Placement succeeded.");
        return true;
    }


    /* 
     * Params:             hoveredCell -> The primary slot you're trying to place into
     *                     grabbedCell -> The relative cell on the shape you grabbed
     *                     shapeCells  -> The fell set of relative cells for this shape
     * GetRelativeCells(): For every cell in the full set for this shape, subtract its value from the primary slot value to accommodate for displacement.
     *                     Then, after getting the relative cells, find necessary cells to check.
     */
    public List<Vector2Int> GetRelativeCells(Vector2Int hoveredCell, Vector2Int grabbedCell, List<Vector2Int> shapeCells)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int cell in shapeCells)
        {
            Vector2Int relative = cell - grabbedCell; // (0,0) - (1,0)... = (-1, 0), (0, 0), (1, 0)
            // at 90: (0,0), (0,1), (0,2) - (0,2) = (0,-2), (0,-1), (0,0)
            result.Add(hoveredCell + relative); // (1,1) + (-1,0)... = (0,1), (1,1), (2,1)
            // at 90: (1,2) + (0,-2), (0,-1), (0,0) = (1,0), (1, 1), (1, 2)
        }

        return result;
    }

    public int GetAxis() { return axis; }

    public Vector2Int GetHoveredCell() { return new Vector2Int(placeCol, placeRow); }

    public Vector3 GetCellCenterLocal(int col, int row)
    {
        float cellWidth = (grid.rect.width - gridBoundTolerance) / 3f;
        float cellHeight = (grid.rect.height - gridBoundTolerance) / 3f;

        float x = grid.rect.xMin + cellWidth * (col + 0.5f);
        float y = grid.rect.yMin + cellHeight * (row + 0.5f);

        return grid.TransformPoint(new Vector3(x, y, 0f));
    }
}
