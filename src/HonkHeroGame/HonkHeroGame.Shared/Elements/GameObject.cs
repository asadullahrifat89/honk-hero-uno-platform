using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;

namespace HonkHeroGame
{
    public class GameObject : Border
    {
        #region Fields

        private readonly Image _content = new() { Stretch = Stretch.Uniform, Visibility = Visibility.Collapsed };

        private readonly CompositeTransform _compositeTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };

        private bool _popPlusCompleted;

        #endregion

        #region HitBox Debug

        //private Border _hitBoxborder;

        #endregion

        #region Properties

        public double Speed { get; set; } = 0;

        public bool HasFaded => Opacity <= 0;

        public bool IsMarkedForPopping { get; set; }

        public bool IsFlaggedForShrinking { get; set; }

        public bool HasShrinked => _compositeTransform.ScaleX <= 0;

        public bool HasPopped { get; set; }

        #endregion

        #region Ctor

        public GameObject()
        {
            RenderTransformOrigin = new Point(0.5, 0.5);

            RenderTransform = _compositeTransform;
            CanDrag = false;

            SetChild(_content);

            #region HitBox Debug

            //BorderThickness = new Thickness(1);
            //BorderBrush = new SolidColorBrush(Colors.Black);

            //_hitBoxborder = new Border()
            //{
            //    BorderThickness = new Thickness(1),
            //    BorderBrush = new SolidColorBrush(Colors.Black)
            //};

            //var grid = new Grid()
            //{
            //    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            //    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            //};

            //grid.Children.Add(_content);
            //grid.Children.Add(_hitBoxborder);

            //SetChild(grid);

            #endregion
        }

        #endregion

        #region Methods

        //public void SetHitBoxBorder(Rect rect)
        //{
        //    _hitBoxborder.Height = rect.Height;
        //    _hitBoxborder.Width = rect.Width;
        //}

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double GetTop()
        {
            return Canvas.GetTop(this);
        }

        public double GetLeft()
        {
            return Canvas.GetLeft(this);
        }

        public int GetZ()
        {
            return Canvas.GetZIndex(this);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(this, top);
        }

        public void SetLeft(double left)
        {
            Canvas.SetLeft(this, left);
        }

        public void SetZ(int z)
        {
            Canvas.SetZIndex(this, z);
        }

        public void SetPosition(double left, double top)
        {
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        public void SetChild(UIElement uIElement)
        {
            Child = uIElement;
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);
            _content.Visibility = Visibility.Visible;
        }

        public void SetScaleTransform(double scaleXY)
        {
            _compositeTransform.ScaleX = scaleXY;
            _compositeTransform.ScaleY = scaleXY;
        }

        public void SetScaleTransform(double scaleX, double scaleY)
        {
            _compositeTransform.ScaleX = scaleX;
            _compositeTransform.ScaleY = scaleY;
        }

        public double GetScaleX()
        {
            return _compositeTransform.ScaleX;
        }

        public double GetScaleY() 
        {
            return _compositeTransform.ScaleY;
        }

        public void SetScaleX(double scaleX)
        {
            _compositeTransform.ScaleX = scaleX;
        }

        public void SetScaleY(double scaleY)
        {
            _compositeTransform.ScaleY = scaleY;
        }

        public void SetRotation(double rotation)
        {
            _compositeTransform.Rotation = rotation;
        }

        public void SetSkewX(double skewX)
        {
            _compositeTransform.SkewX = skewX;
        }

        public void SetSkewY(double skewY)
        {
            _compositeTransform.SkewY = skewY;
        }

        public void Pop()
        {
            if (!HasPopped)
            {
                if (!_popPlusCompleted && _compositeTransform.ScaleX < 1.5)
                {
                    _compositeTransform.ScaleX += 0.1;
                    _compositeTransform.ScaleY += 0.1;
                }

                if (_compositeTransform.ScaleX >= 1.5)
                    _popPlusCompleted = true;

                if (_popPlusCompleted)
                {
                    _compositeTransform.ScaleX -= 0.1;
                    _compositeTransform.ScaleY -= 0.1;

                    if (_compositeTransform.ScaleX <= 1)
                    {
                        HasPopped = true;
                        _popPlusCompleted = false;
                    }
                }
            }
        }

        public void Fade()
        {
            Opacity -= 0.005;
        }

        public void Shrink()
        {
            _compositeTransform.ScaleX -= 0.1;
            _compositeTransform.ScaleY -= 0.1;
        }

        #endregion
    }

    public enum ElementType
    {
        NONE,
        PLAYER,
        PLAYER_FLYING,
        PLAYER_ATTACKING,
        PLAYER_APPRECIATING,
        VEHICLE,
        HEALTH,
        CLOUD,
        STICKER,
        POWERUP,
        HONK,
        COLLECTIBLE,
        ROAD_DECORATION,
        HONKING,
        HONKING_BUSTED,
    }
}

