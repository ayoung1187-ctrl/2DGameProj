/* 
 * Purpose: Handles the display of spent money / budget, including how they increase by player action.
 * 
 * Attached To: SpentBudget
 * 
 * Class Function: Turns the text red if over budget, and incrementally changes displayed text when updating
 *
 * # 1 note in Update() #
 * 
 * Last Edited: 3/25/26
 */

using UnityEngine;
using TMPro;

public class SpentBudget : MonoBehaviour
{
    /*
     * Field Variables
     */
    static public int spentMoney = 0; // Static so that it's easily accessible
    private int spentMoneyDisplay = 0;

    static public int budget = 7500; // Public so that it's easily adjustable
    TextMeshProUGUI sb;

    void Start()
    {
        spentMoney = 0;
        spentMoneyDisplay = 0;
        sb = GetComponent<TextMeshProUGUI>();
    }

    /*
     * Update(): If you go over the budget, turn the text red as a warning.
     *           If the runtime quantity spentMoney is greater than what's on the diplay, change the display cost 5 units at a time.
     *           NOTE: If implementing a delete option (potentially with 25% less refund), be careful of how this conditional is handled.
     */
    void Update()
    {
        if (spentMoney > budget)
        {
            sb.color = Color.red;
        }
        if (spentMoney > spentMoneyDisplay)
        {
            spentMoneyDisplay += 5;
        }
        
        sb.text = spentMoneyDisplay + "/" + budget;
    }
}
