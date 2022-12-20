using System.Linq;

namespace HonkHeroGame
{
    public class Honk : GameObject
    {
        public Honk(double scale)
        {
            Tag = ElementType.HONK;

            Height = Constants.HONK_SIZE * scale;
            Width = Constants.HONK_SIZE * scale;
        }
    }
}

