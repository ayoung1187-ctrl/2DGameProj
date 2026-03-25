using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class TaskListHandling : MonoBehaviour // this can handle both panels so...
{
    [SerializeField] private GameObject taskList;
    [SerializeField] private GameObject text;
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction pointer;
    [SerializeField] private int speed = 1;

    private RectTransform rectPanel;
    private RectTransform rectText;
    private ButtonType selectedType;
    private TextMeshPro tasks;
    private Collider2D selectedCollider;
    private Vector2 mouseWorldCoords;
    private Camera mainCam;
    private GameObject triangleButton;

    private bool taskIsToggled = false;
    private bool craftIsToggled = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;
        tasks = text.GetComponent<TextMeshPro>();
        rectPanel = taskList.GetComponent<RectTransform>();
        rectText = text.GetComponent<RectTransform>();

        press.Enable();
        pointer.Enable();

        press.performed += _ => MouseClicked();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 panelPos = rectPanel.anchoredPosition;
        Vector2 textPos = rectText.anchoredPosition;

        /*if (craftIsToggled)
        {
            panelPos.y = Mathf.MoveTowards(panelPos.y, -534f, speed * Time.deltaTime);
        }
        if (!craftIsToggled)
        {
            panelPos.y = Mathf.MoveTowards(panelPos.y, -538f, speed * Time.deltaTime);
        }*/
        if (taskIsToggled)
        {
            panelPos.y = Mathf.MoveTowards(panelPos.y, 0f, speed * Time.deltaTime);
            textPos.y = Mathf.MoveTowards(textPos.y, 331f, speed * Time.deltaTime);
        }
        else if (!taskIsToggled)
        {
            panelPos.y = Mathf.MoveTowards(panelPos.y, 587f, speed * Time.deltaTime);
            textPos.y = Mathf.MoveTowards(textPos.y, 918f, speed * Time.deltaTime);
        }

        rectPanel.anchoredPosition = panelPos;
        rectText.anchoredPosition = textPos;
    }

    void MouseClicked()
    {
        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        LayerMask buttonLayer = LayerMask.GetMask("UI"); // Make it so that this script will only detect objects
        selectedCollider = Physics2D.OverlapPoint(mouseWorldCoords, buttonLayer);

        // If no object exists here
        if (selectedCollider == null) return;

        selectedType = selectedCollider.GetComponent<ButtonType>();
        if (selectedType == null) return;
        if (selectedType.type == ButtonType.buttonType.Craft)
        {
            Debug.Log("Selected Craft");
            craftIsToggled = !craftIsToggled;
            FlipTriangleButton();
        }
        else
        {
            taskIsToggled = !taskIsToggled;
            FlipTriangleButton();
        }
    }

    void FlipTriangleButton()
    {
        triangleButton = selectedCollider.gameObject;
        Vector2 yComp = triangleButton.transform.localScale;
        yComp.y = -triangleButton.transform.localScale.y;
        triangleButton.transform.localScale = yComp;
    }
}
