using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using Windows.Foundation;

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

        //public double Health { get; set; } = 100;

        //public double HitPoints { get; set; } = 100;

        #endregion

        #region Methods

        public new void SetContent(Uri uri)
        {
            _vehicle.Source = new BitmapImage(uri);
            _vehicle.Visibility = Visibility.Visible;
        }

        public bool IsHonkBusted()
        {
            return HonkState == HonkState.HONKING_BUSTED && AttachedSticker is not null;
        }

        public bool CanBustHonk(Rect vehicleCloseHitBox, PlayerState playerState, Rect playerHitBox)
        {
            return HonkState == HonkState.HONKING && playerState == PlayerState.Attacking && playerHitBox.IntersectsWith(vehicleCloseHitBox);
        }

        public bool WaitForHonk(int gameLevel)
        {
            if (HonkState != HonkState.HONKING_BUSTED && WillHonk)
            {
                _honkCounter--;

                if (_honkCounter < 0)
                {
                    _honkCounter = SetHonkCounter(gameLevel);
                    UpdateHonkState(HonkState.HONKING);

                    return true;
                }
            }

            return false;
        }

        public void BustHonking(Sticker sticker)
        {
            IsMarkedForPopping = true;
            HasPopped = false;

            AttachedSticker = sticker;
            UpdateHonkState(HonkState.HONKING_BUSTED);
        }

        public void ResetHonking(int gameLevel)
        {
            IsMarkedForPopping = false;
            UpdateHonkState(HonkState.DEFAULT);
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

        private void UpdateHonkState(HonkState honkState)
        {
            HonkState = honkState;

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

