using System;

namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {

        #region Fields

        private int _honkCounter;
        private readonly int _honkCounterDefault = 300;
        private readonly Random _random = new Random();


        #endregion

        #region Ctor

        public Vehicle(double scale)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;

            HonkIndex = _random.Next(0, 3);

            _honkCounter = _random.Next(200, _honkCounterDefault);
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
                    _honkCounter = _random.Next(200, _honkCounterDefault);
                    IsHonking = true;

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}

