/* 
 * Purpose: Holds data on if this cell is occupied, and if so by what.
 * 
 * Attached To: CraftingGrid
 */

using System.Collections.Generic;
using UnityEngine;

public class CraftingHandling : MonoBehaviour
{
    /*
     * Field Variables
     */

    // 3x3 crafting grid storing references to placed objects
    private ObjectData[,] gridMat = new ObjectData[3,3];

    [Header("Grid Info")]
    [SerializeField] private RectTransform grid;
    [SerializeField] private GameObject gridDimensions;

    [Header("Craft Button and Pull Down Window Script")]
    [SerializeField] private CraftButton craftButton;
    [SerializeField] private PullDownWindowGameObject pullDown;

    [Header("List of All Existing Recipes")]
    [SerializeField] private List<Recipe> allRecipes;

    [Header("Game Host Script")]
    [SerializeField] private GameHost GH;

    // Grid Variables
    private int grabbedCellEquivalent = -1;
    private int axis;
    private int placeCol;
    private int placeRow;

    private List<Vector2Int> revisedCells;

    // Other variables
    [HideInInspector] public bool fireButtonIsLit = false;
    private GameObject craftableItem;
    private Vector3 instantiatePos;

    // Booleans for end game scoring
    static public int numCraftedItems = 0;
    static public int craftedItemsValue = 0;

    void Start ()
    {
        numCraftedItems = 0;
        craftedItemsValue = 0;
        grid = GetComponent<RectTransform>();
    }

