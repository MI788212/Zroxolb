using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManagerScript : MonoBehaviour
{
    public List<Stage> Stages;
    public int CurrentStageIndex;
    public Stage CurrentStage;
    public Vector2 InitialPlayerPosition = new Vector2(0, 0);


    private PlayerScript playerScript;

    public event Action NextStageIntermission;
    public event Action LastStageCleared;
    public event Action StageLoaded;
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
    }
    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        playerScript.PlayerFellOffPlatform += GameOver;
        playerScript.PlayerFellIntoHole += StageClear;
    }

    public void LoadStage()
    {
        ClearStage();

        GameObject stage = Instantiate(CurrentStage.StagePrefab);
        stage.transform.SetParent(transform);

        playerScript.StartPlayer(InitialPlayerPosition);

        StageLoaded?.Invoke();
    }

    private void LoadNextStage()
    {
        NextStage();
        NextStageIntermission?.Invoke();
    }

    private void NextStage()
    {
        CurrentStageIndex++;

        if (CurrentStageIndex >= Stages.Count())
        {
            CurrentStageIndex--;
            Debug.Log("trying to move onto next stage, but this is the last.");
        }

        CurrentStage = Stages.ElementAt(CurrentStageIndex);
        InitialPlayerPosition = CurrentStage.GetInitialPlayerPosition();
    }

    public void LoadStage(int stageIndex)
    {
        SetStage(stageIndex);
        LoadStage();
    }
    public void SetStage(int stageIndex)
    {
        CurrentStageIndex = stageIndex;

        CurrentStage = Stages.ElementAt(CurrentStageIndex);
        InitialPlayerPosition = CurrentStage.GetInitialPlayerPosition();
    }

    private void GameOver()
    {
        //Debug.Log("Game over.");
        LoadStage();
    }

    private void StageClear()
    {
        StageCleared?.Invoke();

        if (CurrentStageIndex + 1 >= PlayerPrefs.GetInt(unlockedStagesKey))
        {
            PlayerPrefs.SetInt(unlockedStagesKey, CurrentStageIndex + 2);
            UnlockedStage?.Invoke();
        }

        //Debug.Log("Stage cleared.");
        if (CurrentStageIndex== Stages.Count - 1)
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
