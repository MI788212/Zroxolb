using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts;

public class GameStatsManager : MonoBehaviour
{
    internal int moves;
    internal float time;
    internal int stageIndex;

    public event Action CurrentStatsUpdated;

    private PlayerScript playerScript;
    private StageManagerScript stageManagerScript;
    void Awake()
    {
        //Debug.Log("Game stats on!");
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        stageManagerScript = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManagerScript>();
    }

    private void Initialize()
    {
        moves = 0;
        time = 0;
    }

    private void Update()
    {
        time += Time.deltaTime;
        CurrentStatsUpdated?.Invoke();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        playerScript.Rolled += OnRolled;
        stageManagerScript.StageLoaded += OnStageLoaded;
        stageManagerScript.StageCleared += OnStageCleared;
        //Debug.Log("Subscribed to events");
    }

    private void UnsubscribeFromEvents()
    {
        playerScript.Rolled -= OnRolled;
        stageManagerScript.StageLoaded -= OnStageLoaded;
        stageManagerScript.StageCleared -= OnStageCleared;
    }

    private void OnRolled()
    {
        moves++;
    }

    private void OnStageLoaded()
    {
        stageIndex = stageManagerScript.CurrentStageIndex;
        Initialize();
    }

    private void OnStageCleared()
    {
        UpdateStats();
        string formattedTime = TimeSpan.FromSeconds(time).ToString(@"%h\:mm\:ss"); ;
        //Debug.Log("Stage cleared in " + moves + " moves and time: "+formattedTime);

    }

    private void UpdateStats()
    {
        string bestMovesKey = "BestMoves" + stageIndex;
        string bestTimeKey = "BestTime" + stageIndex;
        if (!PlayerPrefs.HasKey(bestMovesKey) || moves < PlayerPrefs.GetInt(bestMovesKey))
        {
            PlayerPrefs.SetInt(bestMovesKey, moves);
        }
        
        if(!PlayerPrefs.HasKey(bestTimeKey) || time < PlayerPrefs.GetFloat(bestTimeKey))
        {
            PlayerPrefs.SetFloat(bestTimeKey, time);
        }

        //string formattedTime = TimeSpan.FromSeconds(PlayerPrefs.GetFloat(bestTimeKey)).ToString(@"%h\:mm\:ss");
        //Debug.Log("Best moves: " + PlayerPrefs.GetInt(bestMovesKey) + " Best time: " + formattedTime);
    }

    public GameStats CalculateTotalStats()
    {
        GameStats totalGameStats = new GameStats(); 
        totalGameStats.moves = 0;
        totalGameStats.time = 0;

        for(int i=0; i<stageManagerScript.Stages.Count; i++)
        {
            string bestMovesKey = "BestMoves" + i;
            string bestTimeKey = "BestTime" + i;
            totalGameStats.moves += PlayerPrefs.GetInt(bestMovesKey);
            totalGameStats.time += PlayerPrefs.GetFloat(bestTimeKey);
        }
        return totalGameStats;
    }
}
