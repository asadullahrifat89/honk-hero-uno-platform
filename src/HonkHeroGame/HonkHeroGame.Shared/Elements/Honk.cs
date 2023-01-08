namespace HonkHeroGame
{
    public class Honk : GameObject
    {
        public Honk(double scale, double speed, VehicleClass vehicleClass, StreamingDirection streamingDirection = StreamingDirection.UpWard)
        {
            Tag = ElementType.HONK;

            VehicleClass = vehicleClass;

            switch (VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    {
                        Height = Constants.HONK_SIZE * scale;
                        Width = Constants.HONK_SIZE * scale;
                    }
                    break;
                case VehicleClass.BOSS_CLASS:
                    {
                        Height = Constants.BOSS_HONK_SIZE * scale;
                        Width = Constants.BOSS_HONK_SIZE * scale;
                    }
                    break;
                default:
                    break;
            }

            Speed = speed;
            StreamingDirection = streamingDirection;
        }

        public StreamingDirection StreamingDirection { get; set; } = StreamingDirection.UpWard;

        public VehicleClass VehicleClass { get; set; }
    }
}

