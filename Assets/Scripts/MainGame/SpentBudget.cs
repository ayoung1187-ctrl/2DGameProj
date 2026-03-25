/* 
 * Purpose:
 * 
 * Attached To:
 * 
 * Class Function:
 * 
 * Last Edited: 3/20/26
 */

using UnityEngine;
using TMPro;

public class SpentBudget : MonoBehaviour
{
    static public int spentMoney = 0;
    private int spentMoneyDisplay = 0;

    public int budget = 5000;
    TextMeshProUGUI sb;

    void Start()
    {
        sb = GetComponent<TextMeshProUGUI>();
    }

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
