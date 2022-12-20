namespace HonkHeroGame
{
    public class PowerUp : GameObject
    {
        public PowerUpType PowerUpType { get; set; }

        public PowerUp()
        {
            Tag = ElementType.POWERUP;           
        }
    }

    public enum PowerUpType
    {
        NONE,
        FORCE_SHIELD,
        SLOW_DOWN_TIME,
        DOUBLE_SCORE,
        QUAD_SCORE,
    }
}

