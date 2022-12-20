namespace HonkHeroGame
{
    public class Vehicle : GameObject
    {
        public Vehicle(double scale)
        {
            Tag = ElementType.VEHICLE;

            Height = Constants.VEHICLE_SIZE * scale;
            Width = Constants.VEHICLE_SIZE * scale;
        }
    }
}

