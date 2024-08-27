using System;
using System.Collections.Generic;

namespace XV2SSEdit
{
    public partial class IDB
    {
        //all offset data goes here
        //if the game ever updates the IDB format again, i can just use this.
        //maybe i can allow for backwards compatability this way too? hmm...
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

        //for quick setting limit bursts
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

        //All known values for souls go here. Basically takes over effect data
        //cb index, (id, name)
        //public static Dictionary<int, Tuple<int, string>> KiBlastList = new Dictionary<int, Tuple<int, string>>
        //{
        //    { 0,  new Tuple<int, string>(0, "Race Default") },
        //    { 1,  new Tuple<int, string>(1, "Power") },
        //    { 2,  new Tuple<int, string>(2, "Rush") },
        //    { 3,  new Tuple<int, string>(3, "Bomb") },
        //    { 4,  new Tuple<int, string>(4, "Homing") },
        //    { 5,  new Tuple<int, string>(5, "Paralyze") },
        //    { 6,  new Tuple<int, string>(6, "Golden Form Beam (FRI)") },
        //    { 7,  new Tuple<int, string>(7, "Purification Stomp (MAP)") },
        //    { 8,  new Tuple<int, string>(8, "Rush (Hearts)") },
        //    { 9,  new Tuple<int, string>(9, "Stone") },
        //    { 10, new Tuple<int, string>(10, "Gamma Beam (Red)") },
        //    { 11, new Tuple<int, string>(11, "Gamma Beam (Blue)") }
        //};

        //public static Dictionary<int, Tuple<short, string>> LBColorList = new Dictionary<int, Tuple<short, string>>
        //{
        //    { 0,  new Tuple<short, string>(-1, "Nothing") },
        //    { 1,  new Tuple<short, string>(0, "Red") },
        //    { 2,  new Tuple<short, string>(1, "Blue") },
        //    { 3,  new Tuple<short, string>(2, "Green") },
        //    { 4,  new Tuple<short, string>(3, "Purple") }
        //};

        //public static Dictionary<int, Tuple<int, string>> VfxList = new Dictionary<int, Tuple<int, string>>
        //{
        //    { 0,  new Tuple<int, string>(-1, "Nothing") },
        //    { 1,  new Tuple<int, string>(0, "Common") },
        //    { 2,  new Tuple<int, string>(1, "Aura") },
        //    { 3,  new Tuple<int, string>(2, "Ki Blast") },
        //    { 4,  new Tuple<int, string>(6, "Common 2") }
        //};

        //public static Dictionary<int, Tuple<int, string>> TargetList = new Dictionary<int, Tuple<int, string>>
        //{
        //    { 0,  new Tuple<int, string>(-1, "Nothing") },
        //    { 1,  new Tuple<int, string>(0, "Self") },
        //    { 2,  new Tuple<int, string>(1, "Self and Allies") },
        //    { 3,  new Tuple<int, string>(2, "Enemies") },
        //    { 4,  new Tuple<int, string>(3, "All but Self") },
        //    { 5,  new Tuple<int, string>(4, "Everyone") },
        //    { 6,  new Tuple<int, string>(5, "Targeted Enemy") },
        //    { 7,  new Tuple<int, string>(6, "Unknown") },
        //    { 8,  new Tuple<int, string>(7, "Unknown") },
        //    { 9,  new Tuple<int, string>(8, "Unknown") },
        //    { 10,  new Tuple<int, string>(9, "Unknown") }
        //};

        public static SortedList<int, string> KiBlastList = new SortedList<int, string>
        {
            { 0, "Race Default" },
            { 1, "Power" },
            { 2, "Rush" },
            { 3, "Bomb" },
            { 4, "Homing" },
            { 5, "Paralyze" },
            { 6, "Golden Form Beam (FRI)" },
            { 7, "Purification Stomp (MAP)" },
            { 8, "Rush (Hearts)" },
            { 9, "Stone" },
            { 10, "Gamma Beam (Red)" },
            { 11, "Gamma Beam (Blue)" },
            { 12, "Soaring Fist*" }
        };

