using Assets.Scripts;
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

    private void Awake()
    {
        Physics.gravity = new Vector3(0, gravity, 0);
    }
    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        playerScript.PlayerFellOffPlatform += GameOver;
        playerScript.PlayerFellIntoHole += StageCleared;

        CurrentStageIndex = 0;
        CurrentStage = Stages.ElementAt(CurrentStageIndex);
        InitialPlayerPosition = CurrentStage.GetInitialPlayerPosition();

        LoadStage();
    }

    private void LoadStage()
    {
        Destroy(transform.GetChild(0).gameObject);

        GameObject stage = Instantiate(CurrentStage.StagePrefab);
        stage.transform.SetParent(transform);

        playerScript.StartPlayer(InitialPlayerPosition);
    }

    private void LoadNextStage()
    {
        NextStage();
        LoadStage();
    }

    private void NextStage()
    {
        CurrentStageIndex++;

        if (CurrentStageIndex >= Stages.Count())
        {
            CurrentStageIndex--;
            Debug.Log("Trying to move onto next stage, but this stage is the last.");
        }

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
        LoadNextStage();
    }
}
