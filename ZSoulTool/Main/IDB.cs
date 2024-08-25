using System;
using System.Collections.Generic;

//data for IDB offsets and such will be put here
//only the offsets here would need to be updated if anything should change in the future
namespace XV2SSEdit
{
    public partial class IDB
    {
        public static int Idb_Size = 772;
        public static int Stats_Start = 108;
        public static Dictionary<string, Tuple<string, int>> IdbOffsets = new Dictionary<string, Tuple<string, int>>
        {
            {"Index",           new Tuple<string, int>("Int16", 0)},
            {"Rarity",          new Tuple<string, int>("Int16", 2)},
            {"Name_ID",         new Tuple<string, int>("Int16", 4)},
            {"Info_ID",         new Tuple<string, int>("Int16", 6)},
            {"How_ID",          new Tuple<string, int>("Int16", 8)},
            {"I_10",            new Tuple<string, int>("Int16", 10)},
            {"Type",            new Tuple<string, int>("Int16", 12)},
            {"Dlc_Flag",        new Tuple<string, int>("Int16", 14)},
            {"I_16",            new Tuple<string, int>("Int16", 16)},
            {"I_18",            new Tuple<string, int>("Int16", 18)},
            {"Prog_Flag",       new Tuple<string, int>("Int16", 20)},
            {"I_22",            new Tuple<string, int>("Int16", 22)},
            {"Cost",            new Tuple<string, int>("Int32", 24)},
            {"Sell",            new Tuple<string, int>("Int32", 28)},
            {"Race",            new Tuple<string, int>("Int32", 32)},
            {"Cost_TP",         new Tuple<string, int>("Int32", 36)},
            {"Cost_STP",        new Tuple<string, int>("Int32", 40)},
            {"I_44",            new Tuple<string, int>("Int32", 44)},
            {"KiBlast",         new Tuple<string, int>("Int32", 48)},
            {"LB_Aura",         new Tuple<string, int>("Int16", 52)},
            {"LB_Color",        new Tuple<string, int>("Int16", 54)},
            {"LB_Desc",         new Tuple<string, int>("Int16", 56)},
            {"LB_Soul_ID1",     new Tuple<string, int>("Int16", 58)},
            {"LB_Soul_ID2",     new Tuple<string, int>("Int16", 60)},
            {"LB_Soul_ID3",     new Tuple<string, int>("Int16", 62)}
        };
        public static Dictionary<string, int> Effect_Start = new Dictionary<string, int>
        {
            {"_EB", 64},  //Basic Details
            {"_E1", 300}, //Effect Details 1
            {"_E2", 536}  //Effect Details 2
        };
        public static Dictionary<string, Tuple<string, int>> EffectOffsets = new Dictionary<string, Tuple<string, int>>
        {
            {"Effect",      new Tuple<string, int>("Int32", 0)},
            {"Activate",    new Tuple<string, int>("Int32", 4)},
            {"Limit",       new Tuple<string, int>("Int32", 8)},
            {"I_12",        new Tuple<string, int>("Int32", 12)},
            {"Duration",    new Tuple<string, int>("Float", 16)},
            {"Value1",      new Tuple<string, int>("Float", 20)},
            {"Value2",      new Tuple<string, int>("Float", 24)},
            {"Value3",      new Tuple<string, int>("Float", 28)},
            {"Value4",      new Tuple<string, int>("Float", 32)},
            {"Value5",      new Tuple<string, int>("Float", 36)},
            {"Value6",      new Tuple<string, int>("Float", 40)},
            {"Chance",      new Tuple<string, int>("Int32", 44)},
            {"I_48",        new Tuple<string, int>("Int32", 48)},
            {"I_52",        new Tuple<string, int>("Int32", 52)},
            {"I_56",        new Tuple<string, int>("Int32", 56)},
            {"Amount1",     new Tuple<string, int>("Float", 60)},
            {"Amount2",     new Tuple<string, int>("Float", 64)},
            {"Amount3",     new Tuple<string, int>("Float", 68)},
            {"Amount4",     new Tuple<string, int>("Float", 72)},
            {"Amount5",     new Tuple<string, int>("Float", 76)},
            {"Amount6",     new Tuple<string, int>("Float", 80)},
            {"Vfx_Type2",   new Tuple<string, int>("Int32", 84)},
            {"Vfx_ID2",     new Tuple<string, int>("Int32", 88)},
            {"Vfx_Type1",   new Tuple<string, int>("Int32", 92)},
            {"Vfx_ID1",     new Tuple<string, int>("Int32", 96)},
            {"I_100",       new Tuple<string, int>("Int32", 100)},
            {"Target",      new Tuple<string, int>("Int32", 104)},
        };
        //all stats are floats
        public static string[] StatNames = {
            "Max Health",
            "Max Ki",
            "Ki Restoration Rate",
            "Max Stamina",
            "Stamina Resoration Rate",
            "Stamina Damage to Enemy",
            "Stamina Damage to User",
            "Ground Speed",
            "Air Speed",
            "Boost Speed",
            "Dash Speed",
            "Basic Melee Damage",
            "Basic Ki Blast Damage",
            "Strike Skill Damage",
            "Ki Skill Damage",
            "Basic Melee Damage Taken",
            "Basic Ki Blast Damage Taken",
            "Strike Skill Damage Taken",
            "Ki Skill Damage Taken",
            "Transform Skill Duration",
            "Reinforcement Skill Duration",
            "Unknown 1",
            "HP Restored on Revive",
            "User Revive Speed",
            "Ally Revive Speed",
            "Unknown 2",
            "Assist Effect 1",
            "Assist Effect 2",
            "Assist Effect 3",
            "Assist Effect 4",
            "Assist Effect 5",
            "Assist Effect 6"
        };

