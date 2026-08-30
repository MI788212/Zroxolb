using Assets.Scripts;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject titleScreen;
    public GameObject stageSelectScreen;
    public GameObject interStageScreen;
    public GameObject congratulationsScreen;
    public GameObject menuScreen;
    public GameObject settingsScreen;
    public GameObject gameScreen;

    public float interStageDuration = 2f;
    public TMP_Text interStageText;

    private StageManager stageManager;
    private GameStatsManager gameStatsManager;

    void Awake()
    {
        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();
        gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

        DeactivateScreens();

        ShowScreen(titleScreen);
    }

    private void OnEnable()
    {
        stageManager.LoadNewStage += OnLoadNewStage;
        stageManager.LastStageCleared += OnLastStageCleared;
    }

    private void OnDisable()
    {
        stageManager.LoadNewStage -= OnLoadNewStage;
        stageManager.LastStageCleared -= OnLastStageCleared;
    }

    public void StartNewGame()
    {
        OnLoadNewStage(0);
    }

    public void OnLoadNewStage(int stageIndex)
    {
        if (stageIndex >= stageManager.Stages.Count)
        {
            return;
        }

        StartCoroutine(InterstageAndLoad(stageIndex));
    }

    private IEnumerator InterstageAndLoad(int stageIndex)
    {
        interStageText.text = "Stage " + stageIndex;

        ShowScreen(interStageScreen);
        yield return new WaitForSeconds(interStageDuration);

        ShowScreen(gameScreen);
        stageManager.LoadStage(stageIndex);
    }

    private void OnLastStageCleared()
    {
        GameStats totalGameStats = gameStatsManager.CalculateTotalStats();
        string totalStats = "Total Time:\t" + TimeSpan.FromSeconds(totalGameStats.time).ToString(@"%h\:mm\:ss") + "\nTotal Moves:\t" + totalGameStats.moves;
        TMP_Text totalStatsTMP = congratulationsScreen.transform.Find("group/TotalStats").GetComponent<TMP_Text>();
        totalStatsTMP.text = totalStats;
        ShowScreen(congratulationsScreen);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void ShowScreen(GameObject screen)
    {
        //Debug.Log("Open " + screen);

        DeactivateScreens();

        screen.SetActive(true);
    }

    void DeactivateScreens()
    {
        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
