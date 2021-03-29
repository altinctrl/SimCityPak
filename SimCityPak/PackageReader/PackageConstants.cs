using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityPak
{
    /// <summary>
    /// Contains all the default packages used by SimCity
    /// </summary>
    public static class SimCityPackages
    {
        public static string[] SimCityScriptsPackages = new string[]
        {
            "SimCity-Scripts_*.package",
            "HeroesAndVillains-Scripts_*.package",
            "Sandbox-Scripts_*.package",
            "SimCityDLCAirships-Scripts_*.package",
            "SimCityDLCAmusementClassic-Scripts_*.package",
            "SimCityDLCAmusementThrill-Scripts_*.package",
            "SimCityDLCAmusementWheels-Scripts_*.package",
            "SimCityDLCBerlin-Scripts_*.package",
            "SimCityDLCCrest-Scripts_*.package",
            "SimCityDLCEP1-Scripts_*.package",
            "SimCityDLCLaunchArcology-Scripts_*.package",
            "SimCityDLCLaunchMemorialPark-Scripts_*.package",
            "SimCityDLCLondon-Scripts_*.package",
            "SimCityDLCMediaMarkt-Scripts_*.package",
            "SimCityDLCMetro-Scripts_*.package",
            "SimCityDLCMicroMania-Scripts_*.package",
            "SimCityDLCNissan-Scripts_*.package",
            "SimCityDLCParis-Scripts_*.package",
            "SimCityDLCPlay-Scripts_*.package",
            "SimCityDLCProgressive-Scripts_*.package",
            "SimCityDLCRedCross-Scripts_*.package",
            "SimCityDLCRollerCoasterCrown-Scripts_*.package",
            "SimCityDLCRomanCasino-Scripts_*.package",
            "SimCityDLCSimsPark-Scripts_*.package",
            "SimCityDLCTelia-Scripts_*.package",
            "SimCityDLCWorship-Scripts_*.package"
        };

        public static string[] SimCityDefaultPackages = new string[]
        {
            "SimCity_Game.package",
            "SimCity_Graphics.package",
            "SimCity_DLC0.package",
            "SimCity_App.package",
            "SimCityDataEP1.package",
            "SimCity_Audio_Banks.package",
            "SimCity_Audio_Streams.package",
            "SimCity_Audio_SpeechBanks.package",
            "SimCity_Audio_MusicStreams.package",
            "SimCity_RegionTerrain0.package",
            "SimCity_RegionTerrain1.package"
        };
        
        public const string SimCity_Game = "SimCity_Game.package";
        public const string SimCity_Graphics = "SimCity_Graphics.package";
        public const string SimCity_DLC0 = "SimCity_DLC0.package";
        public const string SimCity_App = "SimCity_App.package";
        public const string SimCityDataEP1 = "SimCityDataEP1.package";
        public const string SimCity_Audio_Banks = "SimCity_Audio_Banks.package";
        public const string SimCity_Audio_Streams = "SimCity_Audio_Streams.package";
        public const string SimCity_Audio_SpeechBanks = "SimCity_Audio_SpeechBanks.package";
        public const string SimCity_Audio_MusicStreams = "SimCity_Audio_MusicStreams.package";
        public const string SimCity_RegionTerrain0 = "SimCity_RegionTerrain0.package";
        public const string SimCity_RegionTerrain1 = "SimCity_RegionTerrain1.package";
    }



    /// <summary>
    /// Contains often-used folders from the SimCity root folder
    /// </summary>
    public static class SimCityFolders
    {
        public const string SimCityData = "SimCityData";
        public const string EcoGame = "SimCityUserData/EcoGame";
        public const string Packages = "SimCityUserData/Packages";
    }
}
