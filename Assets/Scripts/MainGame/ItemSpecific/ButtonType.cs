/* 
 * Purpose: This class simply defines the buttons that are used for the drop down windows.
 * 
 * Attached To: CraftButton and TaskButton
 * 
 * Class Function: Button types are defined by an enumerator type, which is selected in inspector.
 *                 OnClick() refers to Toggle() in the class PullDownWindow (also selected in inspector).
 * 
 * Last Edited: 3/25/26
 */

using UnityEngine;

public class ButtonType : MonoBehaviour
{
    public enum buttonType { Craft, TaskList };
    public buttonType type;
}
