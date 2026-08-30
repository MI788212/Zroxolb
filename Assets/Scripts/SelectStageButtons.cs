using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectStageButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text text;
    Color color;
    public float lowerA = 0.5f;

    TMP_Text buttonText;
    public float lowererA = 0.2f;

    private string stageIndex;
    private string gameStats;
    private bool hasGameStats;
    private string noGameStats;

    private StageManager stageManager;

    private void Awake()
    {
        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();

        buttonText = transform.GetChild(0).transform.GetComponent<TMP_Text>();

        text = GameObject.FindGameObjectWithTag("BestStats").GetComponent<TMP_Text>();

        stageIndex = transform.name;

        noGameStats = "Best Time:\tx.xx.xx\nBest Moves:\tx";

    }

    private void OnEnable()
    {
        UpdateGameStats();
        UpdateInteractable();
        SetActive(false);
    }

    private void UpdateGameStats()
    {
        if (PlayerPrefs.HasKey("BestMoves" + stageIndex))
        {
            hasGameStats = true;
            string bestMovesKey = "BestMoves" + stageIndex;
            string bestTimeKey = "BestTime" + stageIndex;
            int bestMoves = PlayerPrefs.GetInt(bestMovesKey);
            float bestTime = PlayerPrefs.GetFloat(bestTimeKey);
            string formattedTime = TimeSpan.FromSeconds(bestTime).ToString(@"%h\:mm\:ss");
            gameStats = "Best Time:\t" + formattedTime + "\nBest Moves:\t" + bestMoves;
            //Debug.Log(gameStats);
        }
        else
        {
            hasGameStats = false;
            gameStats = noGameStats;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("on pointer enter " + stageIndex);
        SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        color = text.color;
        if (active&&hasGameStats)
        {
            color.a = 1f;
            text.text = gameStats;
        }
        else
        {
            color.a = lowererA;
            text.text = noGameStats;
        }
        text.color = color;
    }

    private void UpdateInteractable()
    {
        int stageIndexInt = int.Parse(stageIndex);
        bool interactable = stageIndexInt < PlayerPrefs.GetInt(stageManager.unlockedStagesKey);
        transform.GetComponent<Button>().interactable = interactable;
        if (!interactable)
        {
            transform.GetComponent<ButtonScript>().enabled = false;
            Color btnColor = buttonText.color;
            btnColor.a = lowererA;
            buttonText.color = btnColor;
            //Debug.Log("Button " + stageIndex + " set uninteractible");
        }
        else
        {
            Color btnColor = buttonText.color;
            btnColor.a = 0.5f;
            buttonText.color = btnColor;
            transform.GetComponent<ButtonScript>().enabled = true;
            //Debug.Log("Button " + stageIndex + " set interactible");
        }
    }
}
