using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.System;

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

        private double _gameSpeed = 3;
        private readonly double _gameSpeedDefault = 3;

        private double _honkSpeed = 2;

        private int _markNum;

        private Uri[] _vehicles;
        private Uri[] _honks;

        private readonly IBackendService _backendService;

        private Player _player;
        private Rect _playerHitBox;

        private double _playerHealth;

        private readonly double _playerPositionGrace = 7;

        private double _playerSpeed = 70;
        private readonly double _playerSpeedDefault = 70;

        private int _idleDurationCounter;
        private readonly int _idleDurationCounterDefault = 20;

        private double _score;
        private double _scoreCap;
        private double _difficultyMultiplier;

        private bool _isGameOver;

        private bool _isPointerActivated;
        private Point _pointerPosition;

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
                _isPointerActivated = true;

                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;

                _player.SetState(PlayerState.Flying);
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPointerActivated = false;

            _idleDurationCounter = _idleDurationCounterDefault;
            _player.SetState(PlayerState.Idle);
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
            for (int i = 0; i < 15; i++)
                SpawnVehicle();
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            _gameSpeed = _gameSpeedDefault * _scale;
            _playerSpeed = _playerSpeedDefault;

            ResetControls();

            _isGameOver = false;
            //_isPowerMode = false;

            //_powerModeDurationCounter = _powerModeDuration;
            //_powerUpCount = 0;

            _score = 0;
            _scoreCap = 50;
            _difficultyMultiplier = 1;

            //_collectibleCollected = 0;
            ScoreText.Text = "0";

            _playerHealth = 100;

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            RecycleGameObjects();
            RemoveGameObjects();
            StartGameSounds();

            SpawnPlayer();

            RunGame();
#if DEBUG
            Console.WriteLine($"GAME SPEED: {_gameSpeed}");
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

            UpdateGameObjects();
            RemoveGameObjects();

#if DEBUG
            GameElementsCount.Text = GameView.Children.Count.ToString();