    /*
     * Attempts to place an object into the grid based on mouse position and shape.
     */
    public bool TryPlaceObject(Vector2 mouseCoords, Quaternion objRotation, ObjectData inputObj, Vector2Int grabbedCell)
    {
        if (inputObj == null) return false;

        // Reset variables
        grabbedCellEquivalent = -1;
        revisedCells = null;
        axis = 0;


        // Find the primary slot which the mouse is hovering over
        placeCol = (int)(mouseCoords.x - grid.anchoredPosition.x); // 2.405 -> 2
        placeRow = (int)(mouseCoords.y - grid.anchoredPosition.y); // 1.205 -> 1

        // Out of bounds check
        if (placeRow < 0 || placeRow > 2 || placeCol < 0 || placeCol > 2)
        {
            return false;
        }

        // If the object you're trying to place has a dimension of 1x1, simply check the primary slot
        if (inputObj.shapeInCells.Count <= 1)
        { 
            if (gridMat[placeCol, placeRow] == null)
            {
                gridMat[placeCol, placeRow] = inputObj;
                inputObj.occupiedCells.Add(new Vector2Int(placeCol, placeRow));
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

        // Find which cell in the object was grabbed
        for (int i = 0; i < shapeCells.Count; i++)
        {
            if (shapeCells[i] == grabbedCell)
            {
                grabbedCellEquivalent = i;
            }
        }

        // If no grabbed cell, do nothing
        if (grabbedCellEquivalent == -1)
        {
            return false;
        }

        // Rotate shape depending on axis
        switch (axis)
        {
            case 90:
                for (int i = 0; i < shapeCells.Count; i++)
                {
                    shapeCells[i] = new Vector2Int(shapeCells[i].y, shapeCells[i].x);
                }
                grabbedCell = shapeCells[grabbedCellEquivalent];
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
        revisedCells = GetRelativeCells(GetHoveredCell(), grabbedCell, shapeCells);

        // For each slot that needs to be checked
        foreach (Vector2Int cell in revisedCells)
        {
            // If the object goes out of grid bounds, return failure
            if (cell.x < 0 || cell.x > 2 || cell.y < 0 || cell.y > 2)
            {
                return false;
            }

            // If the necessary space is already occupied, return failure
            if (gridMat[cell.x, cell.y] != null)
            {
                return false;
            }
        }

        // If placement is "success," then for each cell checked, make it occupied by current object
        foreach (Vector2Int cell in revisedCells)
        {
            gridMat[cell.x, cell.y] = inputObj;

        }

        inputObj.occupiedCells = new List<Vector2Int>(revisedCells);

        CheckAllRecipes();

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
            Vector2Int relative = cell - grabbedCell; // Offset relative to grabbed cell
            result.Add(hoveredCell + relative); // Apply offset to hovered cell
        }

        return result;
    }

    public int GetAxis() { return axis; }

    public Vector2Int GetHoveredCell() { return new Vector2Int(placeCol, placeRow); }

    /*
     * Returns the world position of the center of a grid cell
     */
    public Vector2 GetCellCenterLocal(int col, int row)
    {
        Vector2 center = gridDimensions.transform.position;
        Vector2 widthHeight = new Vector2(gridDimensions.transform.localScale.x / 3f, gridDimensions.transform.localScale.y / 3f);

        Vector2 localPos = center + new Vector2((col - 1) * widthHeight.x, (row - 1) * widthHeight.y);
        return localPos;
    }

    /*
     * Finds the center position of all occupied cells (used for snapping objects)
     */
    public Vector2 FindCenterSnap()
    {
        Vector2 posAdded = new Vector2(0f, 0f);

        // If no complex dimensions, just return hovered cell center
        if (revisedCells == null)
        {
            return GetCellCenterLocal(placeCol, placeRow);
        }

        // Average all occupied cell positions
        for (int i = 0; i < revisedCells.Count; i++)
        {
            posAdded += GetCellCenterLocal(revisedCells[i].x, revisedCells[i].y);
        }

        return new Vector2(posAdded.x / revisedCells.Count, posAdded.y / revisedCells.Count);
    }

    /* 
     * Clears a single slot
     */
    public void ClearSlots(Vector2Int slot)
    {
        gridMat[slot.x, slot.y] = null;
        CheckAllRecipes();
    }

    /*
     * Clears entire grid and destroys objects
     */
    private void ClearAllSlots()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (gridMat[x, y] != null)
                {
                    gridMat[x, y].occupiedCells.Clear();
                    Destroy(gridMat[x, y].gameObject);
                    gridMat[x, y] = null;
                }
            }
        }
    }

    public void ForceOccupySlots(Vector2Int slot, ObjectData item)
    {
        gridMat[slot.x, slot.y] = item;
        CheckAllRecipes();
    }

    /*
     * Checks if current grid matches a specific recipe
     */
    public bool CheckRecipe(Recipe recipe)
    {

        List<RecipeIngredients> gridState = new List<RecipeIngredients>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (gridMat[x, y] != null)
                {
                    string id = gridMat[x, y].objectID;
                    Vector2Int coords = new Vector2Int(x, y);
                    gridState.Add(new RecipeIngredients(id, coords));
                }
            }
        }

        if (gridState.Count != recipe.ingredients.Count) // Must match ingredient count
        {
            return false;
        }

        // Normalize
        Vector2Int gridMin = gridState[0].cell;
        foreach (var e in gridState) gridMin = Vector2Int.Min(gridMin, e.cell);

        Vector2Int recipeMin = recipe.ingredients[0].cell;
        foreach (var e in recipe.ingredients) recipeMin = Vector2Int.Min(recipeMin, e.cell);

        // Compare normalized positions and IDs
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            Vector2Int normalizedRecipeCell = recipe.ingredients[i].cell - recipeMin;
            bool found = false;

            for (int j = 0; j < gridState.Count; j++)
            {
                Vector2Int normalizedGridCell = gridState[j].cell - gridMin;

                if (normalizedGridCell == normalizedRecipeCell && gridState[j].objectID == recipe.ingredients[i].objectID)
                {
                    found = true;
                    break;
                }
            }

            if (!found) return false;
        }

        return true;
    }

    /*
     * Checks all recipes to see if any match current grid
     */
    public void CheckAllRecipes()
    {
        foreach (Recipe recipe in allRecipes)
        {
            if (CheckRecipe(recipe))
            {
                craftableItem = recipe.resultGO;
                if (!fireButtonIsLit) // Doing these if statements to prevent unnecessary repitition
                {
                    fireButtonIsLit = true;
                    craftButton.ChangeButtonImageOn();
                }
                return;
            }
        }

        // If no recipes match
        craftableItem = null;
        if (fireButtonIsLit)
        {
            fireButtonIsLit = false;
            craftButton.ChangeButtonImageOff();
        }
    }

    /*
     * Called when player presses craft button
     */
    public void CraftButtonIsPressed()
    {
        // If the button is grayed out, do nothing
        if (!fireButtonIsLit)
        {
            return;
        }

        // else, delete the objects on the grid, deactivate the grid, and instantiate the resulting object where the grid was.
        ClearAllSlots();
        grid.gameObject.SetActive(false);
        craftButton.ChangeButtonImageOff();
        fireButtonIsLit = false;

        // Update task system
        if (!TasksHandling.isCraftCheck)
        {
            TasksHandling.isCraftCheck = true;
        }

        // Spawn crafted item
        instantiatePos = gridDimensions.transform.position;
        GameObject result = Instantiate(craftableItem);
        ObjectData resultOD = result.GetComponent<ObjectData>();
        resultOD.SetIsBought(true);
        result.transform.position = instantiatePos;

        // Host comment
        GH.HostComment(resultOD.hostComment);

        // Update scores
        numCraftedItems++;
        craftedItemsValue += resultOD.cost;
    }

    // Returns whether grid UI is active
    public bool IsGridActive()
    {
        return grid.gameObject.activeSelf;
    }

    // Resets grid UI and toggles dropdown
    public void ResetGrid()
    {
        grid.gameObject.SetActive(true);
        craftableItem = null;
        pullDown.Toggle();
    }
}