        public static List<Tuple<string, string, string, string, string, int>> LimitBurstList = new List<Tuple<string, string, string, string, string, int>>
        {
            { new Tuple<string, string, string, string, string, int>("250", "0", "500", "501", "502", 1) }, //Atk Up
            { new Tuple<string, string, string, string, string, int>("251", "1", "520", "521", "522", 2) }, //DEF Up
            { new Tuple<string, string, string, string, string, int>("252", "2", "540", "541", "542", 3) }, //Auto Health
            { new Tuple<string, string, string, string, string, int>("253", "3", "560", "561", "562", 4) }, //Auto Just Guard
            { new Tuple<string, string, string, string, string, int>("253", "4", "580", "581", "582", 4) }, //Revive Gauge Auto-Recovery
            { new Tuple<string, string, string, string, string, int>("253", "5", "600", "601", "602", 4) }, //Death Beam ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "6", "605", "606", "607", 4) }, //Full Power Energy Wave ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "7", "610", "611", "612", 4) }, //Kamehameha ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "8", "615", "616", "617", 4) }, //Dynamite Kick ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "9", "620", "621", "622", 4) }, //Spirit Sword ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "10", "625", "626", "627", 4) }, //Justice Combination ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "11", "630", "631", "632", 4) }, //Super Kamehameha ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "12", "635", "636", "637", 4) }, //Time Skip/Molotov ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "13", "640", "641", "642", 4) }, //Final Kamehameha ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "14", "645", "646", "647", 4) }, //Sword of Hope ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "15", "648", "649", "650", 4) }, //Scatter Kamehameha ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "16", "651", "652", "653", 4) }, //Wolf Fang Fist ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "17", "654", "655", "656", 4) }, //Volleyball Fist ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "18", "657", "658", "659", 4) }, //Special Beam Cannon ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "19", "660", "661", "662", 4) }, //Masenko ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "20", "663", "664", "665", 4) }, //Double Sunday ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "21", "666", "667", "668", 4) }, //Arm Crash ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "22", "669", "670", "671", 4) }, //Galick Gun ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "23", "672", "673", "674", 4) }, //Cross Arm Dive ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "24", "675", "676", "677", 4) }, //Dodon Ray ATK Up
            { new Tuple<string, string, string, string, string, int>("253", "25", "678", "679", "680", 4) }, //Power Pole ATK Up
            { new Tuple<string, string, string, string, string, int>("-1", "0", "65535", "65535", "65535", 0) } //Nothing
        };
    }
}