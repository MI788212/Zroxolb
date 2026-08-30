using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public List<Stage> Stages;
    internal int currentStageIndex;
    private Stage currentStage;
    private Vector2 initialPlayerPosition = new Vector2(0, 0);


    private PlayerScript playerScript;

    public event Action NextStageIntermission;
    public event Action LastStageCleared;
    public event Action<int> StageLoaded;
    public event Action StageCleared;
    public event Action UnlockedStage;

    internal string unlockedStagesKey;

    private void Awake()
    {
        unlockedStagesKey = "UnlockedStages";
        if (!PlayerPrefs.HasKey(unlockedStagesKey))
        {
            PlayerPrefs.SetInt(unlockedStagesKey, 1);
        }
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    private void OnEnable()
    {
        playerScript.PlayerFellOffPlatform += GameOver;
        playerScript.PlayerFellIntoHole += StageClear;
    }

    private void OnDisable()
    {
        playerScript.PlayerFellOffPlatform -= GameOver;
        playerScript.PlayerFellIntoHole -= StageClear;
    }

    public void LoadStage()
    {
        ClearStage();

        GameObject stage = Instantiate(currentStage.StagePrefab);
        stage.transform.SetParent(transform);

        playerScript.StartPlayer(initialPlayerPosition);

        StageLoaded?.Invoke(currentStageIndex);
    }

    private void LoadNextStage()
    {
        NextStage();
        NextStageIntermission?.Invoke();
    }

    private void NextStage()
    {
        currentStageIndex++;

        if (currentStageIndex >= Stages.Count())
        {
            currentStageIndex--;
            Debug.Log("trying to move onto next stage, but this is the last.");
        }

        currentStage = Stages.ElementAt(currentStageIndex);
        initialPlayerPosition = currentStage.GetInitialPlayerPosition();
    }

    public void LoadStage(int stageIndex)
    {
        SetStage(stageIndex);
        LoadStage();
    }
    public void SetStage(int stageIndex)
    {
        currentStageIndex = stageIndex;

        currentStage = Stages.ElementAt(currentStageIndex);
        initialPlayerPosition = currentStage.GetInitialPlayerPosition();
    }

    private void GameOver()
    {
        //Debug.Log("Game over.");
        LoadStage();
    }

    private void StageClear()
    {
        StageCleared?.Invoke();

        if (currentStageIndex + 1 >= PlayerPrefs.GetInt(unlockedStagesKey))
        {
            PlayerPrefs.SetInt(unlockedStagesKey, currentStageIndex + 2);
            UnlockedStage?.Invoke();
        }

        //Debug.Log("Stage cleared.");
        if (currentStageIndex== Stages.Count - 1)
        {
            //Debug.Log("last stage cleared");
            LastStageCleared?.Invoke();
            return;
        }

        LoadNextStage();
    }

    private void ClearStage()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}
