using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace HonkHeroGame
{
    public sealed partial class GamePlayPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private double _gameSpeed = 1.8;
        private readonly double _gameSpeedDefault = 1.8;

        private int _markNum;

        private Uri[] _vehicles;
        private Uri[] _honks;
        private Uri[] _collectibles;
        private Uri[] _powerUps;

        private Player _player;
        private Rect _playerHitBox;
        private Rect _playerDistantHitBox;

        private double _playerHealth;
        private readonly double _playerHitPoints = 5;
        private readonly double _playerHealPoints = 7;

        private readonly double _playerPositionGrace = 7;
        private readonly double _playerAttackingScalePoint = 0.2;

        private double _playerLag;
        private readonly double _playerLagDefault = 35;

        private int _playerIdleDurationCounter;
        private readonly int _playerIdleDurationCounterDefault = 20;

        private int _playerAttackDurationCounter;
        private readonly int _playerAttackDurationCounterDefault = 15;

        private PowerUpType _powerUpType;

        //private int _powerUpCount;
        //private readonly int _powerUpSpawnLimit = 1;

        private int _powerUpSpawnCounter = 600;

        private int _powerModeDurationCounter;
        private readonly int _powerModeDurationDefault = 1000;

        private double _score;
        private double _scoreCap;
        private double _difficultyMultiplier;

        private bool _isGameOver;
        private bool _isPowerMode;

        private Point _pointerPosition;
        private Point _attackPosition;

        private int _collectibleCollected;
        private int _vehiclesTagged;

        private readonly int _numberOfLanes = 5;
        private readonly List<(double Start, double End)> _lanes = new();

        #endregion

        #region Ctor

        public GamePlayPage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePlayPage_Loaded;
            Unloaded += GamePlayPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetLocalization();

            SizeChanged += GamePlayPage_SizeChanged;
        }

        private void GamePlayPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePlayPage_SizeChanged;
            StopGame();
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        }

        #endregion

        #region Input

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isGameOver)
            {
                App.EnterFullScreen(true);

                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                if (QuitGameButton.IsChecked == false)
                {
                    PointerPoint point = e.GetCurrentPoint(GameView);
                    _attackPosition = point.Position;
                    _pointerPosition = point.Position;

                    PlayerAttack();
                }
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(GameView);
            _pointerPosition = point.Position;

            if (!_isGameOver && QuitGameButton.IsChecked == false)
            {
                if (_player.PlayerState == PlayerState.Idle)
                    _player.SetState(PlayerState.Flying);
            }
        }

        #endregion

        #region Button

        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameOver();
        }

        #endregion

        #endregion

        #region Methods

        #region Animation

        #region Game

        private void LoadGameElements()
        {
            _vehicles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.VEHICLE).Select(x => x.Value).ToArray();
            _honks = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.HONK).Select(x => x.Value).ToArray();
            _collectibles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
            _powerUps = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.POWERUP).Select(x => x.Value).ToArray();
        }

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateGameView();
        }

        private void PopulateGameView()
        {
            // add some vehicles
            for (double i = 0; i < 15 * _scale; i++)
                SpawnVehicle();

            // add some collectibles
            for (int i = 0; i < 5; i++)
                SpawnCollectible();
        }

        private void StartGame()
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            HideInGameTextMessage();
            ResetControls();

            _gameSpeed = _gameSpeedDefault * _scale;
            _playerLag = _playerLagDefault;

            _isGameOver = false;
            _isPowerMode = false;

            _powerModeDurationCounter = _powerModeDurationDefault;
            //_powerUpCount = 0;

            _score = 0;
            _scoreCap = 50;
            _difficultyMultiplier = 1;

            _collectibleCollected = 0;
            _vehiclesTagged = 0;

            ScoreText.Text = "0";

            _playerHealth = 100;

            PlayerHealthBarPanel.Visibility = Visibility.Visible;
            QuitGamePanel.Visibility = Visibility.Visible;

            RecycleGameObjects();
            RemoveGameObjects();
            StartGameSounds();

            SpawnPlayer();
            RunGame();