#endif
        }

        private void ResetControls()
        {
            _isPointerActivated = false;
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
            SoundHelper.ResumeSound(SoundType.BACKGROUND);

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
                //CollectiblesCollected = _collectibleCollected
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);

            //TODO: take to game over page
            //NavigateToPage(typeof(GameOverPage));
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #region GameObject

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
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            // add player
            _player = new Player(_scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height - (50 * _scale));

            _player.SetZ(1);

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
                default:
                    break;
            }
        }

        private void PlayerIdle()
        {
            _idleDurationCounter--;

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

            if (_idleDurationCounter <= 0)
            {
                _idleDurationCounter = _idleDurationCounterDefault;
                _player.IdlingDirectionY = _player.IdlingDirectionY == IdlingDirectionY.Down ? IdlingDirectionY.Up : IdlingDirectionY.Down;
            }
        }

        private void PlayerFlying()
        {
            double left = _player.GetLeft();
            double top = _player.GetTop();

            double playerMiddleX = left + _player.Width / 2;
            double playerMiddleY = top + _player.Height / 2;

            if (_isPointerActivated)
            {
                // move up
                if (_pointerPosition.Y < playerMiddleY - _playerPositionGrace)
                {
                    var distance = Math.Abs(_pointerPosition.Y - playerMiddleY);
                    double speed = GetFlightSpeed(distance);

                    _player.SetTop(top - speed);
                }

                // move left
                if (_pointerPosition.X < playerMiddleX - _playerPositionGrace)
                {
                    var distance = Math.Abs(_pointerPosition.X - playerMiddleX);
                    double speed = GetFlightSpeed(distance);

                    _player.SetLeft(left - speed);
                    _player.SetFacingDirectionX(MovementDirectionX.Left);
                }

                // move down
                if (_pointerPosition.Y > playerMiddleY + _playerPositionGrace)
                {
                    var distance = Math.Abs(_pointerPosition.Y - playerMiddleY);
                    double speed = GetFlightSpeed(distance);

                    _player.SetTop(top + speed);
                }

                // move right
                if (_pointerPosition.X > playerMiddleX + _playerPositionGrace)
                {
                    var distance = Math.Abs(_pointerPosition.X - playerMiddleX);
                    double speed = GetFlightSpeed(distance);

                    _player.SetLeft(left + speed);
                    _player.SetFacingDirectionX(MovementDirectionX.Right);
                }
            }
        }

        private double GetFlightSpeed(double distance)
        {
            return distance / _playerSpeed;
            //return distance > 50 ? distance / _playerSpeed : (distance / _playerSpeed) * 2;
        }

        #endregion

        #region Honk

        private void SpawnHonk(Vehicle vehicle)
        {
            Honk honk = new(_scale);

            _markNum = _random.Next(0, _honks.Length);
            honk.SetContent(_honks[_markNum]);

            honk.SetLeft(vehicle.GetLeft());
            honk.SetTop(vehicle.GetTop());
            honk.SetRotation(_random.Next(-30, 45));
            GameView.Children.Add(honk);
        }

        private void UpdateHonk(GameObject honk)
        {
            honk.SetTop(honk.GetTop() - _honkSpeed);
            honk.Fade();

            if (honk.HasFaded)
                GameView.AddDestroyableGameObject(honk);
        }

        #endregion

        #region Sticker

        private Sticker SpawnSticker(Vehicle vehicle)
        {
            Sticker collectible = new(_scale);
            collectible.SetLeft(vehicle.GetLeft() + vehicle.Width / 2);
            collectible.SetTop(vehicle.GetTop());
            collectible.SetRotation(_random.Next(-30, 45));
            GameView.Children.Add(collectible);
            return collectible;
        }

        private void UpdateSticker(Vehicle vehicle)
        {
            var collectible = vehicle.AttachedCollectible;

            collectible.SetLeft(vehicle.GetLeft() + vehicle.Width / 2);
            collectible.SetTop(vehicle.GetTop());

            if (collectible.GetTop() + collectible.Height < 0 || collectible.GetLeft() + collectible.Width < 0)
                GameView.AddDestroyableGameObject(collectible);
        }

        #endregion

        #region Vehicle

        private void SpawnVehicle()
        {
            Vehicle vehicle = new(_scale, _gameSpeed + _random.Next(0, 2));
            GameView.Children.Add(vehicle);
        }

        private void UpdateVehicle(Vehicle vehicle)
        {
            vehicle.SetTop(vehicle.GetTop() - vehicle.Speed * 0.5);
            vehicle.SetLeft(vehicle.GetLeft() - vehicle.Speed);

            if (vehicle.IsBusted && vehicle.AttachedCollectible is not null)
            {
                UpdateSticker(vehicle);
            }

            // if player hits the vehicle, bust honking and attach sticker
            if (vehicle.IsHonking)
            {
                var vehicleHitbox = vehicle.GetHitBox();

                if (_playerHitBox.IntersectsWith(vehicleHitbox))
                {
                    SoundHelper.PlayRandomSound(SoundType.HONK_BUST);
                    AddScore(5);

                    vehicle.BustHonking();
                    Sticker collectible = SpawnSticker(vehicle);
                    vehicle.AttachCollectible(collectible);

                    //TODO: make player angry and change asset to honk busting
                }
            }

            if (vehicle.CanHonk())
            {
                SoundHelper.PlaySound(SoundType.HONK, vehicle.HonkIndex);
                SpawnHonk(vehicle);
            }

            //TODO: this is expensive
            // if vechicle will collide with another vehicle
            if (GameView.Children.OfType<GameObject>()
                .Where(x => (ElementType)x.Tag == ElementType.VEHICLE)
                .LastOrDefault(v => v.GetDistantHitBox(_scale)
                .IntersectsWith(vehicle.GetDistantHitBox(_scale))) is GameObject collidingVehicle)
            {
                // slower vehicles will slow down faster vehicles
                if (collidingVehicle.Speed > vehicle.Speed)
                {
                    vehicle.Speed = collidingVehicle.Speed;
                }
                else
                {
                    collidingVehicle.Speed = vehicle.Speed;
                }
            }

            if (vehicle.GetTop() + vehicle.Height < 0 || vehicle.GetLeft() + vehicle.Width < 0)
                RecyleVehicle(vehicle);
        }

        private void RecyleVehicle(Vehicle vehicle)
        {
            _markNum = _random.Next(0, _vehicles.Length);
            vehicle.SetContent(_vehicles[_markNum]);
            vehicle.Speed = _gameSpeed + _random.Next(0, 2);

            vehicle.ResetHonking();
            RandomizeVehiclePosition(vehicle);
        }

        private void RandomizeVehiclePosition(GameObject vehicle)
        {
            var lane = _lanes[_random.Next(0, _lanes.Count)];

            vehicle.SetPosition(
                left: _random.Next(minValue: (int)GameView.Width, maxValue: (int)GameView.Width * 2),
                top: /*_random.Next(minValue: (int)GameView.Height / 2, maxValue: (int)(GameView.Height * 2))*/(int)(lane.End));

#if DEBUG
            Console.WriteLine("LANE: " + lane);
#endif
        }

        #endregion

        #endregion

        #region Score

        private void AddScore(double score)
        {
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
                _playerSpeed = _playerSpeedDefault + (_difficultyMultiplier / 2);
                _scoreCap += 50;
                _difficultyMultiplier++;
            }
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.BACKGROUND);
            SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.BACKGROUND);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.BACKGROUND);
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

            HighWayDivider.Width = 20 * _scale;
            HighWayDivider.Height = _windowHeight;
            HighWayDivider.SetRotation(-63.5);
            HighWayDivider.SetLeft(_windowWidth / 2 - (HighWayDivider.Width / 2));
            HighWayDivider.SetSkewY(43);

            _player?.SetSize(
                    width: Constants.PLAYER_WIDTH * _scale,
                    height: Constants.PLAYER_HEIGHT * _scale);

            var laneHeight = _windowHeight / 4;
            _lanes.Clear();

            for (int i = 0; i <= 4; i++)
            {
                _lanes.Add((laneHeight * i, laneHeight * (i + 1)));
            }

