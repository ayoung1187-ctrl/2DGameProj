using TMPro;
using UnityEngine;

public class TasksHandling : MonoBehaviour
{
    [SerializeField] private CraftingHandling craftCheck;
    [SerializeField] private InteractObjHandling sheepCheck;

    static public bool isBudgetCheck = true;
    static public bool isCraftCheck = false;
    static public bool isSheepCheck = false;

    TextMeshProUGUI taskList;
    string fullText;
    string[] lines = new string[3];
    string[] content = { "<indent=15%>Stay under budget.</indent>",
                     "<indent=15%>Craft 1 new item</indent>",
                     "<indent=15%>utilize sheep</indent>" };

    private void Start()
    {
        taskList = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (taskList != null)
        {
            if (SpentBudget.spentMoney < SpentBudget.budget)
            {
                lines[0] = "- <s>" + content[0] + "</s>";
                isBudgetCheck = true;
            }
            else
            {
                lines[0] = "- " + content[0];
                isBudgetCheck = false;
            }

            if (craftCheck.oneItemCrafted)
            {
                lines[1] = "- <s>" + content[1] + "</s>";
                isCraftCheck = true;
            }
            else
            {
                isCraftCheck = false;
                lines[1] = "- " + content[1];
            }

            if (sheepCheck.isSheepUtilized)
            {
                isSheepCheck = true;
                lines[2] = "- <s>" + content[2] + "</s>";
            }
            else
            {
                isSheepCheck = false;
                lines[2] = "- " + content[2];
            }

            taskList.text = lines[0] + "\n" + lines[1] + "\n" + lines[2];
        }
    }
}
