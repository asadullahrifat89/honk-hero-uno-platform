using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Linq;

namespace HonkHeroGame
{
    public class Sticker : GameObject
    {
        public Sticker(double scale)
        {
            Tag = ElementType.STICKER;

            Height = Constants.STICKER_SIZE * scale;
            Width = Constants.STICKER_SIZE * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.STICKER).Value);

            //CornerRadius = new Microsoft.UI.Xaml.CornerRadius(5);
            //BorderBrush = new SolidColorBrush(Colors.Black);
            //BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
        }
    }
}

