/* 
 * Purpose: This class handles the pull down windows for crafting and tasks.
 *          It should be a toggle, pull down incrementally, with the pull down triangle inverting after toggle.
 * 
 * Attached To: CraftingTable and TaskList
 * 
 * Class Function: 
 * 
 * Last Edited: 3/25/26
 */

using UnityEngine;

public class PullDownWindow : MonoBehaviour
{
    /*
     * Field Variables
     */

    [Header("Window Parts")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform arrow;

    [Header("Positions")]
    [SerializeField] private Vector2 panelOpenPosition;
    [SerializeField] private Vector2 panelClosedPosition;
    [SerializeField] private Vector2 contentOpenPosition;
    [SerializeField] private Vector2 contentClosedPosition;

    [Header("Drop Down Speed")]
    [SerializeField] private float speed = 800f;

    // Other Variables
    private bool isOpen;

    /*
     * Update(): For both the panel and elements on panel: if it should be open, move towards open position. Else, vice versa.
     *           (Yay ternary operator!)
     */
    void Update()
    {
        if (panel != null)
        {
            Vector2 target = isOpen ? panelOpenPosition : panelClosedPosition;
            panel.anchoredPosition = Vector2.MoveTowards(panel.anchoredPosition, target, speed * Time.deltaTime);
        }

        if (content != null)
        {
            Vector2 target = isOpen ? contentOpenPosition : contentClosedPosition;
            content.anchoredPosition = Vector2.MoveTowards(content.anchoredPosition, target, speed * Time.deltaTime);
        }
    }

    /*
     * Toggle(): Is connected to ButtonType class. If the button is pressed, this is called where isOpen is inverted.
     */
    public void Toggle()
    {
        isOpen = !isOpen;
        UpdateArrow();
    }

    /*
     * UpdateArrow(): If the panel is open, invert the scale to negative. If closed, set the scale to positive to bring it back to original position.
     */
    private void UpdateArrow()
    {
        if (arrow == null) return;

        Vector3 scale = arrow.localScale;
        scale.y = isOpen ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        arrow.localScale = scale;
    }
}