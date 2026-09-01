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

    public GameObject player;
    private Player playerScript;

    public event Action<int> LoadNewStage;
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
        playerScript = player.GetComponent<Player>();
        player.SetActive(false);
    }

    private void OnEnable()
    {
        playerScript.PlayerFellOffPlatform += LoadStage;
        playerScript.PlayerFellIntoHole += StageClear;
    }

    private void OnDisable()
    {
        playerScript.PlayerFellOffPlatform -= LoadStage;
        playerScript.PlayerFellIntoHole -= StageClear;
    }

    public void LoadStage()
    {
        UnloadStage();
        player.SetActive(true);


        GameObject stage = Instantiate(currentStage.StagePrefab);
        stage.transform.SetParent(transform);

        playerScript.StartPlayer(initialPlayerPosition);

        StageLoaded?.Invoke(currentStageIndex);
    }

    public void LoadStage(int stageIndex)
    {
        SetStage(stageIndex);
        LoadStage();
    }
    public void SetStage(int stageIndex)
    {
        if(stageIndex >= Stages.Count)
        {
            return;
        }

        currentStageIndex = stageIndex;

        currentStage = Stages.ElementAt(currentStageIndex);
        initialPlayerPosition = currentStage.GetInitialPlayerPosition();
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

        LoadNewStage?.Invoke(currentStageIndex + 1);
    }

    public void UnloadStage()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}
