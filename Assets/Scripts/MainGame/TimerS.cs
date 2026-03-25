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

public class TimerS : MonoBehaviour
{
    static public float timerWholeSecs = 210; // 3:30
    private int timerMins;
    private int timerSecs;
    TextMeshProUGUI time;

    void Start()
    {
        time = GetComponent<TextMeshProUGUI>();
    }

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
    }
}
