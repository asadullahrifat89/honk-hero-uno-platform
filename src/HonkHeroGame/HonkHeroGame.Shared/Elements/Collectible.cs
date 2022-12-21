using System.Linq;

namespace HonkHeroGame
{
    public class Collectible : GameObject
    {
        #region Ctor

        public Collectible(double scale)
        {
            Tag = ElementType.COLLECTIBLE;

            Height = Constants.COLLECTIBLE_SIZE * scale;
            Width = Constants.COLLECTIBLE_SIZE * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.COLLECTIBLE).Value);

            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50 * scale);
        }

        #endregion
    }
}
