using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;

namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {
        #region Fields

        private int _honkCounter;

        private readonly Random _random = new();
        private readonly Grid _content = new();
        private readonly Image _emoji = new()
        {
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };
        private readonly Image _vehicle = new()
        {
            Stretch = Stretch.Uniform,
            Visibility = Visibility.Collapsed
        };

        #endregion

        #region Ctor

        public Vehicle(double scale, double speed)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
                SetHonkIndex();

            Speed = speed;

            _emoji.Height = 55 * scale;
            _emoji.Margin = new Thickness(0, 15 * scale, 0, 0);

            _content.Children.Add(_vehicle);
            _content.Children.Add(_emoji);

            SetChild(_content);
        }

        #endregion

        #region Properties

        public int HonkIndex { get; set; }

        public HonkState HonkState { get; set; }

        public bool WillHonk { get; set; }

        public Sticker AttachedSticker { get; set; }

        #endregion

        #region Methods

        public new void SetContent(Uri uri)
        {
            _vehicle.Source = new BitmapImage(uri);
            _vehicle.Visibility = Visibility.Visible;
        }

        public bool WaitForHonk()
        {
            if (HonkState != HonkState.HONKING_BUSTED && WillHonk)
            {
                _honkCounter--;

                if (_honkCounter < 0)
                {
                    _honkCounter = SetHonkCounter();
                    HonkState = HonkState.HONKING;
                    SetEmoji(HonkState);

                    return true;
                }
            }

            return false;
        }

        public void BustHonking()
        {
            HonkState = HonkState.HONKING_BUSTED;
            IsMarkedForPopping = true;
            HasPopped = false;
            SetEmoji(HonkState);
        }

        public void ResetHonking()
        {
            HonkState = HonkState.DEFAULT;
            IsMarkedForPopping = false;
            SetEmoji(HonkState.DEFAULT);

            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
            {
                SetHonkIndex();
                _honkCounter = SetHonkCounter();
            }
        }

        public void AttachSticker(Sticker collectible)
        {
            AttachedSticker = collectible;
        }

        private int SetHonkCounter()
        {
            return _random.Next(50, 150);
        }

        private void SetHonkIndex()
        {
            HonkIndex = _random.Next(0, 3);
        }

        private void SetEmoji(HonkState honkState)
        {
            switch (honkState)
            {
                case HonkState.DEFAULT:
                    _emoji.Visibility = Visibility.Collapsed;
                    break;
                case HonkState.HONKING:
                    {
                        _emoji.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING).Value);
                        _emoji.Visibility = Visibility.Visible;
                    }
                    break;
                case HonkState.HONKING_BUSTED:
                    {
                        _emoji.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING_BUSTED).Value);
                        _emoji.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    _emoji.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        #endregion
    }

    public enum HonkState
    {
        DEFAULT,
        HONKING,
        HONKING_BUSTED,
    }
}

