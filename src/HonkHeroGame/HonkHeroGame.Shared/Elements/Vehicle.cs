using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;

namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {
        #region Fields

        private int _honkCounter;
        private readonly int _honkCounterMin = 50;
        private readonly int _honkCounterMax = 150;
        private readonly Random _random = new();

        private readonly Grid _content = new();
        private readonly TextBlock _emoji = new() { Visibility = Visibility.Collapsed, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, FontSize = 45 };
        private readonly Image _vehicle = new() { Stretch = Stretch.Uniform, Visibility = Visibility.Collapsed };


        #endregion

        #region Ctor

        public Vehicle(double scale, double speed)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
            {
                SetHonkIndex();
                _honkCounter = SetHonkCounter();
            }

            Speed = speed;

            _content.Children.Add(_vehicle);
            _content.Children.Add(_emoji);

            SetChild(_content);
        }

        #endregion

        #region Properties

        public int HonkIndex { get; set; }

        public bool IsHonking { get; set; }

        public bool IsBusted { get; set; }

        public bool WillHonk { get; set; }

        public Sticker AttachedCollectible { get; set; }

        #endregion

        #region Methods

        public new void SetContent(Uri uri)
        {
            _vehicle.Source = new BitmapImage(uri);
            _vehicle.Visibility = Visibility.Visible;
        }

        public bool WaitForHonk()
        {
            if (!IsBusted && WillHonk)
            {
                _honkCounter--;

                if (_honkCounter < 0)
                {
                    _honkCounter = SetHonkCounter();
                    IsHonking = true;
                    SetEmoji("📢");

                    return true;
                }
            }

            return false;
        }

        public void BustHonking()
        {
            IsHonking = false;
            IsBusted = true;
            IsMarkedForPopping = true;
            HasPopped = false;
            SetEmoji("🔇");
        }

        public void ResetHonking()
        {
            IsHonking = false;
            IsBusted = false;
            IsMarkedForPopping = false;
            SetEmoji("");

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
            {
                SetHonkIndex();
                _honkCounter = SetHonkCounter();
            }
        }

        public void AttachCollectible(Sticker collectible)
        {
            AttachedCollectible = collectible;
        }

        private int SetHonkCounter()
        {
            return _random.Next(_honkCounterMin, _honkCounterMax);
        }

        private void SetHonkIndex()
        {
            HonkIndex = _random.Next(0, 3);
        }

        private void SetEmoji(string emoji)
        {
            _emoji.Text = emoji;
            _emoji.Visibility = emoji.IsNullOrBlank() ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}

