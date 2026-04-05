using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameHost : MonoBehaviour
{
    [SerializeField] private GameObject speech;
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private float textSpeed = 0.05f;

    private Animator animator;
    //private SpriteRenderer gameHostSR;
    [SerializeField] private TextMeshPro gameHostText;

    private bool isTalking = false;
    private float timer;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Talking", false);
    }

    void Update()
    {
        if (!isTalking) return;
        timer += Time.deltaTime;
        if (timer >= textSpeed)
        {
            timer = 0f;
            gameHostText.maxVisibleCharacters++;

            if (gameHostText.maxVisibleCharacters >= gameHostText.text.Length)
            {
                isTalking = false;
                Invoke("SpeechBubbleDisappear", 5f); // start disappear timer only once fully typed
                Invoke("StopTalkingAnimation", 1f);
            }
        }
    }

   public void HostComment(string comment)
    {
        Debug.Log("HostComment called with: " + comment);
        animator.SetBool("Talking", true);
        gameHostText.text = comment;
        gameHostText.maxVisibleCharacters = 0;
        speech.gameObject.SetActive(true);
        isTalking = true;
        timer = 0f;
        gameHostText.rectTransform.anchoredPosition = speechBubble.transform.position;
        CancelInvoke("SpeechBubbleDisappear");
        CancelInvoke("StopTalkingAnimation");
    }

    private void SpeechBubbleDisappear()
    {
        speech.gameObject.SetActive(false);

    }

    private void StopTalkingAnimation()
    {
        animator.SetBool("Talking", false);
    }
}
