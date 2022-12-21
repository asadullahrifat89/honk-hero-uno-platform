using System;
using System.Collections.Generic;

namespace HonkHeroGame
{
    public static class Constants
    {
        public const string GAME_ID = "honk-hero";
        public const string COMPANY_ID = "selise";

        #region Measurements

        public const double DEFAULT_FRAME_TIME = 18;

        public const double VEHICLE_SIZE = 210;

        public const double PLAYER_WIDTH = 80;
        public const double PLAYER_HEIGHT = 80;

        public const double STICKER_SIZE = 70;

        public const double HONK_SIZE = 100;

        public const double POWERUP_SIZE = 70;

        public const double COLLECTIBLE_SIZE = 90;

        public const double HEALTH_WIDTH = 80;
        public const double HEALTH_HEIGHT = 80;

        public const double CLOUD_WIDTH = 150;
        public const double CLOUD_HEIGHT = 100;

        public const double ISLAND_WIDTH = 600;
        public const double ISLAND_HEIGHT = 600;

        #endregion

        #region Images

        public static KeyValuePair<ElementType, Uri>[] ELEMENT_TEMPLATES = new KeyValuePair<ElementType, Uri>[]
        {
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle2.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle3.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle4.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle5.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle6.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle7.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle8.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle9.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle10.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle11.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.VEHICLE, new Uri("ms-appx:///Assets/Images/vehicle12.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER, new Uri("ms-appx:///Assets/Images/player_idle.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_FLYING, new Uri("ms-appx:///Assets/Images/player_fly.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_POINTING, new Uri("ms-appx:///Assets/Images/player_pointing.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.PLAYER_APPRECIATING, new Uri("ms-appx:///Assets/Images/player_appreciating.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.STICKER, new Uri("ms-appx:///Assets/Images/sticker.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.HEALTH, new Uri("ms-appx:///Assets/Images/health.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.HONK, new Uri("ms-appx:///Assets/Images/honk1.png")),
            new KeyValuePair<ElementType, Uri>(ElementType.HONK, new Uri("ms-appx:///Assets/Images/honk2.png")),

            new KeyValuePair<ElementType, Uri>(ElementType.COLLECTIBLE, new Uri("ms-appx:///Assets/Images/collectible.png")),
        };

        #endregion

        #region Sounds

        public static KeyValuePair<SoundType, string>[] SOUND_TEMPLATES = new KeyValuePair<SoundType, string>[]
        {
            new KeyValuePair<SoundType, string>(SoundType.MENU_SELECT, "Assets/Sounds/menu-select.mp3"),

            //new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro1.mp3"),
            //new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.INTRO, "Assets/Sounds/intro.mp3"),            

            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.BACKGROUND, "Assets/Sounds/background3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk2.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK, "Assets/Sounds/honk3.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HONK_BUST, "Assets/Sounds/honk_bust1.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HONK_BUST, "Assets/Sounds/honk_bust2.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.GAME_OVER, "Assets/Sounds/game-over.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.POWER_UP, "Assets/Sounds/power-up.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.POWER_DOWN, "Assets/Sounds/power-down.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.HEALTH_GAIN, "Assets/Sounds/health-gain.mp3"),
            new KeyValuePair<SoundType, string>(SoundType.HEALTH_LOSS, "Assets/Sounds/health-loss.mp3"),

            new KeyValuePair<SoundType, string>(SoundType.COLLECTIBLE, "Assets/Sounds/collectible_collected.mp3"),
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
