using System.Collections;
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

    private StageManagerScript stageManagerScript;

    void Start()
    {
        stageManagerScript = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManagerScript>();

        HideCanvasChildren();

        ShowScreen(titleScreen);

        stageManagerScript.NextStageIntermission += InterStage;
        stageManagerScript.LastStageCleared += Congratulations;
    }

    public void StartNewGame()
    {
        stageManagerScript.LoadStage(0);

        ShowScreen(gameScreen);
    }

    public void LoadStage(int stageIndex)
    {
        stageManagerScript.LoadStage(stageIndex);

        ShowScreen(gameScreen);
    }

    private void InterStage()
    {
        ShowScreen(interStageScreen);
        StartCoroutine(DelayLoadNextStage());
    }

    private IEnumerator DelayLoadNextStage()
    {
        yield return new WaitForSeconds(interStageDuration);
        ShowScreen(gameScreen);
        stageManagerScript.LoadStage();
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
