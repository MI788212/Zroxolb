using Assets.Scripts;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScreenManagerScript : MonoBehaviour
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

    private StageManagerScript stageManagerScript;
    private GameStatsManager gameStatsManager;

    void Start()
    {
        stageManagerScript = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManagerScript>();
        gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

        HideCanvasChildren();

        ShowScreen(titleScreen);

        stageManagerScript.NextStageIntermission += StageIntermission;
        stageManagerScript.LastStageCleared += OnLastStageCleared;
    }

    public void StartNewGame()
    {
        LoadStage(0);
    }

    public void LoadStage(int stageIndex)
    {
        if (stageIndex >= stageManagerScript.Stages.Count)
        {
            return;
        }

        stageManagerScript.SetStage(stageIndex);

        StageIntermission();
    }

    private void StageIntermission()
    {
        StartCoroutine(InterStage());
    }
    private IEnumerator InterStage()
    {
        updateScreenInfo();

        ShowScreen(interStageScreen);
        yield return new WaitForSeconds(interStageDuration);

        ShowScreen(gameScreen);
        stageManagerScript.LoadStage();
    }

    private void updateScreenInfo()
    {
        interStageText.text = "Stage " + stageManagerScript.CurrentStageIndex;
    }

    private void OnLastStageCleared()
    {
        GameStats totalGameStats = gameStatsManager.CalculateTotalStats();
        string totalStats = "Total Time:\t" + TimeSpan.FromSeconds(totalGameStats.time).ToString(@"%h\:mm\:ss") + "\nTotal Moves:\t" + totalGameStats.moves;
        TMP_Text totalStatsTMP = congratulationsScreen.transform.Find("group/TotalStats").GetComponent<TMP_Text>();
        totalStatsTMP.text = totalStats;
        ShowScreen(congratulationsScreen);
    }

    public void ShowScreen(GameObject screen)
    {
        //Debug.Log("Open " + screen);

        HideCanvasChildren();

        screen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    void HideCanvasChildren()
    {
        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
