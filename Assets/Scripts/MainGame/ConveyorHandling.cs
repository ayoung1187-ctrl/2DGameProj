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
 * Last Edited: 3/25/26
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

    [Header("GameObjects to be spawned")]
    [SerializeField] private GameObject cosmicCircle;
    [SerializeField] private GameObject steelBeam;
    [SerializeField] private GameObject woodenCrate;
    [SerializeField] private GameObject decorativeTriangle;
    [SerializeField] private GameObject sheep;

    [SerializeField] private GameHost GH;

  

    [Header("Conveyor Belt Speed")]
    [SerializeField] private float conveyorSpeed = 2f;
    [SerializeField] private float spawnSpeed = 3f;

    // Other variables
    private Vector3 itemSpawnPoint;
    private List<GameObject> spawnedItems = new List<GameObject>();
    private GameObject itemInstance;

    public Timer timer;

    /*
     * Start(): Defines object spawn point and starts the game (conveyor belt and timer) after a delay
     */

    void Start()
    {
        itemSpawnPoint = new Vector3(11, -4.2f, 0);
        Invoke("SpawnItems", 1f);
        Invoke("StartTimer", 1f);
        GH.HostComment("And so... LET THE GAME BEGIN!");
    }

    /*
     * Update(): Makes all spawned items in list move right to left, continuously checking if any are bought (determined by InteractObjHandling), or if it has exited screen left.
     *           In both cases, if true, the object is removed from the list.
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

            ObjectData item = spawnedItems[i].GetComponent<ObjectData>();

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
     *               I had once said that using a switch case is not the best implementation.
     *               That is possibly still true.
     *               But it makes me happy so it's staying (switches need a little love).
     */
    void SpawnItems()
    {
        int randNum = Random.Range(0, 9);
        //int randNum = Random.Range(0, 2);

        switch (randNum)
        {
            case 0:
            case 1:
                itemInstance = Instantiate<GameObject>(decorativeTriangle);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 2:
            case 3:
                itemInstance = Instantiate<GameObject>(woodenCrate);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 4:
            case 5:
            case 6:
                itemInstance = Instantiate<GameObject>(steelBeam);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 7:
                itemInstance = Instantiate<GameObject>(cosmicCircle);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 8:
                itemInstance = Instantiate<GameObject>(sheep);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            default:
                return;
        }

        /*switch (randNum)
        {
            case 0:
                itemInstance = Instantiate<GameObject>(decorativeTriangle);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
            case 1:
                itemInstance = Instantiate<GameObject>(steelBeam);
                itemInstance.transform.position = itemSpawnPoint;
                spawnedItems.Add(itemInstance);
                break;
        }*/

            Invoke("SpawnItems", spawnSpeed);
    }

    /*
     * StartTimer(): The variable timer refers to the TimerS script, which is deactivated on start.
     *               This simply enables the script, starting the timer.
     */
    void StartTimer()
    {
        timer.enabled = true;
    }
}
