using System;
using System.Collections.Generic;

namespace HonkHeroGame
{
    public static class Constants
    {
        public const string GAME_ID = "honk-hero";
        public const string COMPANY_ID = "horen-maare";

        #region Measurements

        public const double DEFAULT_FRAME_TIME = 18;

        public const double VEHICLE_SIZE = 195;
        public const double BOSS_VEHICLE_SIZE = 350;

        public const double PLAYER_WIDTH = 90;
        public const double PLAYER_HEIGHT = 90;

        public const double STICKER_SIZE = 40;

        public const double HONK_SIZE = 100;

        public const double POWERUP_SIZE = 80;

        public const double COLLECTIBLE_SIZE = 65;

        #endregion

        #region Images

        public static KeyValuePair<ElementType, Uri>[] ELEMENT_TEMPLATES = new KeyValuePair<ElementType, Uri>[]
        {
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle1_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle2_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle3_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle4_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle5_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle6_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle7_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle8_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle9_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle10_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle11_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle12_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle13_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle14_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle15_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle16_up.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_UPWARD, new Uri("ms-appx:///Assets/Images/vehicle17_up.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle1_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle2_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle3_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle4_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle5_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle6_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle7_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle8_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle9_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle10_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle11_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle12_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle13_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle14_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle15_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle16_down.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_DOWNWARD, new Uri("ms-appx:///Assets/Images/vehicle17_down.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player_idle.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_FLYING, new Uri("ms-appx:///Assets/Images/player_flying.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_ATTACKING, new Uri("ms-appx:///Assets/Images/player_attacking.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.STICKER, new Uri("ms-appx:///Assets/Images/sticker.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.HONK, new Uri("ms-appx:///Assets/Images/honk1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.HONK, new Uri("ms-appx:///Assets/Images/honk2.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible.png")),
                        
            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.POWERUP, new Uri("ms-appx:///Assets/Images/powerup2.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.HONKING, new Uri("ms-appx:///Assets/Images/honking.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.HONKING_BUSTED, new Uri("ms-appx:///Assets/Images/honking-busted.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.ROAD_DECORATION, new Uri("ms-appx:///Assets/Images/road-side-buildings-left.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.ROAD_DECORATION, new Uri("ms-appx:///Assets/Images/road-side-buildings-right.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_BOSS, new Uri("ms-appx:///Assets/Images/boss1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE_BOSS, new Uri("ms-appx:///Assets/Images/boss2.png")),
        };

        #endregion

        #region Sounds

        public static KeyValuePair<SoundType, string>[] SOUND_TEMPLATES = new KeyValuePair<SoundType, string>[]
        {
            new KeyValuePair<SoundType, string>(SoundType.MENU_SELECT, "Assets/Sounds/menu-select.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song3.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song4.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song5.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song6.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song7.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song8.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song9.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.SONG, "Assets/Sounds/song10.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.AMBIENCE, "Assets/Sounds/ambience1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.AMBIENCE, "Assets/Sounds/ambience2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.AMBIENCE, "Assets/Sounds/ambience3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk3.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk4.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk5.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk6.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk7.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HONK_BUST, "Assets/Sounds/honk_bust1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK_BUST, "Assets/Sounds/honk_bust2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.GAME_OVER, "Assets/Sounds/game-over.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.POWER_UP, "Assets/Sounds/power-up.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.POWER_DOWN, "Assets/Sounds/power-down.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/collectible-collected1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/collectible-collected2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/collectible-collected3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.LEVEL_UP, "Assets/Sounds/level-up.mp3"),
            
            new KeyValuePair<SoundType, string>(SoundType.BOSS_ENTRY, "Assets/Sounds/boss-entry.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BOSS_IDLING, "Assets/Sounds/boss-idling.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.BOSS_HONK, "Assets/Sounds/boss-honk1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BOSS_HONK, "Assets/Sounds/boss-honk2.mp3"),

        };

        #endregion

        #region Web Api Endpoints

        public const string Action_Ping = "/api/Query/Ping";

        public const string Action_Authenticate = "/api/Command/Authenticate";
        public const string Action_SignUp = "/api/Command/SignUp";
        public const string Action_SubmitGameScore = "/api/Command/SubmitGameScore";
        public const string Action_GenerateSession = "/api/Command/GenerateSession";
        public const string Action_ValidateToken = "/api/Command/ValidateToken";

        public const string Action_GetGameProfile = "/api/Query/GetGameProfile";
        public const string Action_GetGameProfiles = "/api/Query/GetGameProfiles";
        public const string Action_GetGameScoresOfTheDay = "/api/Query/GetGameScoresOfTheDay";
        public const string Action_CheckIdentityAvailability = "/api/Query/CheckIdentityAvailability";
        public const string Action_GetSeason = "/api/Query/GetSeason";
        public const string Action_GetGamePrizeOfTheDay = "/api/Query/GetGamePrizeOfTheDay";
        public const string Action_GetCompany = "/api/Query/GetCompany";

        #endregion

        #region Session Keys

        public const string CACHE_REFRESH_TOKEN_KEY = "RT";
        public const string CACHE_LANGUAGE_KEY = "Lang";

        #endregion

        #region Cookie Keys

        public const string COOKIE_KEY = "Cookie";
        public const string COOKIE_ACCEPTED_KEY = "Accepted";

        #endregion
    }
}
