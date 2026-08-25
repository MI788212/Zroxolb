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

    public float gravity = -25f;

    private PlayerScript playerScript;

    public event Action NextStageIntermission;
    public event Action LastStageCleared;

    private void Awake()
    {
        Physics.gravity = new Vector3(0, gravity, 0);
    }
    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        playerScript.PlayerFellOffPlatform += GameOver;
        playerScript.PlayerFellIntoHole += StageCleared;
    }

    public void LoadStage()
    {
        Destroy(transform.GetChild(0).gameObject);

        GameObject stage = Instantiate(CurrentStage.StagePrefab);
        stage.transform.SetParent(transform);

        playerScript.StartPlayer(InitialPlayerPosition);
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
    private void SetStage(int stageIndex)
    {
        CurrentStageIndex = stageIndex;

        CurrentStage = Stages.ElementAt(CurrentStageIndex);
        InitialPlayerPosition = CurrentStage.GetInitialPlayerPosition();
    }

    private void GameOver()
    {
        Debug.Log("Game over.");
        LoadStage();
    }

    private void StageCleared()
    {
        Debug.Log("Stage cleared.");
        if(CurrentStageIndex== Stages.Count - 1)
        {
            Debug.Log("last stage cleared");
            LastStageCleared?.Invoke();
            return;
        }
        LoadNextStage();
    }
}
