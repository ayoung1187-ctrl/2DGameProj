using TMPro;
using UnityEngine;

public class TasksHandling : MonoBehaviour
{
    [SerializeField] private CraftingHandling craftCheck;
    [SerializeField] private InteractObjHandling sheepCheck;
    [SerializeField] private SpentBudget budgetCheck;

    TextMeshProUGUI taskList;
    string fullText;
    string[] linesNormal;
    string[] lines;

    private void Start()
    {
        taskList = GetComponent<TextMeshProUGUI>();
        fullText = taskList.text;
        linesNormal = fullText.Split('\n');
        lines = new string[linesNormal.Length];
        linesNormal.CopyTo(lines, 0);
    }

    private void Update()
    {
        if (taskList != null && lines != null)
        {
            if (SpentBudget.spentMoney < budgetCheck.budget)
            {
                lines[0] = "<s>" + linesNormal[0] + "</s>";
            }
            else
            {
                lines[0] = linesNormal[0];
            }

            if (craftCheck.oneItemCrafted)
            {
                lines[1] = "<s>" + linesNormal[1] + "</s>";
            }
            else
            {
                lines[1] = linesNormal[1];
            }

            if (sheepCheck.isSheepUtilized)
            {
                lines[2] = "<s>" + linesNormal[2] + "</s>";
            }
            else
            {
                lines[2] = linesNormal[2];
            }

            taskList.text = lines[0] + "\n" + lines[1] + "\n" + lines[2];
        }
    }
}
