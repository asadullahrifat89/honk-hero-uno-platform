using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;

namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {
        #region Fields

        private int _honkCounter;
        private readonly int _honkCounterMin = 150;
        private readonly int _honkCounterMax = 250;
        private readonly Random _random = new Random();

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

        public bool WaitForHonk()
        {
            if (!IsBusted && WillHonk)
            {
                _honkCounter--;

                if (_honkCounter < 0)
                {
                    _honkCounter = SetHonkCounter();
                    IsHonking = true;

                    return true;
                }
            }

            return false;
        }

        public void BustHonking()
        {
            IsHonking = false;
            IsBusted = true;
        }

        public void ResetHonking()
        {
            IsHonking = false;
            IsBusted = false;

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

        #endregion
    }
}

