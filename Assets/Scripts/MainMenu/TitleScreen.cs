/* 
 * Purpose: This is the general game manager that handles the game state and win / lose conditions. Overarching game rules.
 * 
 * Attached To: ShelterAndSpellGM
 * 
 * Class Function:
 * 
 * Last Edited: 3/19/26
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private InputAction press;
    [SerializeField] private InputAction pointer;

    // Other variables
    private Camera mainCam;
    private Collider2D selectedCollider;

    private Vector2 mouseWorldCoords;
    GameObject selectedButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;

        press.Enable();
        pointer.Enable();

        press.performed += _ => MouseClicked();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void MouseClicked()
    {
        // Convert from screen mouse position, to mouse position within the world
        mouseWorldCoords = mainCam.ScreenToWorldPoint(pointer.ReadValue<Vector2>());
        selectedCollider = Physics2D.OverlapPoint(mouseWorldCoords);

        // If no object exists here
        if (selectedCollider == null) return;

        selectedButton = selectedCollider.gameObject;
        if (selectedButton == null) return;
        if (selectedButton.name == "StartGame")
        {
            SceneManager.LoadScene(1);
        }
        if (selectedButton.name == "Quit")
        {
            Debug.Log("Game Quit");
            Application.Quit();
        }
        
    }

    private void OnDestroy()
    {
        press.Dispose();
        pointer.Dispose();
    }
}


