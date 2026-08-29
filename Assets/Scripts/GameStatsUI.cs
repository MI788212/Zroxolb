using System;
using TMPro;
using UnityEngine;

public class GameStatsUI : MonoBehaviour
{
    public StatsType statsType;
    private TMP_Text text;

    private GameStatsManager gameStats;
    private StageManagerScript stageManagerScript;
    private void Awake()
    {
        gameStats = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        stageManagerScript = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManagerScript>();
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if(statsType == StatsType.Moves || statsType == StatsType.Time)
            gameStats.CurrentStatsUpdated += OnCurrentStatsUpdated;
        else
            stageManagerScript.StageLoaded += OnStageLoaded;
    }

    private void OnDisable()
    {
        gameStats.CurrentStatsUpdated -= OnCurrentStatsUpdated;
    }

    private void OnCurrentStatsUpdated()
    {
        if (statsType == StatsType.Moves)
        {
            text.text = gameStats.moves.ToString();
        }
        else if(statsType == StatsType.Time)
        {
            text.text = TimeSpan.FromSeconds(gameStats.time).ToString(@"%h\:mm\:ss");
        }
    }

    private void OnStageLoaded()
    {
        if (statsType == StatsType.BestMoves)
        {
            string key = "BestMoves" + stageManagerScript.CurrentStageIndex;
            if (PlayerPrefs.HasKey(key)) 
            { 
                text.text = PlayerPrefs.GetInt(key).ToString();
            }
            else
            {
                text.text = "x";
            }
        }
        else if (statsType == StatsType.BestTime)
        {
            string key = "BestTime" + stageManagerScript.CurrentStageIndex;
            if (PlayerPrefs.HasKey(key))
            {
                text.text = TimeSpan.FromSeconds(PlayerPrefs.GetFloat(key)).ToString(@"%h\:mm\:ss");
            }
            else
            {
                text.text = "xx:xx:xx";
            }
        }
        else if (statsType == StatsType.StageIndex)
        {
            text.text = stageManagerScript.CurrentStageIndex.ToString();
            //Debug.Log(gameStats.stageIndex);
        }
    }
}
public enum StatsType
{
    Moves,
    Time,
    StageIndex,
    BestMoves,
    BestTime
}