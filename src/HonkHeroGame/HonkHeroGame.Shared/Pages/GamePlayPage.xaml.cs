using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
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

        private double _gameSpeed;
        private readonly double _gameSpeedDefault = 1.5;

        private int _markNum;

        private Uri[] _vehicles_Up;
        private Uri[] _vehicles_Down;
        private Uri[] _vehicles_Boss;
        private Uri[] _honks;
        private Uri[] _collectibles;
        private Uri[] _powerUps;

        private Player _player;
        private Rect _playerHitBox;
        private Rect _playerDistantHitBox;

        private double _playerHealth;
        private readonly double _playerHitPoints = 3;
        private readonly double _playerHealPoints = 5;

        private readonly double _playerPositionGrace = 7;
        private readonly double _playerAttackingScalePoint = 0.2;

        private double _playerLag;
        private readonly double _playerLagDefault = 35;

        private int _playerIdleDurationCounter;
        private readonly int _playerIdleDurationCounterDefault = 20;

        private int _playerAttackDurationCounter;
        private readonly int _playerAttackDurationCounterDefault = 15;

        private PowerUpType _powerUpType;

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
        private int _stickersAmount;
        private int _vehiclesTagged;

        private int _gameLevel = 1;

        private int _honkTemplatesCount = 0;
        private int _bossHonkTemplatesCount = 0;

        private int _inGameMessageCoolDownCounter = 0;
        private readonly int _inGameMessageCoolDownCounterDefault = 125;
        private readonly int _slowMotionFactor = 10;

        #endregion

        #region Properties

        public bool InGameMessageSlowMotionInEffect { get; set; }

        #endregion

        #region Ctor

        public GamePlayPage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage(GetLocalizedResource("TAP_ON_SCREEN_TO_BEGIN"));

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

                    _pointerPosition = point.Position;
                    _attackPosition = point.Position;

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
            _vehicles_Up = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.VEHICLE_UPWARD).Select(x => x.Value).ToArray();
            _vehicles_Down = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.VEHICLE_DOWNWARD).Select(x => x.Value).ToArray();
            _vehicles_Boss = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.VEHICLE_BOSS).Select(x => x.Value).ToArray();

            _honks = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.HONK).Select(x => x.Value).ToArray();
            _collectibles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
            _powerUps = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.POWERUP).Select(x => x.Value).ToArray();

            _honkTemplatesCount = Constants.SOUND_TEMPLATES.Where(x => x.Key == SoundType.HONK).Count();
            _bossHonkTemplatesCount = Constants.SOUND_TEMPLATES.Where(x => x.Key == SoundType.BOSS_HONK).Count();
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
            SpawnVehicles();
            SpawnCollectibles();
        }

        private void StartGame()
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            HideInGameTextMessage();
            ResetControls();

            InGameMessageSlowMotionInEffect = false;

            _gameSpeed = _gameSpeedDefault * _scale;
            _playerLag = _playerLagDefault;

            _gameLevel = 1;
            SetGameLevelText();

            _isGameOver = false;
            _isPowerMode = false;

            _powerModeDurationCounter = _powerModeDurationDefault;

            _score = 0;
            _scoreCap = 50;
            _difficultyMultiplier = 1;
            SetScoreText();

            _collectibleCollected = 0;
            _stickersAmount = 10;
            SetStickersAmountText();

            _vehiclesTagged = 0;

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
            PlayerHealthBar.Value = _playerHealth;

            _playerHitBox = _player.GetHitBox();
            _playerDistantHitBox = _player.GetDistantHitBox();

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            if (_inGameMessageCoolDownCounter > 0)
                CoolDownInGameTextMessage();

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
            ShowInGameTextMessage(GetLocalizedResource("GAME_PAUSED"));

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
                        UpdateHonk(x as Honk);
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

            _player.SetZ(8);

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
            _playerIdleDurationCounter = InGameMessageSlowMotionInEffect ? _playerIdleDurationCounter - (1 / _slowMotionFactor) : _playerIdleDurationCounter - 1;

            var speed = (InGameMessageSlowMotionInEffect ? 1 / _slowMotionFactor : 1);

            switch (_player.IdlingDirectionY)
            {
                case IdlingDirectionY.Up:
                    _player.SetTop(_player.GetTop() - speed);
                    break;
                case IdlingDirectionY.Down:
                    _player.SetTop(_player.GetTop() + speed);
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
            var scalePoint = InGameMessageSlowMotionInEffect ? _playerAttackingScalePoint / _slowMotionFactor : _playerAttackingScalePoint;

            _playerAttackDurationCounter = InGameMessageSlowMotionInEffect ? _playerAttackDurationCounter - (1 / _slowMotionFactor) : _playerAttackDurationCounter - 1;

            if (_playerAttackDurationCounter > _playerAttackDurationCounterDefault / 2)
            {
                // increase scale
                if (_player.GetScaleY() <= 2)
                {
                    _player.SetScaleTransform(
                        scaleX: _player.GetScaleX() + scalePoint,
                        scaleY: _player.GetScaleY() + scalePoint);
                }
            }
            else
            {
                // decrease scale
                if (_player.GetScaleY() > 1.0)
                {
                    _player.SetScaleTransform(
                        scaleX: _player.GetScaleX() - scalePoint,
                        scaleY: _player.GetScaleY() - scalePoint);
                }
            }

            MovePlayer(point: _attackPosition);

            if (_playerAttackDurationCounter <= 0)
            {
                _player.SetState(PlayerState.Flying);
                _player.SetScaleTransform(1);
            }
        }

        private bool MovePlayer(Point point)
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
                double speed = GetFlightSpeed(distance);

                _player.SetTop(top - speed);

                hasMoved = true;
            }

            // move left
            if (point.X < playerMiddleX - _playerPositionGrace)
            {
                var distance = Math.Abs(point.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                _player.SetLeft(left - speed);
                _player.SetFacingDirectionX(MovementDirectionX.Left);

                hasMoved = true;
            }

            // move down
            if (point.Y > playerMiddleY + _playerPositionGrace)
            {
                var distance = Math.Abs(point.Y - playerMiddleY);
                double speed = GetFlightSpeed(distance);

                _player.SetTop(top + speed);

                hasMoved = true;
            }

            // move right
            if (point.X > playerMiddleX + _playerPositionGrace)
            {
                var distance = Math.Abs(point.X - playerMiddleX);
                double speed = GetFlightSpeed(distance);

                _player.SetLeft(left + speed);
                _player.SetFacingDirectionX(MovementDirectionX.Right);

                hasMoved = true;
            }

            return hasMoved;
        }

        private double GetFlightSpeed(double distance)
        {
            var speedBoost = _player.PlayerState == PlayerState.Attacking
                ? _gameSpeedDefault * 2
                : 1;

            var flightSpeed = (distance / _playerLag) * speedBoost;

            return InGameMessageSlowMotionInEffect ? flightSpeed / _slowMotionFactor : flightSpeed;
        }

        private void PlayerAttack()
        {
            _player.SetScaleTransform(1);
            _playerAttackDurationCounter = _playerAttackDurationCounterDefault;
            _player.SetState(PlayerState.Attacking);
        }

        #endregion

        #region Vehicle

        private void SpawnVehicles()
        {
            for (double i = 0; i < 8 * _scale; i++)
                SpawnVehicle(StreamingDirection.UpWard);

            for (double i = 0; i < 8 * _scale; i++)
                SpawnVehicle(StreamingDirection.DownWard);
        }

        private Vehicle SpawnVehicle(StreamingDirection streamingDirection, VehicleClass vehicleClass = VehicleClass.DEFAULT_CLASS)
        {
            var speed = RandomizeVehicleSpeed();

            Vehicle vehicle = new(
                scale: _scale,
                speed: speed,
                gameLevel: _gameLevel,
                honkTemplatesCount: _honkTemplatesCount,
                streamingDirection: streamingDirection,
                vehicleClass: vehicleClass);

            GameView.Children.Add(vehicle);

            return vehicle;
        }

        private void UpdateVehicle(Vehicle vehicle)
        {
            if (vehicle.IsMarkedForPopping && !vehicle.HasPopped)
                vehicle.Pop();

            var vehicleCloseHitBox = vehicle.GetCloseHitBox(_scale);

            if (IsHonkBusted(vehicle))
                UpdateSticker(vehicle);

            if (CanBustHonk(vehicle: vehicle, vehicleCloseHitBox: vehicleCloseHitBox))
                BustHonk(vehicle);

            if (WaitForHonk(vehicle))
                SpawnHonk(vehicle);

            CalibrateAndSetVehicleZ(vehicle);

            switch (vehicle.VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    break;
                case VehicleClass.BOSS_CLASS:
                    {
                        switch (vehicle.HonkState)
                        {
                            case HonkState.DEFAULT:
                                break;
                            case HonkState.HONKING:
                                {
                                    // make boss vehicle stop in the middle after reaching center of the road
                                    if (vehicleCloseHitBox.Left > _windowWidth / 2)
                                        vehicle.VehicleIntent = VehicleIntent.IDLE;
                                }
                                break;
                            case HonkState.HONKING_BUSTED:
                                {
                                    if (vehicle.VehicleIntent != VehicleIntent.MOVE)
                                        vehicle.VehicleIntent = VehicleIntent.MOVE;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }

            CalibrateAndMoveBehicle(vehicle, vehicleCloseHitBox);
            DetectAndRecycleVehicle(vehicle, vehicleCloseHitBox);

            var vehicleZ = vehicle.GetZ();

            if (vehicleZ > _player.GetZ())
                _player.SetZ(vehicleZ + 1);
        }

        private void CalibrateAndMoveBehicle(Vehicle vehicle, Rect vehicleCloseHitBox)
        {
            if (GameView.Children.OfType<Vehicle>().FirstOrDefault(x => x.StreamingDirection == vehicle.StreamingDirection
                            && x.GetCloseHitBox(_scale).IntersectsWith(vehicleCloseHitBox)) is Vehicle collidingVehicle)
            {
                if (vehicle.Speed > collidingVehicle.Speed)
                {
                    vehicle.Speed = collidingVehicle.Speed;

                    if (vehicle.VehicleIntent == VehicleIntent.MOVE)
                        MoveVehicle(collidingVehicle);
                }
                else if (collidingVehicle.Speed > vehicle.Speed)
                {
                    collidingVehicle.Speed = vehicle.Speed;

                    if (vehicle.VehicleIntent == VehicleIntent.MOVE)
                        MoveVehicle(vehicle);
                }
                else if (collidingVehicle.Speed == vehicle.Speed)
                {
                    switch (vehicle.StreamingDirection)
                    {
                        case StreamingDirection.UpWard:
                            {
                                if (vehicle.GetZ() > collidingVehicle.GetZ() && vehicle.VehicleIntent == VehicleIntent.MOVE && collidingVehicle.VehicleIntent == VehicleIntent.MOVE)
                                    MoveVehicle(collidingVehicle);
                                else if (vehicle.VehicleIntent == VehicleIntent.MOVE && collidingVehicle.VehicleIntent == VehicleIntent.MOVE)
                                    MoveVehicle(vehicle);
                            }
                            break;
                        case StreamingDirection.DownWard:
                            {
                                if (vehicle.GetZ() > collidingVehicle.GetZ() && vehicle.VehicleIntent == VehicleIntent.MOVE && collidingVehicle.VehicleIntent == VehicleIntent.MOVE)
                                    MoveVehicle(vehicle);
                                else if (vehicle.VehicleIntent == VehicleIntent.MOVE && collidingVehicle.VehicleIntent == VehicleIntent.MOVE)
                                    MoveVehicle(collidingVehicle);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (vehicle.VehicleIntent == VehicleIntent.MOVE)
                    MoveVehicle(vehicle);
            }
        }

        private void DetectAndRecycleVehicle(Vehicle vehicle, Rect vehicleCloseHitBox)
        {
            // recycle vehicle according to streaming direction
            switch (vehicle.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        if (vehicleCloseHitBox.Bottom < 0 || vehicleCloseHitBox.Right < 0)
                        {
                            switch (vehicle.VehicleClass)
                            {
                                case VehicleClass.DEFAULT_CLASS:
                                    {
                                        RecyleVehicle(vehicle);

                                        // loose health if a honking car escapes view without getting tagged with a sticker
                                        if (vehicle.HonkState == HonkState.HONKING)
                                            LooseHealth();
                                    }
                                    break;
                                case VehicleClass.BOSS_CLASS:
                                    {
                                        GameView.AddDestroyableGameObject(vehicle);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        if (vehicleCloseHitBox.Top > _windowHeight || vehicleCloseHitBox.Left > _windowWidth)
                        {
                            switch (vehicle.VehicleClass)
                            {
                                case VehicleClass.DEFAULT_CLASS:
                                    {
                                        RecyleVehicle(vehicle);

                                        // loose health if a honking car escapes view without getting tagged with a sticker
                                        if (vehicle.HonkState == HonkState.HONKING)
                                            LooseHealth();
                                    }
                                    break;
                                case VehicleClass.BOSS_CLASS:
                                    {
                                        GameView.AddDestroyableGameObject(vehicle);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void CalibrateAndSetVehicleZ(Vehicle vehicle)
        {
            var vehicle_distantHitBox = vehicle.GetDistantHitBox();

            switch (vehicle.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        if (GameView.Children.OfType<Vehicle>().FirstOrDefault(x => x.StreamingDirection == StreamingDirection.UpWard && x.GetDistantHitBox() is Rect x_DistantHitBox
                            && x_DistantHitBox.IntersectsWith(vehicle_distantHitBox)
                            && vehicle_distantHitBox.Bottom > x_DistantHitBox.Bottom
                            && vehicle.GetZ() <= x.GetZ()) is Vehicle underneathVehicle)
                        {
                            vehicle.SetZ(underneathVehicle.GetZ() + 1);
                        }
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        if (GameView.Children.OfType<Vehicle>().FirstOrDefault(x => x.StreamingDirection == StreamingDirection.DownWard && x.GetDistantHitBox() is Rect x_DistantHitBox
                           && x_DistantHitBox.IntersectsWith(vehicle_distantHitBox)
                           && vehicle_distantHitBox.Bottom > x_DistantHitBox.Bottom
                           && vehicle.GetZ() <= x.GetZ()) is Vehicle aboveVehicle)
                        {
                            vehicle.SetZ(aboveVehicle.GetZ() + 1);
                        }

                        if (GameView.Children.OfType<Vehicle>().FirstOrDefault(x => x.StreamingDirection == StreamingDirection.DownWard && x.GetDistantHitBox() is Rect x_DistantHitBox
                          && x_DistantHitBox.IntersectsWith(vehicle_distantHitBox)
                          && vehicle_distantHitBox.Bottom < x_DistantHitBox.Bottom
                          && vehicle.GetZ() > x.GetZ()) is Vehicle belowVehicle)
                        {
                            vehicle.SetZ(belowVehicle.GetZ() - 1);
                        }

                        if (GameView.Children.OfType<Vehicle>().FirstOrDefault(x => x.StreamingDirection == StreamingDirection.UpWard && x.GetDistantHitBox() is Rect x_DistantHitBox
                          && x_DistantHitBox.IntersectsWith(vehicle_distantHitBox)
                          && vehicle_distantHitBox.Bottom < x_DistantHitBox.Bottom
                          && vehicle.GetZ() >= x.GetZ()) is Vehicle belowVehicle2)
                        {
                            vehicle.SetZ(belowVehicle2.GetZ() - 1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void MoveVehicle(Vehicle vehicle, int divideSpeedBy = 1)
        {
            var vehicleSpeed = InGameMessageSlowMotionInEffect ? vehicle.Speed / _slowMotionFactor : vehicle.Speed;

            switch (vehicle.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        if (vehicle.GetLeft() < _windowWidth)
                            vehicle.SetTop(vehicle.GetTop() - (vehicleSpeed * 0.5) / divideSpeedBy);

                        vehicle.SetLeft(vehicle.GetLeft() - vehicleSpeed / divideSpeedBy);
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        if (vehicle.GetLeft() + vehicle.Width > 0)
                            vehicle.SetTop(vehicle.GetTop() + (vehicleSpeed * 0.5) / divideSpeedBy);

                        vehicle.SetLeft(vehicle.GetLeft() + vehicleSpeed / divideSpeedBy);
                    }
                    break;
                default:
                    break;
            }
        }

        private void RecyleVehicle(Vehicle vehicle)
        {
            switch (vehicle.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        _markNum = _random.Next(0, _vehicles_Up.Length);
                        vehicle.SetContent(_vehicles_Up[_markNum]);
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        _markNum = _random.Next(0, _vehicles_Down.Length);
                        vehicle.SetContent(_vehicles_Down[_markNum]);
                    }
                    break;
                default:
                    break;
            }

            vehicle.Speed = RandomizeVehicleSpeed();

            vehicle.ResetHonking(
                gameLevel: _gameLevel,
                honkTemplatesCount: _honkTemplatesCount,
                willHonk: Convert.ToBoolean(_random.Next(0, 2)));

            RandomizeVehiclePosition(vehicle);
        }

        private void RandomizeVehiclePosition(Vehicle vehicle)
        {
            var one4thHeight = GameView.Height / 4;
            var halfHeight = GameView.Height / 2;

            double left = 0;
            double top = 0;

            switch (vehicle.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        left = _random.Next(
                            minValue: (int)GameView.Width,
                            maxValue: (int)GameView.Width * _random.Next(1, 3));

                        // portrait
                        if (GameView.Height > GameView.Width)
                        {
                            top = _random.Next(
                                minValue: (int)(one4thHeight),
                                maxValue: (int)(halfHeight));

                            top += one4thHeight + vehicle.Height;
                        }
                        else // landscape
                        {
                            top = _random.Next(
                                minValue: (int)(one4thHeight + vehicle.Height),
                                maxValue: (int)(halfHeight + one4thHeight + vehicle.Height));

                            top += halfHeight;
                        }
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        left = _random.Next(
                           minValue: (int)(GameView.Width * _random.Next(1, 3)) * -1,
                           maxValue: 0);

                        // portrait
                        if (GameView.Height > GameView.Width)
                        {
                            top = _random.Next(
                                minValue: (int)(halfHeight - one4thHeight),
                                maxValue: (int)(GameView.Height - vehicle.Height - one4thHeight));

                            top -= (one4thHeight + vehicle.Height);
                        }
                        else // landscape
                        {
                            top = _random.Next(
                                minValue: (int)(halfHeight - (vehicle.Height * 2) - one4thHeight),
                                maxValue: (int)(GameView.Height - (vehicle.Height * 2) - one4thHeight));

                            top -= (halfHeight + (50 * _scale));
                        }
                    }
                    break;
                default:
                    break;
            }

            vehicle.SetPosition(
                left: left,
                top: top);
        }

        private double RandomizeVehicleSpeed()
        {
            if (_windowWidth > _windowHeight)
                return _gameSpeed + _random.Next(1, 5);
            else
                return _gameSpeed + _random.Next(0, 3);
        }

        #endregion

        #region Honk

        private void SpawnHonk(Vehicle vehicle)
        {
            Honk honk = new(
                scale: _scale,
                speed: vehicle.Speed * 1.4,
                streamingDirection: vehicle.StreamingDirection);

            var vehicleCloseHitBox = vehicle.GetCloseHitBox(_scale);

            _markNum = _random.Next(0, _honks.Length);
            honk.SetContent(_honks[_markNum]);

            switch (honk.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        honk.SetLeft(vehicleCloseHitBox.Left - vehicle.Width / 2);
                        honk.SetTop(vehicleCloseHitBox.Top - (25 * _scale));
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        honk.SetLeft(vehicleCloseHitBox.Right - vehicle.Width / 2);
                        honk.SetTop(vehicleCloseHitBox.Bottom - (75 * _scale));
                    }
                    break;
                default:
                    break;
            }

            honk.SetRotation(_random.Next(-30, 30));
            honk.SetZ(vehicle.GetZ() + 1);

            GameView.Children.Add(honk);

            switch (vehicle.VehicleClass)
            {
                case VehicleClass.DEFAULT_CLASS:
                    SoundHelper.PlaySound(soundType: SoundType.HONK, index: vehicle.HonkSoundIndex);
                    break;
                case VehicleClass.BOSS_CLASS:
                    SoundHelper.PlaySound(soundType: SoundType.BOSS_HONK, index: vehicle.HonkSoundIndex);
                    break;
                default:
                    break;
            }

            var honkHitBox = honk.GetHitBox();

            // only loose health if the honk is spawned inside game view
            if (honkHitBox.Top > 0 && honkHitBox.Top < _windowHeight && honkHitBox.Left > 0 && honkHitBox.Left < _windowWidth)
                LooseHealth();
        }

        private void UpdateHonk(Honk honk)
        {
            MoveHonk(honk);

            honk.Fade();

            if (honk.HasFaded)
                GameView.AddDestroyableGameObject(honk);
        }

        private void MoveHonk(Honk honk)
        {
            var honkSpeed = InGameMessageSlowMotionInEffect ? honk.Speed / _slowMotionFactor : honk.Speed;

            switch (honk.StreamingDirection)
            {
                case StreamingDirection.UpWard:
                    {
                        honk.SetTop(honk.GetTop() - honkSpeed * 0.5);
                        honk.SetLeft(honk.GetLeft() - honkSpeed);
                    }
                    break;
                case StreamingDirection.DownWard:
                    {
                        honk.SetTop(honk.GetTop() + honkSpeed * 0.5);
                        honk.SetLeft(honk.GetLeft() + honkSpeed);
                    }
                    break;
                default:
                    break;
            }
        }

        private bool WaitForHonk(Vehicle vehicle)
        {
            if (!InGameMessageSlowMotionInEffect)
            {
                var vehicleHitBox = vehicle.GetHitBox();

                switch (vehicle.StreamingDirection)
                {
                    case StreamingDirection.UpWard:
                        {
                            if (_windowWidth > _windowHeight)
                            {
                                if (vehicleHitBox.Right < _windowWidth + (_windowWidth / 3))
                                    return vehicle.WaitForHonk(_gameLevel);
                            }
                            else
                            {
                                if (vehicleHitBox.Right < _windowWidth * 2.5)
                                    return vehicle.WaitForHonk(_gameLevel);
                            }
                        }
                        break;
                    case StreamingDirection.DownWard:
                        {
                            if (_windowWidth > _windowHeight)
                            {
                                if (vehicleHitBox.Right > (_windowWidth / 3) * -1)
                                    return vehicle.WaitForHonk(_gameLevel);
                            }
                            else
                            {
                                if (vehicleHitBox.Right > _windowWidth * -2.5)
                                    return vehicle.WaitForHonk(_gameLevel);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return false;
        }

        private void BustHonk(Vehicle vehicle)
        {
            // honk busting is only possible when stickers are at hand
            if (_stickersAmount > 0)
            {
                Sticker sticker = SpawnSticker(vehicle);

                if (vehicle.BustHonking(sticker))
                {
                    _vehiclesTagged++;
                    AddScore(5);

                    if (vehicle.VehicleClass == VehicleClass.BOSS_CLASS)
                    {
                        StopBossSounds();
                        StartGameSounds();

                        vehicle.Speed = RandomizeVehicleSpeed();
                        vehicle.VehicleIntent = VehicleIntent.MOVE;

                        ShowInGameTextMessage(message: GetLocalizedResource("BOSS_CLEARED"), activateSlowMotion: true);
                    }
                }

                AddHealth();

                _stickersAmount--;
                SetStickersAmountText();

                SoundHelper.PlayRandomSound(SoundType.HONK_BUST);
            }
            else
            {
                ShowInGameTextMessage(GetLocalizedResource("COLLECT_STICKERS"));
            }
        }

        private bool IsHonkBusted(Vehicle vehicle)
        {
            return vehicle.IsHonkBusted();
        }

        private bool CanBustHonk(Vehicle vehicle, Rect vehicleCloseHitBox)
        {
            return vehicle.CanBustHonk(
                vehicleCloseHitBox: vehicleCloseHitBox,
                playerState: _player.PlayerState,
                playerHitBox: _playerHitBox);
        }

        #endregion

        #region Sticker

        private Sticker SpawnSticker(Vehicle vehicle)
        {
            Sticker sticker = new(_scale);

            MoveSticker(vehicle, sticker);

            sticker.SetZ(vehicle.GetZ() + 1);
            sticker.SetRotation(_random.Next(-30, 30));

            //sticker.SetSkewY(-30);
            //sticker.SetSkewY(30);

            GameView.Children.Add(sticker);

            return sticker;
        }

        private void UpdateSticker(Vehicle vehicle)
        {
            var sticker = vehicle.AttachedSticker;
            sticker.SetZ(vehicle.GetZ() + 1);

            MoveSticker(vehicle, sticker);

            var stickerHitBox = sticker.GetHitBox();

            if (stickerHitBox.Bottom < 0 || stickerHitBox.Right < 0 || stickerHitBox.Top > _windowHeight || stickerHitBox.Left > _windowWidth)
                GameView.AddDestroyableGameObject(sticker);
        }

        private void MoveSticker(Vehicle vehicle, Sticker sticker)
        {
            sticker.SetLeft(vehicle.GetLeft() + vehicle.Width / 2.5);
            sticker.SetTop(vehicle.GetTop() + vehicle.Height / 2.0);
        }

        #endregion        

        #region Collectible

        private void SpawnCollectibles()
        {
            // add some collectibles
            for (double i = 0; i < 7 * _scale; i++)
                SpawnCollectible();
        }

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);
            collectible.SetRotation(_random.Next(-30, 30));
            collectible.SetZ(60);

            RandomizeCollectiblePosition(collectible);

            GameView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject collectible)
        {
            MoveCollectible(collectible);

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

        private void MoveCollectible(GameObject collectible)
        {
            var collectibleSpeed = InGameMessageSlowMotionInEffect ? _gameSpeed / _slowMotionFactor : _gameSpeed;

            collectible.SetTop(collectible.GetTop() + collectibleSpeed);
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
                left: _random.Next((int)collectible.Width, (int)(GameView.Width - collectible.Width)),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void Collectible()
        {
            _collectibleCollected++;
            _stickersAmount++;
            SetStickersAmountText();

            if (InGameMessageText.Text == GetLocalizedResource("COLLECT_STICKERS"))
                HideInGameTextMessage();

            AddScore(1);
            SoundHelper.PlayRandomSound(SoundType.COLLECTIBLE);
        }

        #endregion

        #region Health

        private void AddHealth()
        {
            if (_playerHealth < 100)
            {
                var health = _playerHealPoints;

                if (_playerHealth + _playerHealPoints > 100)
                    health = _playerHealth + _playerHealPoints - 100;

                _playerHealth += health;

                SoundHelper.SetVolume(SoundType.SONG, _playerHealth / 100);
            }
        }

        private void LooseHealth()
        {
            SoundHelper.VolumeDown(SoundType.SONG);

            _playerHealth -= _playerHitPoints;

            SoundHelper.SetVolume(SoundType.SONG, _playerHealth / 100);

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
                left: _random.Next((int)powerUp.Width, (int)(GameView.Width - powerUp.Width)),
                top: _random.Next(100 * (int)_scale, (int)GameView.Height) * -1);
        }

        private void UpdatePowerUp(GameObject powerUp)
        {
            MovePowerUp(powerUp);

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

        private void MovePowerUp(GameObject powerUp)
        {
            var powerUpSpeed = InGameMessageSlowMotionInEffect ? _gameSpeed / _slowMotionFactor : _gameSpeed;

            powerUp.SetTop(powerUp.GetTop() + powerUpSpeed);
        }

        private void PowerUp(PowerUp powerUp)
        {
            _isPowerMode = true;

            _powerModeDurationCounter = _powerModeDurationDefault;
            _powerUpType = powerUp.PowerUpType;

            switch (_powerUpType)
            {
                case PowerUpType.MagnetPull:
                    ShowInGameTextMessage(message: GetLocalizedResource("MAGNET_PULL"), activateSlowMotion: true);
                    break;
                case PowerUpType.TwoxScore:
                    ShowInGameTextMessage(message: GetLocalizedResource("2X_SCORE"), activateSlowMotion: true);
                    break;
                default:
                    break;
            }

            PlayerPowerBar.Visibility = Visibility.Visible;
            SoundHelper.PlaySound(SoundType.POWER_UP);
        }

        private void PowerUpCoolDown()
        {
            _powerModeDurationCounter -= 1;
            double remainingPow = (double)_powerModeDurationCounter / (double)_powerModeDurationDefault * 100;
            PlayerPowerBar.Value = remainingPow;
        }

        private void PowerDown()
        {
            _isPowerMode = false;

            PlayerPowerBar.Visibility = Visibility.Collapsed;
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

        #region Boss

        private void EngageBoss()
        {
            StopGameSounds();

            Vehicle boss = SpawnVehicle(streamingDirection: StreamingDirection.DownWard, vehicleClass: VehicleClass.BOSS_CLASS);

            _markNum = _random.Next(0, _vehicles_Boss.Length);
            boss.SetContent(_vehicles_Boss[_markNum]);

            boss.Speed = RandomizeVehicleSpeed();

            var one4thHeight = GameView.Height / 4;
            var halfHeight = GameView.Height / 2;

            double left;
            double top;

            left = _random.Next(
                        minValue: (int)(GameView.Width * _random.Next(1, 3)) * -1,
                        maxValue: 0);

            // portrait
            if (GameView.Height > GameView.Width)
            {
                top = (GameView.Height - boss.Height - one4thHeight);

                top -= (one4thHeight + boss.Height);
            }
            else // landscape
            {
                top = (GameView.Height - (boss.Height * 2) - one4thHeight);

                top -= (halfHeight + (50 * _scale));
            }

            boss.SetPosition(
               left: left,
               top: top);

            boss.ResetHonking(
               gameLevel: _gameLevel,
               honkTemplatesCount: _bossHonkTemplatesCount,
               willHonk: true);

            PlayBossSounds();

            ShowInGameTextMessage(message: GetLocalizedResource("BOSS_INCOMING"), activateSlowMotion: true);
        }

        #endregion

        #endregion

        #region Score

        private void AddScore(double score)
        {
            score = TwoxScore(score);

            _score += score;

            SetScoreText();
            ScaleDifficulty();
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_gameLevel < 101 && _score > _scoreCap)
            {
                LevelUp();

                // TODO: enage boss after level 4
                if (_gameLevel > 2 && _gameLevel % 2 != 0)
                    EngageBoss();
#if DEBUG
                Console.WriteLine("PLAYER LAG: " + _playerLag);
                Console.WriteLine("GAME SPEED: " + _gameSpeed);
#endif
            }
        }

        private void LevelUp()
        {
            _gameSpeed = (_gameSpeedDefault * _scale) + (0.2 * _difficultyMultiplier / 2);

            if (_playerLag > 15)
                _playerLag = _playerLagDefault - (_difficultyMultiplier * 1.5);

            _scoreCap += (int)(50 * (_difficultyMultiplier / 2));

            _difficultyMultiplier++;
            _gameLevel++;

            SetGameLevelText();
            ShowInGameTextMessage(GetLocalizedResource("LEVEL_UP") + " " + _gameLevel, true);
            SoundHelper.PlaySound(SoundType.LEVEL_UP);
        }

        private void SetScoreText()
        {
            ScoreText.Text = $"🌟 {_score} / {_scoreCap}";
        }

        private void SetGameLevelText()
        {
            GameLevelText.Text = $"🔥 {_gameLevel}";
        }

        private void SetStickersAmountText()
        {
            StickersAmountText.Text = $"{_stickersAmount}";
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

        private void PlayBossSounds()
        {
            SoundHelper.PlaySound(SoundType.BOSS_ENTRY);
            SoundHelper.PlaySound(SoundType.BOSS_IDLING);
        }

        private void StopBossSounds()
        {
            SoundHelper.StopSound(SoundType.BOSS_IDLING);
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

            UnderView.Children.Clear();

            // potrait
            if (_windowHeight > _windowWidth)
            {
                for (int i = 0; i <= 10; i++)
                {
                    var left = (_windowWidth / 3 / 20) + (i * 200 * _scale);
                    var top = (_windowHeight / 3) + (i * 200 * _scale) * 0.5;

                    AddRoadMark(left / 1.1, top / 1.1);
                }
            }
            else // landscape
            {
                for (int i = -1; i <= 10; i++)
                {
                    var left = (60 * _scale) + (i * 200 * _scale);
                    var top = (5 * _scale) + (i * 200 * _scale) * 0.5;

                    AddRoadMark(left, top);
                }
            }

            var applicableWidth = _windowWidth > _windowHeight ? (_windowWidth / 2) * 1.1 : _windowWidth * 1.45;

            RoadSideLeftImage.Width = applicableWidth;
            RoadSideLeftImage.Height = _windowHeight;

            RoadSideRightImage.Width = applicableWidth;
            RoadSideRightImage.Height = _windowHeight;
#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");
#endif           
        }

        private void AddRoadMark(double left, double top)
        {
            GameObject gameObject = new()
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
            };

            gameObject.SetPosition(left: left, top: top);
            gameObject.SetSize(width: 20 * _scale, height: _windowHeight / 10);
            gameObject.SetSkewY(42);
            gameObject.SetRotation(-63.5);

            UnderView.Children.Add(gameObject);
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region InGameMessage

        private void ShowInGameTextMessage(string message, bool activateSlowMotion = false)
        {
            _inGameMessageCoolDownCounter = activateSlowMotion ? _inGameMessageCoolDownCounterDefault : 0;
            InGameMessageText.Text = message;
            InGameMessagePanel.Visibility = Visibility.Visible;
            InGameMessageSlowMotionInEffect = activateSlowMotion;
        }

        private string GetLocalizedResource(string resourceKey)
        {
            return LocalizationHelper.GetLocalizedResource(resourceKey);
        }

        private void HideInGameTextMessage()
        {
            InGameMessagePanel.Visibility = Visibility.Collapsed;
            InGameMessageText.Text = "";
        }

        public void CoolDownInGameTextMessage()
        {
            _inGameMessageCoolDownCounter--;

            if (_inGameMessageCoolDownCounter <= 0)
            {
                HideInGameTextMessage();

                if (InGameMessageSlowMotionInEffect)
                    InGameMessageSlowMotionInEffect = false;
            }
        }

        #endregion

        #endregion
    }
}
