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

        private readonly Image _honking = new()
        {
            Opacity = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };
        //private readonly Image _honkingBusted = new()
        //{
        //    Opacity = 0,
        //    HorizontalAlignment = HorizontalAlignment.Center,
        //    VerticalAlignment = VerticalAlignment.Top,
        //};
        private readonly Image _vehicle = new()
        {
            Stretch = Stretch.Uniform,
            Visibility = Visibility.Collapsed
        };

        #endregion

        #region Ctor

        public Vehicle(double scale, double speed, int gameLevel)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;

            Speed = speed;

            _honking.Height = 55 * scale;
            _honking.Margin = new Thickness(0, 15 * scale, 0, 0);
            _honking.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING).Value);

            //_honkingBusted.Height = 55 * scale;
            //_honkingBusted.Margin = new Thickness(0, 15 * scale, 0, 0);
            //_honkingBusted.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING_BUSTED).Value);

            _content.Children.Add(_vehicle);
            _content.Children.Add(_honking);
            //_content.Children.Add(_honkingBusted);

            SetChild(_content);
            SetHonk(gameLevel);
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

        public void AttachSticker(Sticker collectible)
        {
            AttachedSticker = collectible;
        }

        public bool WaitForHonk(int gameLevel)
        {
            if (HonkState != HonkState.HONKING_BUSTED && WillHonk)
            {
                _honkCounter--;

                if (_honkCounter < 0)
                {
                    _honkCounter = SetHonkCounter(gameLevel);
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

        public void ResetHonking(int gameLevel)
        {
            HonkState = HonkState.DEFAULT;
            IsMarkedForPopping = false;
            SetEmoji(HonkState.DEFAULT);
            SetHonk(gameLevel);
        }

        private void SetHonk(int gameLevel)
        {
            WillHonk = Convert.ToBoolean(_random.Next(0, 2));

            if (WillHonk)
            {
                SetHonkIndex();
                _honkCounter = SetHonkCounter(gameLevel);
            }
        }

        private int SetHonkCounter(int gameLevel)
        {
            var halfGameLevel = gameLevel / 2;
            return _random.Next(50 - (int)Math.Floor(0.2 * halfGameLevel), 120 - (int)Math.Floor(0.4 * halfGameLevel));
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
                    {
                        _honking.Opacity = 0;
                        //_honkingBusted.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING:
                    {
                        _honking.Opacity = 1;
                        //_honkingBusted.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING_BUSTED:
                    {
                        _honking.Opacity = 0;
                        //_honkingBusted.Opacity = 1;
                    }
                    break;
                default:
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

