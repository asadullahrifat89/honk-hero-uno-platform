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

        private readonly Image _honkingIndicator = new()
        {
            Opacity = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };

        private readonly Image _honkingBustedIndicator = new()
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

        private readonly GameObject _stickerIndicator;

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

                        Health = 50 * (gameLevel / 2);

                        SetHonk(gameLevel: gameLevel, honkTemplatesCount: honkTemplatesCount, willHonk: true);
                    }
                    break;
                default:
                    break;
            }

            Speed = speed;
            StreamingDirection = streamingDirection;

            _honkingIndicator.Height = 50 * scale;
            _honkingIndicator.Margin = new Thickness(0, 15 * scale, 0, 0);
            _honkingIndicator.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING).Value);

            _honkingBustedIndicator.Height = 50 * scale;
            _honkingBustedIndicator.Margin = new Thickness(0, 15 * scale, 0, 0);
            _honkingBustedIndicator.Source = new BitmapImage(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.HONKING_BUSTED).Value);

            _stickerIndicator = new Sticker(scale)
            {
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            _stickerIndicator.SetRotation(_random.Next(-30, 30));

            _content.Children.Add(_vehicle);
            _content.Children.Add(_honkingIndicator);
            _content.Children.Add(_honkingBustedIndicator);
            _content.Children.Add(_stickerIndicator);

            SetChild(_content);
        }

        #endregion

        #region Properties

        public int HonkSoundIndex { get; set; }

        public bool WillHonk { get; set; }

        public HonkState HonkState { get; set; }

        //public Sticker AttachedSticker { get; set; }

        public StreamingDirection StreamingDirection { get; set; } = StreamingDirection.UpWard;

        public VehicleClass VehicleClass { get; set; } = VehicleClass.DEFAULT_CLASS;

        public MovementIntent MovementIntent { get; set; } = MovementIntent.MOVE;

        public double Health { get; set; } = 100;

        public double HitPoints { get; set; } = 10;

        public bool IsRecoveringFromPlayerAttack { get; set; }

        #endregion

        #region Methods

        public new void SetContent(Uri uri)
        {
            _vehicle.Source = new BitmapImage(uri);
            _vehicle.Visibility = Visibility.Visible;
        }

        //public bool IsHonkBusted()
        //{
        //    return HonkState == HonkState.HONKING_BUSTED && AttachedSticker is not null;
        //}

        public bool CanBustHonk(Rect vehicleCloseHitBox, PlayerState playerState, Rect playerHitBox)
        {
            return HonkState == HonkState.HONKING && !IsRecoveringFromPlayerAttack && playerState == PlayerState.Attacking && playerHitBox.IntersectsWith(vehicleCloseHitBox);
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

        public bool BustHonk()
        {
            bool isHonkBusted = false;

            IsMarkedForPopping = true;
            HasPopped = false;

            switch (VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    {
                        UpdateHonkState(HonkState.HONKING_BUSTED);
                        isHonkBusted = true;
                    }
                    break;
                case VehicleClass.BOSS_CLASS:
                    {
                        Health -= HitPoints;
                        IsRecoveringFromPlayerAttack = true;

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

            return isHonkBusted;
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
                        _honkingIndicator.Opacity = 0;
                        _honkingBustedIndicator.Opacity = 0;
                        _stickerIndicator.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING:
                    {
                        _honkingIndicator.Opacity = 1;
                        _honkingBustedIndicator.Opacity = 0;
                        _stickerIndicator.Opacity = 0;
                    }
                    break;
                case HonkState.HONKING_BUSTED:
                    {
                        _honkingIndicator.Opacity = 0;
                        _honkingBustedIndicator.Opacity = 1;
                        _stickerIndicator.Opacity = 1;
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

    public enum MovementIntent
    {
        MOVE,
        IDLE,
    }

    public enum StreamingDirection
    {
        DownWard,
        UpWard,
    }

    public enum VehicleClass
    {
        DEFAULT_CLASS,
        BOSS_CLASS,
    }
}

