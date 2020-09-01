using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EnsoulSharp;
using EnsoulSharp.SDK;
using System.Reflection;
using System.Security.Permissions;
namespace RevampedAio
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
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
            //Checkgit();
        }
        public static void Checkgit()
        {
            using (var wb = new WebClient())
            {
                var raw = wb.DownloadString("https://github.com/Chooca/RevampedAio/blob/master/version.txt");

                Version Version = Assembly.GetExecutingAssembly().GetName().Version;

                if (raw != Version.ToString())
                {
                    Game.Print("Oudated", raw);
                }
                else
                    Game.Print("Updated", Version.ToString());
            }
        }

    }
}
