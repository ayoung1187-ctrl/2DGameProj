using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Bonus = # of crafted items * added worth - overspent budget
public class EndGameScoring : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] text = new TextMeshProUGUI[6];
    [SerializeField] private TextMeshProUGUI[] score = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI letterGrade;

    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonQuit;

    [SerializeField] private GameHost GH;

    private int totalScore;

    private float textSpeed = 0.05f;
    private float timerAmt = 1f;
    private int val1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GH.HostComment("And that's time! Now let's see how you did.");

        StartCoroutine(DisplayText());
        score[0].text = TasksHandling.isBudgetCheck ? "+ 1000" : "+ 0";
        score[0].color = TasksHandling.isBudgetCheck ? Color.forestGreen : Color.crimson;
        totalScore += TasksHandling.isBudgetCheck ? 1000 : 0;

        score[1].text = TasksHandling.isCraftCheck ? "+ 1000" : "+ 0";
        score[1].color = TasksHandling.isCraftCheck ? Color.forestGreen : Color.crimson;
        totalScore += TasksHandling.isCraftCheck ? 1000 : 0;

        score[2].text = TasksHandling.isSheepCheck ? "+ 1000" : "+ 0";
        score[2].color = TasksHandling.isSheepCheck ? Color.forestGreen : Color.crimson;
        totalScore += TasksHandling.isSheepCheck ? 1000 : 0;

        val1 = TasksHandling.isBudgetCheck ? 0 : SpentBudget.spentMoney - SpentBudget.budget;
        int val2 = CraftingHandling.numCraftedItems * CraftingHandling.craftedItemsValue - val1;
        score[3].text = "+ " + val2;
        score[3].color = val2 > 0 ? Color.forestGreen : Color.crimson;
        totalScore += val2;

        score[4].text = "" + totalScore;

        if (totalScore < 1000)
        {
            letterGrade.text = "F";
            letterGrade.color = Color.crimson;
        }
        else if (totalScore <= 1500)
        {
            letterGrade.text = "C";
            letterGrade.color = Color.orangeRed;
        }
        else if (totalScore <= 3000)
        {
            letterGrade.text = "B";
            letterGrade.color = Color.seaGreen;
        }
        else if (totalScore > 4500)
        {
            letterGrade.text = "A";
            letterGrade.color = Color.forestGreen;
        }
        else if (totalScore > 6000)
        {
            letterGrade.text = "S";
            letterGrade.color = Color.royalBlue;
        }
    }

    private IEnumerator DisplayText()
    {
        text[0].maxVisibleCharacters = 0;
        text[0].gameObject.SetActive(true);
        while (text[0].maxVisibleCharacters < text[0].text.Length)
        {
            text[0].maxVisibleCharacters++;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(timerAmt); // pause before showing score

        for (int i = 1; i < text.Length; i++)
        {
            int j = i - 1;
            // type out the label text
            text[i].maxVisibleCharacters = 0;
            text[i].gameObject.SetActive(true);
            while (text[i].maxVisibleCharacters < text[i].text.Length)
            {
                text[i].maxVisibleCharacters++;
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(timerAmt); // pause before showing score

            // type out the score
            score[j].maxVisibleCharacters = 0;
            score[j].gameObject.SetActive(true);
            while (score[j].maxVisibleCharacters < score[j].text.Length)
            {
                score[j].maxVisibleCharacters++;
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(timerAmt); // pause before next row
        }

        // show letter grade after all rows done
        letterGrade.gameObject.SetActive(true);

        if (totalScore <= 1500)
        {
            GH.HostComment("Oof... you stink. Better luck next season.");
        }
        else
        {
            GH.HostComment("Wow, incredible work! Keep it up for the next round!");
        }

        yield return new WaitForSeconds(1f);

        buttonRetry.gameObject.SetActive(true);
        buttonQuit.gameObject.SetActive(true);
    }

    public void OnRetryClick()
    {
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void OnQuitClick()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
