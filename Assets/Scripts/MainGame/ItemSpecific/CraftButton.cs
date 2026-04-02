using UnityEngine;

public class CraftButton : MonoBehaviour
{
    public Sprite[] buttonImage = new Sprite[2];
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ChangeButtonImageOff()
    {
        spriteRenderer.sprite = buttonImage[0];
    }

    public void ChangeButtonImageOn()
    {
        spriteRenderer.sprite = buttonImage[1];
    }
}
