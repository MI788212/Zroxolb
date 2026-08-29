using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class GameStats
    {
        public int stageIndex;
        public int moves;
        public float time;

        public override string ToString()
        {
            return $"Stage: {stageIndex}, Moves: {moves}, Time: {time}";
        }
   
    }

}
