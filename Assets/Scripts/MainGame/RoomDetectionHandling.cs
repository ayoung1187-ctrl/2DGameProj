using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomDetectionHandling : MonoBehaviour
{
    private Camera mainCam;

    private GameObject dot;
    [SerializeField] private GameObject dotPrefab;

    [SerializeField] private InputAction pointer;
    [SerializeField] private InputAction click;

    [Header("Panel Handlers")]
    [SerializeField] GameObject notifPanel;
    [SerializeField] TextMeshPro roomNameTxt;
    [SerializeField] Vector2 OpenPosition;
    [SerializeField] Vector2 ClosePosition;

    private bool panelOpening;
    private bool panelClosing;

    [SerializeField] private float speed = 1f;
    private float panelTimer = 0f;
    [SerializeField] private float panelDuration = 5f;

    // Helps with making sure the button click vs place click goes in the right order
    public bool isPlacing;
    private bool startedPlacing;

    // Raycasting distance variables
    private Vector3 posVec;
    private float rayDistanceMax = 0f;
    private float rayDistanceMin = float.MaxValue;
    private float rayDistanceAvg;

    // Room name strings-- helps in determining if a room changed
    private string roomName = "";
    private string newRoomName = "";

    // Room trackers
    private bool tragedyFound = false;
    private bool diningFound = false;
    private bool barnFound = false;
    private bool storeFound = false;
    private bool kitchenFound = false;

    // Used for end game scoring
    static public int numOfRoomsFound = 0;

    // Game host script
    [SerializeField] private GameHost GH;

    void Awake()
    {
        mainCam = Camera.main;
        isPlacing = false;
        startedPlacing = false;

        numOfRoomsFound = 0;

        pointer.Enable();
        click.Enable();

        click.performed += OnClickPerformed;
    }

    void Update()
    {
        // If a new room is detected, go to game host comment
        if (newRoomName != roomName)
        {
            GameHostComment();
        }

        // Slide panel opem if appropriate
        if (panelOpening)
        {
            notifPanel.transform.localPosition = Vector2.MoveTowards(notifPanel.transform.localPosition, OpenPosition, speed * Time.deltaTime);

            if ((Vector2)notifPanel.transform.localPosition == OpenPosition)
            {
                panelOpening = false;
                panelTimer = panelDuration;
            }
        }

        // Keep the panel open for 5 seconds
        else if (panelTimer > 0f)
        {
            panelTimer -= Time.deltaTime;

            if (panelTimer <= 0f)
            {
                panelClosing = true;
            }
        }

        // Then close panel
        else if (panelClosing)
        {
            notifPanel.transform.localPosition = Vector2.MoveTowards(
                notifPanel.transform.localPosition,
                ClosePosition,
                speed * Time.deltaTime
            );

            // Stop when fully closed
            if ((Vector2)notifPanel.transform.localPosition == ClosePosition)
            {
                panelClosing = false;
            }
        }

        if (!isPlacing && dot == null) return;
        // If dot exists and is placed, send out raycasts
        else if (!isPlacing)
        {
            SendRaycastsOut(dot.transform.position);
        }
        // Otherwise have the dot follow the mouse
        else
        {
            startedPlacing = false;
            Vector3 mouseScreen = pointer.ReadValue<Vector2>();
            mouseScreen.z = -mainCam.transform.position.z;
            posVec = mainCam.ScreenToWorldPoint(mouseScreen);
            ContinueRoomClick();
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        // Don't do if not placing or if just placing (aka you clicked the button)
        if (!isPlacing || startedPlacing) return;

        PlaceDotClick();
    }

    public void OnRoomClick()
    {
        // Mouse to world coords
        Vector3 mouseScreen = pointer.ReadValue<Vector2>();
        mouseScreen.z = -mainCam.transform.position.z;
        posVec = mainCam.ScreenToWorldPoint(mouseScreen);

        // Destroy any other dots that exist (ideally, I would handle this by keeping track of different instances. Oh well)
        if (dot != null)
        {
            Destroy(dot);
        }

        // Spawn new dot
        dot = Instantiate(dotPrefab);
        dot.SetActive(true);
        dot.transform.position = posVec;

        // Enter placing and continue the click
        isPlacing = true;
        startedPlacing = true;

        ContinueRoomClick();
    }

    private void ContinueRoomClick()
    {
        if (isPlacing == false) return;

        dot.transform.position = posVec;
    }

    public void PlaceDotClick()
    {
        isPlacing = false;
    }


    private void SendRaycastsOut(Vector2 pos)
    {
        int missCount = 0;
        rayDistanceMax = 0f;
        rayDistanceMin = float.MaxValue;

        // Send out 16 raycasts in a full circle around the dot
        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            RaycastHit2D hit = Physics2D.Raycast(pos, direction);
            // If any ray misses, not a valid room
            if (hit.collider == null) missCount++;
            if (missCount >= 1)
            {
                NoRoomDetected();
                return;
            }

            // Track min/max room boundary distances
            if (hit.distance > rayDistanceMax)
            {
                rayDistanceMax = hit.distance;
            }
            if (hit.distance < rayDistanceMin)
            {
                rayDistanceMin = hit.distance;
            }
        }

        // Average the min/max distances (a fragile way to handle this, but what can ya do...)
        rayDistanceAvg = (rayDistanceMin + rayDistanceMax) / 2;

        // Room is detected
        CheckRoom();
    }

    private void NoRoomDetected()
    {
        GH.HostComment("Hm, no room detected here.");
        TasksHandling.isRoomCheck = false;
        Destroy(dot);
        dot = null;
    }

    private void CheckRoom()
    {
        // Check all objects inside room radius
        LayerMask mask = LayerMask.GetMask("Objects");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(dot.transform.position, rayDistanceAvg, mask);

        bool hasSheep = false;
        bool hasPizza = false;
        bool hasPizzaOven = false;

        // Look for defining objects that determine room type
        foreach (Collider2D col in colliders)
        {
            ObjectData data = col.GetComponent<ObjectData>();
            if (data == null) continue;

            if (data.objectID == "Sheep") hasSheep = true;
            if (data.objectID == "LambChopPizza") hasPizza = true;
            if (data.objectID == "PizzaOven") hasPizzaOven = true;
        }

        if (hasPizza && hasSheep)
        {
            newRoomName = "A Tragedy";
            return;
        }
        if (hasPizza)
        {
            newRoomName = "Dining Room";
            return;
        }
        if (hasSheep)
        {
            newRoomName = "Barn";
            return;
        }
        if (hasPizzaOven)
        {
            newRoomName = "Kitchen";
            return;
        }

        newRoomName = "Storage Room";

    }

    private void GameHostComment()
    {
        if (!TasksHandling.isRoomCheck) TasksHandling.isRoomCheck = true;
        roomName = newRoomName;

        // The host will only comment on a room once, which adds to unique rooms found
        if (roomName == "A Tragedy")
        {
            if (!tragedyFound)
            {
                GH.HostComment("How could you!? His mom is on that pizza!");
                numOfRoomsFound++;
                tragedyFound = true;
            }
            roomNameTxt.text = roomName;
            panelOpening = true;

            return;
        }
        if (roomName == "Dining Room")
        {
            if (!diningFound)
            {
                GH.HostComment("Mmm, smells good!... Is what I would say if I had a nose.");
                numOfRoomsFound++;
                diningFound = true;
            }
            roomNameTxt.text = roomName;
            panelOpening = true;

            return;
        }
        if (roomName == "Barn")
        {
            if (!barnFound)
            {
                GH.HostComment("I hope you know it baas on collision.");
                numOfRoomsFound++;
                barnFound = true;
            }
            roomNameTxt.text = roomName;
            panelOpening = true;
            return;
        }
        if (roomName == "Kitchen")
        {
            if (!kitchenFound)
            {
                GH.HostComment("Us sea scorpions aren't very meaty, sheep on the other hand...");
                numOfRoomsFound++;
                kitchenFound = true;
            }
            roomNameTxt.text = roomName;
            panelOpening = true;
            return;
        }

        // Default room is the storage room

        if (!storeFound)
        {
            GH.HostComment("Ah, a good ol' room of nothing.");
            numOfRoomsFound++;
            storeFound = true;
        }
        roomNameTxt.text = roomName;
        panelOpening = true;
    }

    private void OnDestroy()
    {
        click.performed -= OnClickPerformed;
        click.Dispose();
        pointer.Dispose();
    }
}