        public static SortedList<short, string> LBColorList = new SortedList<short, string>
        {
            { -1, "Nothing" },
            { 0, "Red" },
            { 1, "Blue" },
            { 2, "Green" },
            { 3, "Purple" }
        };

        public static SortedList<int, string> VfxList = new SortedList<int, string>
        {
            { -1, "Nothing" },
            { 0, "Common" },
            { 1, "Aura" },
            { 2, "Ki Blast" },
            { 6, "Common 2" }
        };

        public static SortedList<int, string> TargetList = new SortedList<int, string>
        {
            { -1, "Nothing" },
            { 0, "Self" },
            { 1, "Self and Allies" },
            { 2, "Enemies" },
            { 3, "All but Self" },
            { 4, "Everyone" },
            { 5, "Targeted Enemy" },
            { 6, "6 - Unknown" },
            { 7, "7 - Unknown" },
            { 8, "8 - Unknown" },
            { 9, "9 - Unknown" }
        };

        public static SortedList<int, string> ActivatorList = new SortedList<int, string>
        {
            {-1, "No Activation"},
            {0, "Always"},
            {1, "Outside Activation (PUP/Other)"},
            {2, "User is KO'ed"},
            {3, "Ally is KO'ed"},
            {4, "Enemy is KO'ed"},
            {5, "User KO's Enemy"},
            {6, "Health is Less Than X% (Duration)"},
            {7, "Super Skill is activated"},
            {8, "Ultimate Skill is activated"},
            {9, "Evasive Skill is activated"},
            {10, "User is struck by a Super Skill"},
            {11, "User is struck by an Ultimate Skill"},
            {12, "User is struck by an Evasive Skill"},
            {13, "Reinforcement is Activated"},
            {14, "Transformation is Activated"},
            {15, "User Revived by Ally"},
            {16, "User Revives Ally"},
            {17, "Enemy Revived"},
            {18, "Damage Dealt is at Least X%"},
            {19, "Damage Taken is at Least X%"},
            {20, "After Battle Begins (Delay)"},
            {21, "Health is Greater than X% (Constant)"},
            {22, "KI is Greater Than X%"},
            {23, "Stamina is Greater Than X%"},
            {24, "Health is Less Than X% (Constant)"},
            {25, "Ki is Less Than X%"},
            {26, "Stamina is Less Than X%"},
            {27, "After User is Revived"},
            {28, "Awoken Skill is Activated/Deactivated"},
            {29, "During User Revival"},
            {30, "Firing Charged Basic Ki Blast"},
            {31, "Charging Basic KI blast"},
            {32, "While Reviving"},
            {33, "User Just-Guards an attack"},
            {34, "User's stamina is broken (Enemy)"},
            {35, "Awoken and Reinforcement Skill Activated"},
            {36, "Heavy Smash hits user"},
            {37, "Unknown"},
            {38, "Enemy is struck by a Super Skill"},
            {39, "Enemy is struck by an Ultimate Skill"},
            {40, "Enemy is struck by an Evasive Skill"},
            {41, "Charged KI blast Hits"},
            {42, "Heavy Smash hits enemy"},
            {43, "Unknown"},
            {44, "Charged Attack hits enemy"},
            {45, "Charged Attack hits user"},
            {46, "User's stamina is broken (Any)"},
            {47, "Counter/Afterimage skill Activated"},
            {48, "Super Skill Duration"},
            {49, "Ultimate Skill Duration"},
            {50, "Evasive Skill Duration"},
            {51, "Unknown"},
            {52, "Specific Awoken stage is activated"},
            {53, "KO'd with teammates(?)"},
            {54, "Unknown"},
            {55, "Unknown"},
            {56, "Unknown"},
            {57, "Never Activate"},
            {58, "User hit by Super Skill"},
            {59, "User hit by Ultimate Skill"},
            {60, "User hit by Evasive Skill"},
            {61, "Ally KO (Temporary)?"},
            {62, "Duration of Pose Skill effect"},
            {63, "Unknown"},
            {64, "User Grabs Enemy"},
            {65, "Super Skill activated Alt."},
            {66, "Ultimate Skill activated Alt."},
            {67, "Evasive Skill activated Alt."},
            {68, "Unknown"},
            {69, "Unknown"},
            {70, "Unknown"},
            {71, "Specific Awoken Skill is activated"},
            {72, "Unknown"},
            {73, "Unknown"},
            {74, "Unknown"},
            {75, "Unknown"},
            {76, "Unknown"},
            {77, "Unknown"},
            {78, "Unknown"},
            {79, "Unknown"},
            {80, "Unknown"},
            {81, "Unknown"},
            {82, "Unknown"},
            {83, "Unknown"},
            {84, "Unknown"},
            {85, "Unknown"},
            {86, "Unknown"},
            {87, "Unknown"},
            {88, "Unknown"},
            {89, "Character Bac Entry Used"},
            {90, "Ally Uses Assist Effect on User"},
            {91, "Unknown"},
            {92, "Current Ki is x%"},
            {93, "Unknown"},
            {94, "Unknown"},
            {95, "Unknown"},
            {96, "Unknown"},
            {97, "Unknown"},
            {98, "Unknown"},
            {99, "Unknown"},
            {100, "Unknown"},
            {101, "Unknown"},
            {102, "Unknown"},
            {103, "Unknown"},
            {104, "Unknown"},
            {105, "Unknown"},
            {106, "Unknown"},
            {107, "Unknown"},
            {108, "Unknown"},
            {109, "Unknown"},
            {110, "Unknown"},
            {111, "Unknown"},
            {112, "Unknown"},
            {113, "Unknown"},
            {114, "Unknown"},
            {115, "Unknown"},
            {116, "Unknown"},
            {117, "Unknown"},
            {118, "Unknown"},
            {119, "Unknown"},
            {120, "Unknown"},
            {121, "Unknown"},
            {122, "Unknown"},
            {123, "Unknown"},
            {124, "Unknown"},
            {125, "Unknown"},
            {126, "Unknown"}
        };

