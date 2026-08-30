using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text;
    Color color;
    public float lowerA = 0.7f;

    private void Start()
    {
        color = text.color;
        color.a = lowerA;
        text.color = color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        color = text.color;
        if (active)
        {
            color.a = 1f;
            text.fontStyle |= FontStyles.Underline;
        }
        else
        {
            color.a = lowerA;
            text.fontStyle &= ~FontStyles.Underline;
        }
        text.color = color;
    }

}
