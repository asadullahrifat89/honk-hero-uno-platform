using System;

namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {

        #region Fields

        private int _honkCounter;
        private readonly int _honkCounterDefault = 350;
        private readonly Random _random = new Random();


        #endregion

        #region Ctor

        public Vehicle(double scale)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;
            SetHonkIndex();

            _honkCounter = SetHonkCounter();
        }

        #endregion

        #region Properties

        public int HonkIndex { get; set; }

        public bool IsHonking { get; set; }

        public bool IsBusted { get; set; }

        #endregion

        #region Methods

        public bool ShouldHonk()
        {
            if (!IsBusted)
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
            HonkIndex = _random.Next(0, 3);

            _honkCounter = SetHonkCounter();
            SetHonkIndex();
        }

        private int SetHonkCounter()
        {
            return _random.Next(300, _honkCounterDefault);
        }

        private void SetHonkIndex()
        {
            HonkIndex = _random.Next(0, 3);
        }

        #endregion
    }
}