#if DEBUG
            Console.WriteLine($"SCALE: {_scale}");
            var lanesDetails = string.Join(",", _lanes.Select(x => $"{x.Start}-{x.End}").ToArray());
            Console.WriteLine($"AVAILABLE LANES: {lanesDetails}");
#endif

            RoadMarkLeft1.Width = 15 * _scale;
            RoadMarkLeft1.Height = 30 * _scale;
            RoadMarkLeft1.SetTop(_lanes[0].End);
            RoadMarkLeft1.SetLeft(_windowWidth / 4 - (30 * _scale / 2));
            RoadMarkLeft1.SetSkewY(43);
            RoadMarkLeft1.SetRotation(-63.5);

            RoadMarkLeft2.Width = 15 * _scale;
            RoadMarkLeft2.Height = 30 * _scale;
            RoadMarkLeft2.SetTop(_lanes[1].End);
            RoadMarkLeft2.SetLeft(_windowWidth / 4 - (30 * _scale / 2));
            RoadMarkLeft2.SetSkewY(43);
            RoadMarkLeft2.SetRotation(-63.5);

            RoadMarkLeft3.Width = 15 * _scale;
            RoadMarkLeft3.Height = 30 * _scale;
            RoadMarkLeft3.SetTop(_lanes[2].Start);
            RoadMarkLeft3.SetLeft(_windowWidth / 4 - (30 * _scale / 2));
            RoadMarkLeft3.SetSkewY(43);
            RoadMarkLeft3.SetRotation(-63.5);

            RoadMarkLeft4.Width = 15 * _scale;
            RoadMarkLeft4.Height = 30 * _scale;
            RoadMarkLeft4.SetTop(_lanes[3].End);
            RoadMarkLeft4.SetLeft(_windowWidth / 4 - (30 * _scale / 2));
            RoadMarkLeft4.SetSkewY(43);
            RoadMarkLeft4.SetRotation(-63.5);

            RoadMarkLeft5.Width = 15 * _scale;
            RoadMarkLeft5.Height = 30 * _scale;
            RoadMarkLeft5.SetTop(_lanes[4].End);
            RoadMarkLeft5.SetLeft(_windowWidth / 4 - (30 * _scale / 2));
            RoadMarkLeft5.SetSkewY(43);
            RoadMarkLeft5.SetRotation(-63.5);
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
