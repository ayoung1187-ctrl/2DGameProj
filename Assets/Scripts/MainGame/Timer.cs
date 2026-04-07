/* 
 * Purpose: Controls the timer, how it works, and how it's displayed.
 * 
 * Attached To: Timer
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
    [SerializeField] private InteractObjHandling interactObjHandling;
    [SerializeField] private RoomDetectionHandling RDH;
    [SerializeField] private SheepBehavior sheep;
    [SerializeField] private AudioSource AS;

    TextMeshProUGUI time;

    void Start()
    {
        timerWholeSecs = 60;
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

    // Finds all object types that need to be disabled on game over, disables itself and loads next scene
    void TimerEnded()
    {
        conveyor.enabled = false;
        interactObjHandling.enabled = false;
        RDH.enabled = false;

        SheepBehavior[] sheep = FindObjectsByType<SheepBehavior>(FindObjectsSortMode.None);
        foreach (MonoBehaviour s in sheep)
        {
            s.StopAllCoroutines();
            s.enabled = false;
        }

        ObjectData[] obj = FindObjectsByType<ObjectData>(FindObjectsSortMode.None);
        foreach (MonoBehaviour s in obj)
        {
            s.enabled = false;
        }

        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in audioSources)
        {
            if (audio.gameObject.tag != "MainCamera")
            {
                audio.Stop();
                audio.enabled = false;
            }
        }

        Rigidbody2D[] bodies2D = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (Rigidbody2D rb in bodies2D)
        {
            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            rb.simulated = false;
        }


        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        SceneManager.LoadScene("EndGame", LoadSceneMode.Additive);
    }
}
