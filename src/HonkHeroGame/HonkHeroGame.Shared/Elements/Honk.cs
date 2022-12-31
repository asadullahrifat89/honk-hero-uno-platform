namespace HonkHeroGame
{
    public class Honk : GameObject
    {
        public Honk(double scale, double speed, StreamingDirection streamingDirection = StreamingDirection.UpWard)
        {
            Tag = ElementType.HONK;

            Height = Constants.HONK_SIZE * scale;
            Width = Constants.HONK_SIZE * scale;

            Speed = speed;
            StreamingDirection = streamingDirection;
        }

        public StreamingDirection StreamingDirection { get; set; } = StreamingDirection.UpWard;
    }
}

