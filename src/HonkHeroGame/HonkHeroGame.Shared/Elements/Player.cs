using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace HonkHeroGame
{
    public class Player : GameObject
    {
        #region Fields

        private readonly Grid _content = new();
        private readonly Image _idle = new() { Opacity = 1 };
        private readonly Image _flying = new() { Opacity = 0 };
        private readonly Image _attacking = new() { Opacity = 0 };

        #endregion

        public Player(double scale)
        {
            Tag = ElementType.PLAYER;

            Width = Constants.PLAYER_WIDTH * scale;
            Height = Constants.PLAYER_HEIGHT * scale;

            _idle.Source = Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value;
            _flying.Source = Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FLYING).Value;
            _attacking.Source = Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_ATTACKING).Value;

            _content.Children.Add(_idle);
            _content.Children.Add(_flying);
            _content.Children.Add(_attacking);

            SetChild(_content);

            //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        #region Properties

        public PlayerState PlayerState { get; set; } = PlayerState.Idle;

        public IdlingDirectionY IdlingDirectionY { get; set; } = IdlingDirectionY.Down;

        public MovementDirectionX FacingDirectionX { get; set; } = MovementDirectionX.None;

        #endregion

        #region Methods     

        public void SetState(PlayerState playerState)
        {
            PlayerState = playerState;

            switch (PlayerState)
            {
                case PlayerState.Idle:
                    {
                        _idle.Opacity = 1;
                        _flying.Opacity = 0;
                        _attacking.Opacity = 0;
                        //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
                    }
                    break;
                case PlayerState.Flying:
                    {
                        _idle.Opacity = 0;
                        _flying.Opacity = 1;
                        _attacking.Opacity = 0;
                        //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FLYING).Value);
                    }
                    break;
                case PlayerState.Attacking:
                    {
                        _idle.Opacity = 0;
                        _flying.Opacity = 0;
                        _attacking.Opacity = 1;
                        //SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_ATTACKING).Value);
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetFacingDirectionX(MovementDirectionX movementDirectionX, double scaleX = 1)
        {
            FacingDirectionX = movementDirectionX;

            switch (PlayerState)
            {
                case PlayerState.Flying:
                    {
                        switch (movementDirectionX)
                        {
                            case MovementDirectionX.Left:
                                SetScaleX(scaleX * -1);
                                break;
                            case MovementDirectionX.Right:
                                SetScaleX(scaleX);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case PlayerState.Idle:
                case PlayerState.Attacking:
                    break;
                default:
                    break;
            }
        }

        #endregion    
    }

    public enum PlayerState
    {
        Idle,
        Flying,
        Attacking,
    }

    public enum IdlingDirectionY
    {
        Up,
        Down,
    }

    //public enum MovementDirectionY
    //{
    //    Up,
    //    Down,
    //}

    public enum MovementDirectionX
    {
        None,
        Left,
        Right,
    }
}
