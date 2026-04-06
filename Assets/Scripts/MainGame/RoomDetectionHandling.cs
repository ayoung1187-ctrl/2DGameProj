using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class RoomDetectionHandling : MonoBehaviour
{
    private GameObject dot;

    private Camera mainCam;

    [SerializeField] private GameObject dotPrefab;

    //[SerializeField] private InteractObjHandling ObjHandling;

    [SerializeField] private InputAction pointer;
    [SerializeField] private InputAction click;

    [SerializeField] GameObject notifPanel;
    [SerializeField] TextMeshPro roomNameTxt;
    [SerializeField] Vector2 OpenPosition;
    [SerializeField] Vector2 ClosePosition;

    private bool panelOpening;
    private bool panelClosing;

    [SerializeField] private float speed = 1f;
    private float panelTimer = 0f;
    [SerializeField] private float panelDuration = 5f;

    public bool isPlacing;
    private bool startedPlacing;

    private Vector3 posVec;
    private float rayDistanceMax = 0f;
    private float rayDistanceMin = float.MaxValue;
    private float rayDistanceAvg;

    private string roomName = "";
    private string newRoomName = "";

    private bool tragedyFound = false;
    private bool diningFound = false;
    private bool barnFound = false;
    private bool storeFound = false;
    private bool kitchenFound = false;

    static public int numOfRooms = 0;

    [SerializeField] private GameHost GH;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mainCam = Camera.main;
        isPlacing = false;
        startedPlacing = false;

        pointer.Enable();
        click.Enable();

        click.performed += OnClickPerformed;
    }

    // Update is called once per frame
    void Update()
    {

        if (newRoomName != roomName)
        {
            GameHostComment();
        }

        if (panelOpening)
        {
            notifPanel.transform.localPosition = Vector2.MoveTowards(notifPanel.transform.localPosition, OpenPosition, speed * Time.deltaTime);

            if ((Vector2)notifPanel.transform.localPosition == OpenPosition)
            {
                panelOpening = false;
                panelTimer = panelDuration;
            }
        }

        else if (panelTimer > 0f)
        {
            panelTimer -= Time.deltaTime;

            if (panelTimer <= 0f)
            {
                panelClosing = true;
            }
        }

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
        else if (!isPlacing)
        {
            SendRaycastsOut(dot.transform.position);
            //return;
        }
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
        if (!isPlacing || startedPlacing) return;

        PlaceDotClick();
    }

    public void OnRoomClick()
    {
        Vector3 mouseScreen = pointer.ReadValue<Vector2>();
        mouseScreen.z = -mainCam.transform.position.z;
        posVec = mainCam.ScreenToWorldPoint(mouseScreen);

        if (dot != null)
        {
            Destroy(dot);
        }

        dot = Instantiate(dotPrefab);
        dot.SetActive(true);
        dot.transform.position = posVec;

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
        numOfRooms += 1;
    }

    // I'll call myself out here. There's a bug because I overreached the wall colliders. What can ya do
    private void SendRaycastsOut(Vector2 pos)
    {
        int missCount = 0;
        rayDistanceMax = 0f;
        rayDistanceMin = float.MaxValue;
        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            RaycastHit2D hit = Physics2D.Raycast(pos, direction);
            if (hit.collider == null) missCount++;
            if (missCount >= 1)
            {
                NoRoomDetected();
                return;
            }
            if (hit.distance > rayDistanceMax)
            {
                rayDistanceMax = hit.distance;
            }
            if (hit.distance < rayDistanceMin)
            {
                rayDistanceMin = hit.distance;
            }
        }

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
        numOfRooms -= 1;
    }

    private void CheckRoom()
    {
        LayerMask mask = LayerMask.GetMask("Objects");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(dot.transform.position, rayDistanceAvg, mask);

        bool hasSheep = false;
        bool hasPizza = false;
        bool hasPizzaOven = false;

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

        if (roomName == "A Tragedy")
        {
            if (!tragedyFound)
            {
                GH.HostComment("How could you!? His mom is on that pizza!");
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
                kitchenFound = true;
            }
            roomNameTxt.text = roomName;
            panelOpening = true;
            return;
        }

        if (!storeFound)
        {
            GH.HostComment("Ah, a good ol' room of nothing.");
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
