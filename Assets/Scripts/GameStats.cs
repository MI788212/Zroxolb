using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class GameStats
    {
        public int stageIndex;
        public int moves;
        public float time;
        public static string UNKNOWNTIME = "x.xx.xx";
        public static string UNKNOWNMOVES = "x";

        public override string ToString()
        {
            return $"Stage: {stageIndex}, Moves: {moves}, Time: {time}";
        }

        public static string ToTimeFormat(float seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"%h\:mm\:ss");
        }
   
    }

}
