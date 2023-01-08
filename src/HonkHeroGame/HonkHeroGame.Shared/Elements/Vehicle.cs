using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Polly;
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
        private readonly Image _honkingBusted = new()
        {
            Opacity = 0,
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

        public Vehicle(
            double scale,
            double speed,
            int gameLevel,
            int honkTemplatesCount,
            StreamingDirection streamingDirection = StreamingDirection.UpWard,
            VehicleClass vehicleClass = VehicleClass.DEFAULT_CLASS)
        {
            Tag = ElementType.VEHICLE;

            VehicleClass = vehicleClass;

            switch (VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    {
                        Height = Constants.VEHICLE_SIZE * scale;
                        Width = Constants.VEHICLE_SIZE * scale;

                        SetHonk(gameLevel: gameLevel, honkTemplatesCount: honkTemplatesCount, willHonk: Convert.ToBoolean(_random.Next(0, 2)));
                    }
                    break;
                case VehicleClass.BOSS_CLASS:
                    {
                        Height = Constants.BOSS_VEHICLE_SIZE * scale;
                        Width = Constants.BOSS_VEHICLE_SIZE * scale;

                        Health = 100 * (gameLevel / 2);

                        SetHonk(gameLevel: gameLevel, honkTemplatesCount: honkTemplatesCount, willHonk: true);
                    }
                    break;
                default:
                    break;
            }

            Speed = speed;
            StreamingDirection = streamingDirection;

            _honking.Height = 50 * scale;
            _honking.Margin = new Thickness(0, 15 * scale, 0, 0);
            _honking.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING).Value);

            _honkingBusted.Height = 50 * scale;
            _honkingBusted.Margin = new Thickness(0, 15 * scale, 0, 0);
            _honkingBusted.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING_BUSTED).Value);

            _content.Children.Add(_vehicle);
            _content.Children.Add(_honking);
            _content.Children.Add(_honkingBusted);

            SetChild(_content);
        }

        #endregion

        #region Properties

        public int HonkSoundIndex { get; set; }

        public bool WillHonk { get; set; }

        public HonkState HonkState { get; set; }

        public Sticker AttachedSticker { get; set; }

        public StreamingDirection StreamingDirection { get; set; } = StreamingDirection.UpWard;

        public VehicleClass VehicleClass { get; set; } = VehicleClass.DEFAULT_CLASS;

        public VehicleIntent VehicleIntent { get; set; } = VehicleIntent.MOVE;

        public double Health { get; set; } = 100;

        public double HitPoints { get; set; } = 0.01;

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

        public (bool IsHonkingBusted, double Health, bool HasTakenDamage) BustHonking()
        {
            bool isHonkBusted = false;
            bool hasTakenDamage = false;

            IsMarkedForPopping = true;
            HasPopped = false;

            switch (VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    {
                        Health = 0;
                        UpdateHonkState(HonkState.HONKING_BUSTED);
                        isHonkBusted = true;
                        hasTakenDamage = true;
                    }
                    break;
                case VehicleClass.BOSS_CLASS:
                    {
                        Health -= HitPoints;
                        hasTakenDamage = true;

                        if (Health <= 0)
                        {
                            UpdateHonkState(HonkState.HONKING_BUSTED);
                            isHonkBusted = true;
                        }


                    }
                    break;
                default:
                    break;
            }

            return (isHonkBusted, Health, hasTakenDamage);
        }

        public void ResetHonking(int gameLevel, int honkTemplatesCount, bool willHonk)
        {
            IsMarkedForPopping = false;

            UpdateHonkState(HonkState.DEFAULT);

            SetHonk(
                gameLevel: gameLevel,
                honkTemplatesCount: honkTemplatesCount,
                willHonk: willHonk);
        }

        private void SetHonk(int gameLevel, int honkTemplatesCount, bool willHonk)
        {
            WillHonk = willHonk;

            if (WillHonk)
            {
                SetHonkIndex(honkTemplatesCount);
                _honkCounter = SetHonkCounter(gameLevel);
            }
        }

        private int SetHonkCounter(int gameLevel)
        {
            var halfGameLevel = gameLevel / 2;

            int count = _random.Next(55 - (int)Math.Floor(0.2 * halfGameLevel), 125 - (int)Math.Floor(0.4 * halfGameLevel));

            switch (VehicleClass)
            {
                case VehicleClass.BOSS_CLASS:
                    count = count * 2;
                    break;
                default:
                    break;
            }

            return count;
        }

        private void SetHonkIndex(int honkTemplatesCount)
        {
            HonkSoundIndex = _random.Next(0, honkTemplatesCount);
        }

        private void UpdateHonkState(HonkState honkState)
        {
            HonkState = honkState;

            switch (honkState)
            {
                case HonkState.DEFAULT:
                    {
                        _honking.Opacity = 0;
                        _honkingBusted.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING:
                    {
                        _honking.Opacity = 1;
                        _honkingBusted.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING_BUSTED:
                    {
                        _honking.Opacity = 0;
                        _honkingBusted.Opacity = 1;
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

    public enum VehicleIntent
    {
        MOVE,
        IDLE,
    }

    public enum StreamingDirection
    {
        UpWard,
        DownWard,
    }

    public enum VehicleClass
    {
        DEFAULT_CLASS,
        BOSS_CLASS,
    }
}

