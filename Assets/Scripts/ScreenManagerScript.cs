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
    public TMP_Text menuStageText;

    private StageManagerScript stageManagerScript;

    void Start()
    {
        stageManagerScript = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManagerScript>();

        HideCanvasChildren();

        ShowScreen(titleScreen);

        stageManagerScript.NextStageIntermission += StageIntermission;
        stageManagerScript.LastStageCleared += Congratulations;
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
        menuStageText.text = "Stage " + stageManagerScript.CurrentStageIndex;
    }

    private void Congratulations()
    {
        ShowScreen(congratulationsScreen);
    }

    public void ShowScreen(GameObject screen)
    {
        Debug.Log("Open " + screen);

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
