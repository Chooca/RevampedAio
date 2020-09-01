using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace RevampedAio
{
    class Program
    {
        private static AIHeroClient Player => ObjectManager.Player;
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }

        private static void GameEventOnOnGameLoad()
        {
            switch (Player.CharacterName)
            {
                case "Cassiopeia":
                    Cassiopeia_Du_Couteau_2.Program.CassiopeiaLoading_OnLoadingComplete();
                    break;
            }
        }
    }
}
