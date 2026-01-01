using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics.Geometry;
using UnityEngine;
using Zen.Core;
using Zen.Debug;

namespace StrangeSpace
{
    public enum MainMenuNavigationState
    {
        GamePlay,
        UIWork
    }
    
    public class MainMenuState : GameState
    {
        // Dependencies
        private readonly IEventService _eventService;
        private readonly ISaveManager _saveManager;
        
        // State Objects
        private GameData _gameData;
        
        private MainMenuNavigationState? _navigationTarget;
        
        private bool _isNewGame;


        public MainMenuState(IEventService eventService, ISaveManager saveManager)
        {
            _eventService = eventService;
            _saveManager = saveManager;
        }
        protected override async UniTask OnEnter(object data)
        {
            Watch.Tik();
            
            // Handle The data sent by who changed to this state.
            HandleInputData(data);
            
            // RegisterHUD Elements
            //_mainMenuHUD = _uiManager.AddHUDElement<UIMainMenuHUD>();
            
            //_mainMenuHUD.Show();

            _gameData = await _saveManager.LoadGameDataAsync();

            if (_gameData == null)
            {
                _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, "No previous Save Data Found. Creating a new Game");
                _saveManager.NewGameData();
                _isNewGame = true;
            }
            else
            {
                _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, "Save Data Found");
                _isNewGame = false;
            }
            
            _navigationTarget = MainMenuNavigationState.GamePlay;
            // _navigationTarget = MainMenuNavigationState.UIWork;
            
            ZenLog.Info(LogCategory.System, $"[MainMenuState] -> Entered Main Menu. Took {Watch.Tok()} s");
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, "Entered [Main Menu State]");
        }

        private void HandleInputData(object data)
        {
            // TODO: Implement this to handle errors from GamePlayState
            if (data is GameStateMachineTransactionError error)
            {
                ZenLog.Info(LogCategory.System, $"[MainMenuState] -> Returning from a failed transaction {error.ErrorMessage}");
            }
        }
        
        protected override UniTask OnLeave()
        {
           // _uiManager.RemoveHUDElement<UIMainMenuHUD>();
            
            _eventService.RegisteEvent(EventService.EventType.GameLifeCycle, "Leaved [Main Menu State]");
            
            return UniTask.CompletedTask;
        }
        
        protected override void OnUpdate(float deltaTime)
        {
            if (_navigationTarget.HasValue)
            {
                switch (_navigationTarget.Value)
                {
                    case MainMenuNavigationState.GamePlay:
                        StartGame();
                        break;
                    case MainMenuNavigationState.UIWork:
                        MoveToState<UIWorkState>();
                        break;
                }
                _navigationTarget = null;
            }
        }
        
        private void StartGame()
        {
            var gamePlayStateData = new GameplayStateData
            {
                IsNewGame = _isNewGame,
                GameData = _gameData
            };
                
            MoveToState<GameplayState>(gamePlayStateData);
        }
    }
}