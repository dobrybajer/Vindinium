using System;
using System.Collections.Generic;

namespace vindinium
{
    public enum Tile
    {
        IMPASSABLE_WOOD,
        FREE,
        HERO_1,
        HERO_2,
        HERO_3,
        HERO_4,
        TAVERN,
        GOLD_MINE_NEUTRAL,
        GOLD_MINE_1,
        GOLD_MINE_2,
        GOLD_MINE_3,
        GOLD_MINE_4
    }

    public class Direction
    {
        public const string Stay = "Stay";
        public const string North = "North";
        public const string East = "East";
        public const string South = "South";
        public const string West = "West";

        public static string GetRandomDirection()
        {
            var myList = new List<string> {Stay, North, East, South, West};

            var r = new Random();
            var index = r.Next(myList.Count);
            return myList[index];
        }

        public static string GetDirectionByNumber(int number)
        {
            switch (number)
            {
                case 0:
                    return Stay;
                case 1:
                    return West;
                case 2:
                    return North;
                case 3:
                    return East;
                case 4:
                    return South;
                default:
                    return Stay;
            }
        }
    }
}
