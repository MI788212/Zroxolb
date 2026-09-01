using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;

public class GameStatsUI : MonoBehaviour
{
    public StatsType statsType;
    private TMP_Text text;

    private GameStatsManager gameStatsManager;
    private StageManager stageManager;
    private Player playerScript;
    private void Awake()
    {
        gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        stageManager = GameObject.FindGameObjectWithTag("StageManager").GetComponent<StageManager>();
        playerScript = FindAnyObjectByType<Player>(FindObjectsInactive.Include);
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        switch (statsType)
        {
            case StatsType.Moves:
                gameStatsManager.MovesUpdated += OnMovesUpdated;
                OnMovesUpdated(gameStatsManager.moves);
                break;
            case StatsType.Time:
                gameStatsManager.TimeUpdated += OnTimeUpdated;
                OnTimeUpdated(gameStatsManager.time);
                break;
            case StatsType.StageIndex:
            case StatsType.BestMoves:
            case StatsType.BestTime:
                stageManager.StageLoaded += OnStageLoaded;
                OnStageLoaded(stageManager.currentStageIndex);
                break;
            case StatsType.StarPower:
                playerScript.UpdatedStarPower += OnUpdatedStarPower;
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        switch (statsType)
        {
            case StatsType.Moves:
                gameStatsManager.MovesUpdated -= OnMovesUpdated;
                break;
            case StatsType.Time:
                gameStatsManager.TimeUpdated -= OnTimeUpdated;
                break;
            case StatsType.StageIndex:
            case StatsType.BestMoves:
            case StatsType.BestTime:
                stageManager.StageLoaded -= OnStageLoaded;
                OnStageLoaded(stageManager.currentStageIndex);
                break;
            case StatsType.StarPower:
                playerScript.UpdatedStarPower -= OnUpdatedStarPower;
                break;
            default:
                break;
        }
    }

    private void OnMovesUpdated(int moves)
    {
        text.text = moves.ToString();
    }

    private void OnTimeUpdated(float time)
    {
        text.text = GameStats.ToTimeFormat(time);
    }

    private void OnStageLoaded(int stageIndex)
    {
        if (statsType == StatsType.BestMoves)
        {
            string key = "BestMoves" + stageIndex;
            if (PlayerPrefs.HasKey(key)) 
            { 
                text.text = PlayerPrefs.GetInt(key).ToString();
            }
            else
            {
                text.text = GameStats.UNKNOWNMOVES;
            }
        }
        else if (statsType == StatsType.BestTime)
        {
            string key = "BestTime" + stageManager.currentStageIndex;
            if (PlayerPrefs.HasKey(key))
            {
                text.text = GameStats.ToTimeFormat(PlayerPrefs.GetFloat(key));
            }
            else
            {
                text.text = GameStats.UNKNOWNTIME;
            }
        }
        else if (statsType == StatsType.StageIndex)
        {
            text.text = stageIndex.ToString();
        }
    }

    private void OnUpdatedStarPower(int starPower)
    {
        if (starPower == 0)
        {
            text.alpha = 0;
            return;
        }

        text.alpha = 1;
        text.text = "Star Power: " + starPower;
    }
}
public enum StatsType
{
    Moves,
    Time,
    StageIndex,
    BestMoves,
    BestTime,
    StarPower
}