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
