using TMPro;
using UnityEngine;

public class TasksHandling : MonoBehaviour
{
    static public bool isBudgetCheck = true;
    static public bool isCraftCheck = false;
    static public bool isRoomCheck = false;

    TextMeshProUGUI taskList;
    string[] lines = new string[3];
    string[] content = { "<indent=15%>Stay under budget.</indent>",
                     "<indent=15%>Craft 1 new item.</indent>",
                     "<indent=15%>Create unique rooms.</indent>" };

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

            if (isCraftCheck)
            {
                lines[1] = "- <s>" + content[1] + "</s>";
                isCraftCheck = true;
            }
            else
            {
                isCraftCheck = false;
                lines[1] = "- " + content[1];
            }

            if (isRoomCheck)
            {
                isRoomCheck = true;
                lines[2] = "- <s>" + content[2] + "</s>";
            }
            else
            {
                isRoomCheck = false;
                lines[2] = "- " + content[2];
            }

            taskList.text = lines[0] + "\n" + lines[1] + "\n" + lines[2];
        }
    }
}
