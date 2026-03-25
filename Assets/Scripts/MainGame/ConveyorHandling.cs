/* 
 * Purpose: This class will handle the behavior of the conveyor belt and the objects that are on it.
 * 
 * Attached To: ItemsAndConveyor
 * 
 * Class Function: After a 2 second delay, begins spawning objects to be carried down the conveyor belt.
 *                 SpawnItems() will randomly select an object to spawn each 5 seconds.
 *                 Then, objects will move right to left until they either exit the boundaries or are dragged onto the building range,
 *                 with which they are no longer considered by this class.
 * 
 * Last Edited: 3/19/26
 */

using System.Collections.Generic;
using UnityEngine;

public class ConveyorHandling : MonoBehaviour
{
    /*
     * Field Variables
     */

    // Bounds of buidable floor and left side of the screen
    static public float boundLeft = -15f;
    static public float boundFloor = -2.5f;

    // GameObjects to be spawned
    [SerializeField] private GameObject cosmicCircle;
    [SerializeField] private GameObject steelBeam;
    [SerializeField] private GameObject woodenCrate;
    [SerializeField] private GameObject decorativeTriangle;
    [SerializeField] private GameObject sheep;

    // Other variables
    [SerializeField] private float conveyorSpeed = 1f;
    private Vector3 itemSpawnPoint;
    private List<GameObject> spawnedItems = new List<GameObject>();
    private GameObject itemInstance;

    public TimerS timer;

    void Start()
    {
        itemSpawnPoint = new Vector3(11, -4.2f, 0);
        Invoke("SpawnItems", 1f);
        Invoke("StartTimer", 1f);
    }

    /*
     * Update(): Makes all spawned items in list move right to left, continuously checking if any are out of bounds or in building area.
     *           If the item enters the building area, check which item and add its price to the amount spent.
     */
    void Update()
    {
        // If list is empty
        if (spawnedItems.Count <= 0) return;

        foreach (GameObject item in spawnedItems)
        {
            Vector2 itemPos = item.transform.position;
            itemPos.x -= conveyorSpeed * Time.deltaTime;
            item.transform.position = itemPos;
        }

        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (spawnedItems[i] == null) continue; 

            BuildItem item = spawnedItems[i].GetComponent<BuildItem>();

            if (item.GetIsBought())
            {
                SpentBudget.spentMoney += item.cost;
                spawnedItems.RemoveAt(i);
            }
            else if (spawnedItems[i].transform.position.x < boundLeft)
            {
               Destroy(spawnedItems[i]);
                spawnedItems.RemoveAt(i);
            }
        }
    }

    /*
     * SpawnItems(): Randomly decides which item gets instantiated, and then calls itself with a 5 second delay.
     *               Not smart to use a switch case, cascading if's would be better, but switches make me happy so it's staying for now.
     */
    void SpawnItems()
    {
        int randNum = Random.Range(0, 5);
        
        switch (randNum)
        {
            case 0:
                itemInstance = Instantiate<GameObject>(decorativeTriangle);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 1:
                itemInstance = Instantiate<GameObject>(woodenCrate);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 2:
                itemInstance = Instantiate<GameObject>(steelBeam);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 3:
                itemInstance = Instantiate<GameObject>(cosmicCircle);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 4:
                itemInstance = Instantiate<GameObject>(sheep);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            default:
                return;
        }

        Invoke("SpawnItems", 5f);
    }

    void StartTimer()
    {
        timer.enabled = true;
    }
}
