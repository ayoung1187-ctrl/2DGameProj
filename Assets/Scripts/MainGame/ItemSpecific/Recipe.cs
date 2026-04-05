using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public string resultObjectID;
    public GameObject resultGO;
    public List<RecipeIngredients> ingredients;
}

[System.Serializable]
public class RecipeIngredients
{
    public string objectID;
    public Vector2Int cell;

    public RecipeIngredients(string objectID, Vector2Int cell)
    {
        this.objectID = objectID;
        this.cell = cell;
    }
}