        public static SortedList<int, string> EffectList = new SortedList<int, string>
        {
            {-1, "No Effect"},
            {0, "Stat Modifier"},
            {1, "Nullifies Damage (?)"},
            {2, "Restores/Drains Health"},
            {3, "Activates Ki Auto-recovery"},
            {4, "Restores/Drains Ki"},
            {5, "Activates Stamina Auto-recovery"},
            {6, "Restores/Drains Stamina"},
            {7, "Unknown"},
            {8, "Unknown"},
            {9, "Revive all down teamates to full (?)"},
            {10, "Revive after KO (?)"},
            {11, "Revive after KO with a X% of HP (Amount)(?)"},
            {12, "Zeni earned multiplier"},
            {13, "Score earned multiplier"},
            {14, "Capsule recovery speed multiplier"},
            {15, "Seal Guard"},
            {16, "Activates Auto-Guard"},
            {17, "Activates Just-Guard"},
            {18, "Super Armor (Basic)"},
            {19, "Super Armor (Ungrabbable) "},
            {20, "Super Armor (Stamina Breakable)"},
            {21, "Seal Super Skills"},
            {22, "Seal Ultimate Skills"},
            {23, "Seal Evasive Skills"},
            {24, "Seal all Skills"},
            {25, "No Ki depletion"},
            {26, "Unknown"},
            {27, "No Stamina depletion"},
            {28, "No Ki or Stamina depletion"},
            {29, "Unused/Nothing"},
            {30, "Unused/Nothing"},
            {31, "Nullifies Super skill damage (except grabs)"},
            {32, "Nullifies Ultimate skill damage (except grabs)"},
            {33, "Nullifies Evasive skill damage (except grabs)"},
            {34, "Nullifies poison/movement reduction"},
            {35, "Activates Candy Status Ailment"},
            {36, "Unknown"},
            {37, "Nullifies Candy Status Ailment"},
            {38, "Unknown"},
            {39, "Unknown"},
            {40, "Nullifies Heavy Break stun"},
            {41, "Activates Poison Status Ailment"},
            {42, "Unknown"},
            {43, "Nullifies Poison Status Ailment"},
            {44, "Activates Slow Status Ailment"},
            {45, "Unknown"},
            {46, "Nullifies Slow Status Ailment"},
            {47, "Activates Lock-On Status Ailment"},
            {48, "Unknown"},
            {49, "Nullifies Lock-On Status Ailment"},
            {50, "Unknown"},
            {51, "Nullifies Blind Status Ailment"},
            {52, "Activates Freeze Status Ailment (Alt)"},
            {53, "Nullifies Paralyze Status Ailment"},
            {54, "Activates Sleep Status Ailment"},
            {55, "Nullifies Sleep Status Ailment"},
            {56, "Activates Paralyze Status Ailment (XV1 Only)"},
            {57, "Nullifies Burn Status Ailment"},
            {58, "Activates Freeze Status Ailment"},
            {59, "Nullifies Petrify Status Ailment"},
            {60, "Nullifies All Status Ailments"},
            {61, "Change damage done with Super Skills (Value)"},
            {62, "Change damage done with Ultimate Skills (Value)"},
            {63, "Change damage done with Evasive Skills (Value)"},
            {64, "Super skills damage recieved multiplier (Value)"},
            {65, "Ultimate skills damage recieved multiplier (Value)"},
            {66, "Evasive skills damage recieved multiplier (Value)"},
            {67, "Super Armor (auto-reactivates)"},
            {68, "Ki &amp; Stam regen multiplier"},
            {69, "Restores/Drains Ki &amp; Stamina"},
            {70, "Activates Auto-Dodge 1 (Boss)"},
            {71, "Charged Ki Blasts damage dealt multiplier"},
            {72, "Charged Ki Blasts damage recieved multiplier"},
            {73, "Unknown"},
            {74, "Change damage of Kamehameha Type Skills (Amount)"},
            {75, "Attack VS females Damage Multiplier (Amount)"},
            {76, "Attack VS males Damage Multiplier (Amount)"},
            {77, "Seal Awoken Skills"},
            {78, "Super Armor (Damage Linked) ?"},
            {79, "Guard Break recovery time multiplier (Amount)"},
            {80, "Unused/Nothing"},
            {81, "Charged Ki Blast Paralysis length multiplier"},
            {82, "Boosts abilities based on number of allies of same race"},
            {83, "Activates Poison &amp; Slow Status Ailments"},
            {84, "Basic Ki Blast damage multiplier (Amnt/Time)"},
            {85, "Z-Vanish Cost multiplier (Amount)"},
            {86, "Super Armor (Decaying)(?)"},
            {87, "Activates automatic revival gauge recovery"},
            {88, "Unknown"},
            {89, "Make User Ungrabbable"},
            {90, "Activates Auto-Dodge 2 (Data Input)"},
            {91, "Activates Super Armor 1 (Crystal Raid)"},
            {92, "Activates Super Armor 2 (Crystal Raid)"},
            {93, "Activates Auto-Dodge 3 (NPC)"},
            {94, "Unknown"},
            {95, "Unknown"},
            {96, "Restores/Drains Health (Absolute)"},
            {97, "Restores/Drains Health (Percentage)"},
            {98, "Restores/Drains Ki (Absolute)"},
            {99, "Restores/Drains Ki (Percentage)"},
            {100, "Restores/Drains Stamina (Absolute)"},
            {101, "Restores/Drains Stamina (Percentage)"},
            {102, "Stat Increase based on Assist Effect Total"},
            {103, "Unknown"},
            {104, "Unknown"},
            {105, "Unknown"},
            {106, "Unknown"},
            {107, "Unknown"},
            {108, "Unknown"},
            {109, "Unknown"},
            {110, "Unknown"},
            {111, "Unknown"},
            {112, "Unknown"},
            {113, "Unknown"},
            {114, "Unknown"},
            {115, "Unknown"},
            {116, "Unknown"},
            {117, "Unknown"},
            {118, "Unknown"},
            {119, "Unknown"},
            {120, "Unknown"}
        };

    }
}