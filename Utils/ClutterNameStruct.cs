using System.Collections.Generic;

namespace TYR_DeClutterer.Utils
{
    internal struct ClutterNameStruct
    {
        public Dictionary<string, bool> Garbage;
        public Dictionary<string, bool> Heaps;
        public Dictionary<string, bool> SpentCartridges;
        public Dictionary<string, bool> FoodDrink;
        public Dictionary<string, bool> Decals;
        public Dictionary<string, bool> Puddles;
        public Dictionary<string, bool> Shards;
    }
}