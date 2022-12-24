namespace HonkHeroGame
{
    public class Honk : GameObject
    {
        public Honk(double scale, double speed)
        {
            Tag = ElementType.HONK;

            Height = Constants.HONK_SIZE * scale;
            Width = Constants.HONK_SIZE * scale;

            Speed = speed;
        }
    }
}

