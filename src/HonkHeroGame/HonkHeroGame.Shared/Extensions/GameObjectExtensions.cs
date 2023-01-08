using System.ComponentModel.DataAnnotations;
using Windows.Foundation;

namespace HonkHeroGame
{
    public static class GameObjectExtensions
    {
        #region Methods      

        /// <summary>
        /// Checks if a two rects intersect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IntersectsWith(this Rect source, Rect target)
        {
            var targetX = target.X;
            var targetY = target.Y;
            var sourceX = source.X;
            var sourceY = source.Y;

            var sourceWidth = source.Width;
            var sourceHeight = source.Height;

            var targetWidth = target.Width;
            var targetHeight = target.Height;

            if (source.Width >= 0.0 && target.Width >= 0.0
                && targetX <= sourceX + sourceWidth && targetX + targetWidth >= sourceX
                && targetY <= sourceY + sourceHeight)
            {
                return targetY + targetHeight >= sourceY;
            }

            return false;
        }

        public static Rect GetHitBox(this GameObject gameObject)
        {
            var rect = new Rect(
              x: gameObject.GetLeft(),
              y: gameObject.GetTop(),
              width: gameObject.Width,
              height: gameObject.Height);

            //gameObject.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetCloseHitBox(this GameObject gameObject)
        {
            var fourthWidht = gameObject.Width / 4;
            var fourthHeight = gameObject.Height / 4;

            var rect = new Rect(
                x: gameObject.GetLeft() + fourthWidht,
                y: gameObject.GetTop() + fourthHeight,
                width: gameObject.Width - fourthWidht,
                height: gameObject.Height - fourthHeight);

            //gameObject.SetHitBoxBorder(rect);

            return rect;
        }

        public static Rect GetDistantHitBox(this GameObject gameObject)
        {
            var maxWidth = (gameObject.Width * 4);
            var maxHeight = (gameObject.Height * 4);

            return new Rect(
                x: gameObject.GetLeft() - maxWidth,
                y: gameObject.GetTop() - maxHeight,
                width: gameObject.Width + maxWidth,
                height: gameObject.Height + maxHeight);
        }

        #endregion
    }
}
