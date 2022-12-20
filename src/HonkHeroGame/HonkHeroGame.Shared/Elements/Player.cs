﻿using System.Linq;

namespace HonkHeroGame
{
    public class Player : GameObject
    {
        public Player(double scale)
        {
            Tag = ElementType.PLAYER;

            Width = Constants.PLAYER_WIDTH * scale;
            Height = Constants.PLAYER_HEIGHT * scale;

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
        }

        #region Properties

        public PlayerState PlayerState { get; set; } = PlayerState.Idle;

        #endregion

        #region Methods     

        public void SetState(PlayerState playerState)
        {
            PlayerState = playerState;

            switch (PlayerState)
            {
                case PlayerState.Idle:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER).Value);
                    break;
                case PlayerState.Flying:
                    SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.Key is ElementType.PLAYER_FLY).Value);
                    break;
                default:
                    break;
            }
        }

        public void SetFacingDirectionX(MovementDirectionX movementDirectionX, double scaleX = 1)
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

        #endregion    
    }

    public enum PlayerState
    {
        Idle,
        Jumping,
        Falling,
        Flying
    }

    public enum MovementDirectionY
    {
        Up,
        Down,
    }

    public enum MovementDirectionX
    {
        None,
        Left,
        Right,
    }
}
