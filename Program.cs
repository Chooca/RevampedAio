using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        public static void Check()
        {
            try
            {

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                bool wb = new WebClient().DownloadString("https://github.com/Chooca/RevampedAio/blob/master/version.txt").Contains("0.0.0.1");
                if (!wb)
                {
                    Game.Print("<b><font Size='25' color='#0000b2'>RevampedAIO outdated version...Please Update!</font></b>");
                }
                else
                    Game.Print("<b><font Size='35' color='#FF0000'>RevampedAIO loaded, current version(0.0.0.1)</font></b>");

            }
            catch (Exception E)
            {
                Console.WriteLine("An error try again " + E);
            }
        }


    }
}