#if DEBUG
            Console.WriteLine("GAME STARTED");
            Console.WriteLine("PLAYER LAG: " + _playerLag);
            Console.WriteLine("GAME SPEED: " + _gameSpeed);
#endif
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
                GameViewLoop();
        }

        private void GameViewLoop()
        {
            ScoreText.Text = _score.ToString("#");
            PlayerHealthBar.Value = _playerHealth;

            _playerHitBox = _player.GetHitBox();
            _playerDistantHitBox = _player.GetDistantHitBox();

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            if (_isPowerMode)
            {
                PowerUpCoolDown();
                if (_powerModeDurationCounter <= 0)
                    PowerDown();
            }

#if DEBUG
            GameElementsCount.Text = GameView.Children.Count.ToString();
#endif
        }

        private void ResetControls()
        {
            _pointerPosition = null;
        }

        private void PauseGame()
        {
            InputView.Focus(FocusState.Programmatic);
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            PauseGameSounds();
        }

        private void ResumeGame()
        {
            InputView.Focus(FocusState.Programmatic);
            HideInGameTextMessage();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            SoundHelper.ResumeSound(SoundType.AMBIENCE);
            SoundHelper.ResumeSound(SoundType.SONG);

            RunGame();
        }

        private void StopGame()
        {
            _gameViewTimer?.Dispose();
            StopGameSounds();
        }

        private void GameOver()
        {
            _isGameOver = true;

            PlayerScoreHelper.PlayerScore = new HonkHeroGameScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _collectibleCollected,
                VehiclesTagged = _vehiclesTagged,
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);
            NavigateToPage(typeof(GameOverPage));
        }

        #endregion

        #region GameObject

        private void SpawnGameObjects()
        {
            if (!_isPowerMode)
            {
                _powerUpSpawnCounter--;

                if (_powerUpSpawnCounter < 1)
                {
                    SpawnPowerUp();
                    _powerUpSpawnCounter = _random.Next(800, 1000);
                }
            }
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.VEHICLE:
                        UpdateVehicle(x as Vehicle);
                        break;
                    case ElementType.PLAYER:
                        UpdatePlayer();
                        break;
                    case ElementType.HONK:
                        UpdateHonk(x);
                        break;
                    case ElementType.COLLECTIBLE:
                        UpdateCollectible(x);
                        break;
                    case ElementType.POWERUP:
                        UpdatePowerUp(x);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        private void RecycleGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.VEHICLE:
                        RecyleVehicle(x as Vehicle);
                        break;
                    case ElementType.COLLECTIBLE:
                        RecyleCollectible(x);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            _player = new Player(_scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height - (50 * _scale));

            _player.SetZ(_lanes.Count + 3);

            GameView.Children.Add(_player);
        }

        private void UpdatePlayer()
        {
            switch (_player.PlayerState)
            {
                case PlayerState.Idle:
                    {
                        PlayerIdle();
                    }
                    break;
                case PlayerState.Flying:
                    {
                        PlayerFlying();
                    }
                    break;
                case PlayerState.Attacking:
                    {
                        PlayerAttacking();
                    }
                    break;
                default:
                    break;
            }
        }

        private void PlayerIdle()
        {
            _playerIdleDurationCounter--;

            switch (_player.IdlingDirectionY)
            {
                case IdlingDirectionY.Up:
                    _player.SetTop(_player.GetTop() - 1);
                    break;
                case IdlingDirectionY.Down:
                    _player.SetTop(_player.GetTop() + 1);
                    break;
                default:
                    break;
            }

            if (_playerIdleDurationCounter <= 0)
            {
                _playerIdleDurationCounter = _playerIdleDurationCounterDefault;
                _player.IdlingDirectionY = _player.IdlingDirectionY == IdlingDirectionY.Down ? IdlingDirectionY.Up : IdlingDirectionY.Down;
            }
        }

        private void PlayerFlying()
        {
            if (!MovePlayer(_pointerPosition))
            {
                _playerIdleDurationCounter = _playerIdleDurationCounterDefault;
                _player.SetState(PlayerState.Idle);
                _player.SetScaleTransform(1);
            }
        }

        private void PlayerAttacking()
        {
            _playerAttackDurationCounter--;

            if (_playerAttackDurationCounter > _playerAttackDurationCounterDefault / 2)
            {
                //TODO: increase scale

                //if (_player.GetScaleY() <= 2)
                //{
                //    if (_player.FacingDirectionX == MovementDirectionX.Right)
                //        _player.SetScaleTransform(
                //            scaleX: _player.GetScaleX() + _playerAttackingScalePoint,
                //            scaleY: _player.GetScaleY() + _playerAttackingScalePoint);
                //    else
                //        _player.SetScaleTransform(
                //            scaleX: (_player.GetScaleX() - _playerAttackingScalePoint),
                //            scaleY: _player.GetScaleY() + _playerAttackingScalePoint);
                //}

                if (_player.GetScaleY() <= 2)
                    _player.SetScaleTransform(
                        scaleX: _player.GetScaleX() + _playerAttackingScalePoint,
                        scaleY: _player.GetScaleY() + _playerAttackingScalePoint);
            }
            else
            {
                if (_player.GetScaleY() > 1.0)
                {
                    //TODO: decrease scale

                    //if (_player.FacingDirectionX == MovementDirectionX.Right)
                    //    _player.SetScaleTransform(
                    //        scaleX: _player.GetScaleX() - _playerAttackingScalePoint,
                    //        scaleY: _player.GetScaleY() - _playerAttackingScalePoint);
                    //else
                    //    _player.SetScaleTransform(
                    //        scaleX: _player.GetScaleX() + _playerAttackingScalePoint,
                    //        scaleY: _player.GetScaleY() - _playerAttackingScalePoint);

                    _player.SetScaleTransform(
                        scaleX: _player.GetScaleX() - _playerAttackingScalePoint,
                        scaleY: _player.GetScaleY() - _playerAttackingScalePoint);
                }
            }

            MovePlayer(point: _attackPosition, isAttacking: true);

            if (_playerAttackDurationCounter <= 0)
            {
                _player.SetState(PlayerState.Flying);
                _player.SetScaleTransform(1);
            }
        }

        private bool MovePlayer(Point point, bool isAttacking = false)
        {
            bool hasMoved = false;

            double left = _player.GetLeft();
            double top = _player.GetTop();

            double playerMiddleX = left + _player.Width / 2;
            double playerMiddleY = top + _player.Height / 2;

            // move up
            if (point.Y < playerMiddleY - _playerPositionGrace)
            {
                var distance = Math.Abs(point.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance, isAttacking);

                _player.SetTop(top - speed);

                hasMoved = true;
            }

            // move left
            if (point.X < playerMiddleX - _playerPositionGrace)
            {
                var distance = Math.Abs(point.X - playerMiddleX);
                double speed = GetFlightSpeed(distance, isAttacking);

                _player.SetLeft(left - speed);
                _player.SetFacingDirectionX(MovementDirectionX.Left);

                hasMoved = true;
            }

            // move down
            if (point.Y > playerMiddleY + _playerPositionGrace)
            {
                var distance = Math.Abs(point.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance, isAttacking);

                _player.SetTop(top + speed);

                hasMoved = true;
            }

            // move right
            if (point.X > playerMiddleX + _playerPositionGrace)
            {
                var distance = Math.Abs(point.X - playerMiddleX);
                double speed = GetFlightSpeed(distance, isAttacking);

                _player.SetLeft(left + speed);
                _player.SetFacingDirectionX(MovementDirectionX.Right);

                hasMoved = true;
            }

            return hasMoved;
        }

        private double GetFlightSpeed(double distance, bool isAttacking = false)
        {
            return (distance / _playerLag) * (isAttacking ? _gameSpeedDefault * 2 : 1);
        }

        private void PlayerAttack()
        {
            _playerAttackDurationCounter = _playerAttackDurationCounterDefault;
            _player.SetState(PlayerState.Attacking);
        }

        #endregion

        #region Honk

        private void SpawnHonk(Vehicle vehicle)
        {
            Honk honk = new(_scale, vehicle.Speed / 2);

            _markNum = _random.Next(0, _honks.Length);
            honk.SetContent(_honks[_markNum]);

            honk.SetLeft(vehicle.GetLeft() - 10 * _scale);
            honk.SetTop(vehicle.GetTop() + vehicle.Height / 3);

            honk.SetRotation(_random.Next(-30, 45));
            honk.SetZ(vehicle.GetZ() + 1);

            GameView.Children.Add(honk);

            SoundHelper.PlaySound(SoundType.HONK, vehicle.HonkIndex);

            LooseHealth();
        }

        private void UpdateHonk(GameObject honk)
        {
            honk.SetLeft(honk.GetLeft() - honk.Speed * 2.5);
            honk.SetTop(honk.GetTop() - honk.Speed);
            honk.Fade();

            if (honk.HasFaded)
                GameView.AddDestroyableGameObject(honk);
        }

        private bool WaitForHonk(Vehicle vehicle)
        {
            if (vehicle.GetLeft() > 0 && vehicle.GetLeft() + vehicle.Width / 3 < _windowWidth
                && vehicle.GetTop() > 0 && vehicle.GetTop() + vehicle.Height / 3 < _windowHeight)
            {
                return vehicle.WaitForHonk();
            }

            return false;
        }

        private void BustHonk(Vehicle vehicle)
        {
            Sticker sticker = SpawnSticker(vehicle);
            vehicle.BustHonking();
            vehicle.AttachSticker(sticker);

            AddScore(5);
            AddHealth();

            _vehiclesTagged++;

            SoundHelper.PlayRandomSound(SoundType.HONK_BUST);
        }

        #endregion

        #region Sticker

        private Sticker SpawnSticker(Vehicle vehicle)
        {
            Sticker sticker = new(_scale);

            sticker.SetLeft(vehicle.GetLeft() + vehicle.Width / 1.5);
            sticker.SetTop(vehicle.GetTop() + vehicle.Height / 1.5);

            sticker.SetZ(vehicle.GetZ() + 1);
            sticker.SetRotation(_random.Next(-30, 45));

            GameView.Children.Add(sticker);

            return sticker;
        }

        private void UpdateSticker(Vehicle vehicle)
        {
            var sticker = vehicle.AttachedSticker;

            sticker.SetLeft(vehicle.GetLeft() + vehicle.Width / 1.5);
            sticker.SetTop(vehicle.GetTop() + vehicle.Height / 1.5);

            if (sticker.GetTop() + sticker.Height < 0 || sticker.GetLeft() + sticker.Width < 0)
                GameView.AddDestroyableGameObject(sticker);
        }

        #endregion

        #region Vehicle

        private void SpawnVehicle()
        {
            Vehicle vehicle = new(_scale, _gameSpeed + _random.Next(-1, 2));
            GameView.Children.Add(vehicle);
        }

        private void UpdateVehicle(Vehicle vehicle)
        {
            if (vehicle.IsMarkedForPopping && !vehicle.HasPopped)
                vehicle.Pop();

            if (vehicle.GetLeft() < _windowWidth)
                vehicle.SetTop(vehicle.GetTop() - vehicle.Speed * 0.5);

            vehicle.SetLeft(vehicle.GetLeft() - vehicle.Speed);

            if (vehicle.GetTop() + vehicle.Height < 0 || vehicle.GetLeft() + vehicle.Width < 0)
            {
                RecyleVehicle(vehicle);
            }
            else
            {
                if (vehicle.IsBusted && vehicle.AttachedSticker is not null)
                    UpdateSticker(vehicle);

                // if player hits the vehicle, bust honking and attach sticker
                if (vehicle.IsHonking && _player.PlayerState == PlayerState.Attacking && _playerHitBox.IntersectsWith(vehicle.GetCloseHitBox(_scale)))
                    BustHonk(vehicle);

                if (WaitForHonk(vehicle))
                    SpawnHonk(vehicle);

                // if vechicle will collide with another vehicle, slower vehicles will slow down faster vehicles
                if (GameView.Children.OfType<Vehicle>()
                    .LastOrDefault(v => v.GetCollisionPreventionHitBox(_scale)
                    .IntersectsWith(vehicle.GetCollisionPreventionHitBox(_scale))) is Vehicle collidingVehicle && collidingVehicle.Speed != vehicle.Speed)
                {
                    if (collidingVehicle.Speed > vehicle.Speed)
                        collidingVehicle.Speed = vehicle.Speed;
                    else
                        vehicle.Speed = collidingVehicle.Speed;
                }
            }
        }

        private void RecyleVehicle(Vehicle vehicle)
        {
            _markNum = _random.Next(0, _vehicles.Length);

            vehicle.SetContent(_vehicles[_markNum]);
            vehicle.Speed = _gameSpeed + _random.Next(0, 3);

            vehicle.ResetHonking();
            RandomizeVehiclePosition(vehicle);
        }

        private void RandomizeVehiclePosition(GameObject vehicle)
        {
            var laneNumber = _random.Next(0, _lanes.Count);
            var (Start, End) = _lanes[laneNumber];

            var left = _random.Next(minValue: (int)GameView.Width, maxValue: (int)GameView.Width * 2);
            var top = laneNumber == 0 ? End : laneNumber + 1 == _lanes.Count ? Start : _random.Next((int)Start, (int)End);

            vehicle.SetPosition(
                left: left,
                top: top);

            vehicle.SetZ(laneNumber + 1);

#if DEBUG
            Console.WriteLine("VEHICLE SPAWNED ON LANE: " + laneNumber + " X: " + left + " Y: " + top);
#endif
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);
            collectible.SetRotation(_random.Next(-30, 45));
            collectible.SetZ(_lanes.Count + 1);

            RandomizeCollectiblePosition(collectible);

            GameView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject collectible)
        {
            collectible.SetTop(collectible.GetTop() + _gameSpeed);

            if (collectible.GetTop() > GameView.Height)
            {
                RecyleCollectible(collectible);
            }
            else
            {
                // only consider player intersection after appearing in viewport
                if (collectible.GetTop() + collectible.Height > 10)
                {
                    if (collectible.IsFlaggedForShrinking)
                    {
                        collectible.Shrink();

                        if (collectible.HasShrinked)
                            RecyleCollectible(collectible);
                    }
                    else
                    {
                        if (_playerHitBox.IntersectsWith(collectible.GetHitBox()))
                        {
                            collectible.IsFlaggedForShrinking = true;
                            Collectible();
                        }

                        MagnetPull(collectible);
                    }
                }
            }
        }

        private void RecyleCollectible(GameObject collectible)
        {
            RandomizeCollectiblePosition(collectible);
            collectible.SetContent(_collectibles[0]);
            collectible.SetScaleTransform(1);
            collectible.IsFlaggedForShrinking = false;
        }

        private void RandomizeCollectiblePosition(GameObject collectible)
        {
            collectible.SetPosition(
                left: _random.Next(50, (int)GameView.Width - 50),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void Collectible()
        {
            SoundHelper.PlayRandomSound(SoundType.COLLECTIBLE);
            _collectibleCollected++;
            AddScore(1);
        }

        #endregion

        #region Health

        private void AddHealth()
        {
            if (_playerHealth < 100)
            {
                SoundHelper.VolumeUp(soundType: SoundType.SONG, level: 0.2);

                var health = _playerHealPoints;

                if (_playerHealth + _playerHealPoints > 100)
                    health = _playerHealth + _playerHealPoints - 100;

                _playerHealth += health;
            }
        }

        private void LooseHealth()
        {
            SoundHelper.VolumeDown(SoundType.SONG);

            _playerHealth -= _playerHitPoints;

            if (_playerHealth <= 0)
                GameOver();
        }

        #endregion

        #region PowerUp

        private void SpawnPowerUp()
        {
            PowerUp powerUp = new(_scale);

            var powerUpTypes = Enum.GetNames<PowerUpType>();
            powerUp.PowerUpType = (PowerUpType)_random.Next(0, powerUpTypes.Length);

            powerUp.SetContent(_powerUps[(int)powerUp.PowerUpType]);
            powerUp.SetZ(_player.GetZ() + 1);

            RandomizePowerUpPosition(powerUp);
            GameView.Children.Add(powerUp);
#if DEBUG
            Console.WriteLine("POWER UP SPAWNED: " + powerUp.PowerUpType);
#endif
        }

        private void RandomizePowerUpPosition(GameObject powerUp)
        {
            powerUp.SetPosition(
                left: _random.Next(0, (int)GameView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void UpdatePowerUp(GameObject powerUp)
        {
            powerUp.SetTop(powerUp.GetTop() + _gameSpeed);

            if (_playerHitBox.IntersectsWith(powerUp.GetHitBox()))
            {
                GameView.AddDestroyableGameObject(powerUp);
                PowerUp(powerUp as PowerUp);
            }
            else
            {
                if (powerUp.GetTop() > GameView.Height)
                    GameView.AddDestroyableGameObject(powerUp);
            }
        }

        private void PowerUp(PowerUp powerUp)
        {
            _isPowerMode = true;

            _powerModeDurationCounter = _powerModeDurationDefault;
            _powerUpType = powerUp.PowerUpType;

            powerUpText.Visibility = Visibility.Visible;
            SoundHelper.PlaySound(SoundType.POWER_UP);
        }

        private void PowerUpCoolDown()
        {
            _powerModeDurationCounter -= 1;
            double remainingPow = (double)_powerModeDurationCounter / (double)_powerModeDurationDefault * 4;

            powerUpText.Text = "";

            for (int i = 0; i < remainingPow; i++)
            {
                powerUpText.Text += "⚡";
            }
        }

        private void PowerDown()
        {
            _isPowerMode = false;

            powerUpText.Visibility = Visibility.Collapsed;
            SoundHelper.PlaySound(SoundType.POWER_DOWN);
        }

        private void MagnetPull(GameObject collectible)
        {
            // if magnet power up received then pull collectibles to player
            if (_isPowerMode && _powerUpType == PowerUpType.MagnetPull)
            {
                var collectibleHitBoxDistant = collectible.GetDistantHitBox();

                if (_playerDistantHitBox.IntersectsWith(collectibleHitBoxDistant))
                {
                    var collectibleHitBox = collectible.GetHitBox();

                    if (_playerHitBox.Left < collectibleHitBox.Left)
                        collectible.SetLeft(collectible.GetLeft() - _gameSpeed * 3.5);

                    if (collectibleHitBox.Right < _playerHitBox.Left)
                        collectible.SetLeft(collectible.GetLeft() + _gameSpeed * 3.5);

                    if (collectibleHitBox.Top > _playerHitBox.Bottom)
                        collectible.SetTop(collectible.GetTop() - _gameSpeed * 3.5);

                    if (collectibleHitBox.Bottom < _playerHitBox.Top)
                        collectible.SetTop(collectible.GetTop() + _gameSpeed * 3.5);
                }
            }
        }

        private double TwoxScore(double score)
        {
            if (_isPowerMode && _powerUpType == PowerUpType.TwoxScore)
                score *= 2;

            return score;
        }

        #endregion

        #endregion

        #region Score

        private void AddScore(double score)
        {
            score = TwoxScore(score);

            _score += score;
            ScaleDifficulty();
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_score > _scoreCap)
            {
                _gameSpeed = (_gameSpeedDefault * _scale) + 0.2 * _difficultyMultiplier;

                if (_playerLag > 15)
                    _playerLag = _playerLagDefault - (_difficultyMultiplier * 1.5);

                _scoreCap += 50;
                _difficultyMultiplier++;

#if DEBUG
                Console.WriteLine("PLAYER LAG: " + _playerLag);
                Console.WriteLine("GAME SPEED: " + _gameSpeed);
#endif
            }
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.AMBIENCE);
            SoundHelper.PlaySound(SoundType.AMBIENCE);

            SoundHelper.RandomizeSound(SoundType.SONG);
            SoundHelper.PlaySound(SoundType.SONG);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.AMBIENCE);
            SoundHelper.StopSound(SoundType.SONG);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.AMBIENCE);
            SoundHelper.PauseSound(SoundType.SONG);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            GameView.Width = _windowWidth;
            GameView.Height = _windowHeight;

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;

            RoadMarkLeft2.Width = 15 * _scale;
            RoadMarkLeft2.Height = _windowHeight;
            RoadMarkLeft2.SetLeft(5);
            RoadMarkLeft2.SetSkewY(43);
            RoadMarkLeft2.SetRotation(-63.5);

            RoadMarkLeft.Width = 15 * _scale;
            RoadMarkLeft.Height = _windowHeight;
            RoadMarkLeft.SetLeft((_windowWidth / 4 - (RoadMarkLeft.Width / 2)));
            RoadMarkLeft.SetSkewY(43);
            RoadMarkLeft.SetRotation(-63.5);

            //HighWayDivider.Width = 15 * _scale;
            //HighWayDivider.Height = _windowHeight;
            //HighWayDivider.SetLeft(_windowWidth / 2 - (HighWayDivider.Width / 2));
            //HighWayDivider.SetSkewY(43);
            //HighWayDivider.SetRotation(-63.5);

            RoadMarkRight.Width = 15 * _scale;
            RoadMarkRight.Height = _windowHeight;
            RoadMarkRight.SetLeft((_windowWidth / 4 - (RoadMarkRight.Width / 2)) * 3);
            RoadMarkRight.SetSkewY(43);
            RoadMarkRight.SetRotation(-63.5);

            RoadMarkRight2.Width = 15 * _scale;
            RoadMarkRight2.Height = _windowHeight;
            RoadMarkRight2.SetLeft((_windowWidth / 4 - (RoadMarkRight2.Width / 2)) * 4);
            RoadMarkRight2.SetSkewY(43);
            RoadMarkRight2.SetRotation(-63.5);

            RoadSideLeftImage.Width = _windowWidth;
            RoadSideLeftImage.Height = _windowHeight;

            RoadSideCenterImage.Width = _windowWidth;
            RoadSideCenterImage.Height = _windowHeight;

            RoadSideRightImage.Width = _windowWidth;
            RoadSideRightImage.Height = _windowHeight;

            _player?.SetSize(
                    width: Constants.PLAYER_WIDTH * _scale,
                    height: Constants.PLAYER_HEIGHT * _scale);


            _lanes.Clear();
            double laneHeight = _windowHeight / _numberOfLanes;

            for (int i = 0; i < _numberOfLanes + 1; i++)
            {
                _lanes.Add((laneHeight * i, laneHeight * (i + 1)));
            }

#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");

            Console.WriteLine($"TOTAL LANE COUNT: {_lanes.Count}");

            var lanesDetails = string.Join(" | ", _lanes.Select(x => $"{x.Start} <-> {x.End}").ToArray());
            Console.WriteLine($"TOTAL LANE POINTS: {lanesDetails}");
#endif           
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region InGameMessage

        private void ShowInGameTextMessage(string resourceKey)
        {
            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
            InGameMessagePanel.Visibility = Visibility.Visible;
        }

        private void HideInGameTextMessage()
        {
            InGameMessageText.Text = "";
            InGameMessagePanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
