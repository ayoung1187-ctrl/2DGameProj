/* 
 * Purpose: Controls the timer, how it works, and how it's displayed.
 * 
 * Attached To: Timer
 * 
 * Last Edited: 3/25/26
 */

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    /*
     * Field Variables
     */
    static public float timerWholeSecs; //210; // 3:30
    private int timerMins;
    private int timerSecs;
    [SerializeField] private ConveyorHandling conveyor;
    [SerializeField] InteractObjHandling interactObjHandling;
    TextMeshProUGUI time;

    void Start()
    {
        timerWholeSecs = 90;
        time = GetComponent<TextMeshProUGUI>();
    }

    /*
     * Update(): Displays the elasped time while also checking to see if it has reached zero. If so, the timer turns off.
     */
    void Update()
    {
        timerWholeSecs -= Time.deltaTime;
        if (timerWholeSecs <= 0)
        {
            TimerEnded();
        }

        timerMins = (int)(timerWholeSecs / 60.0);
        timerSecs = (int)timerWholeSecs - timerMins * 60;
        time.text = string.Format("{0}:{1:D2}", timerMins, timerSecs);
    }

    void TimerEnded()
    {
        Debug.Log("TIMES UP!");
        conveyor.enabled = false;
        interactObjHandling.enabled = false;
        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        SceneManager.LoadScene("EndGame", LoadSceneMode.Additive);
    }
}
