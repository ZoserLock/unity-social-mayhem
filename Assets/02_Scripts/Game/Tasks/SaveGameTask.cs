using Cysharp.Threading.Tasks;
using Zen.Debug;

namespace StrangeSpace
{
    public class SaveGameTask : ZenTask
    {
        private ISaveManager _saveManager;
        private GameData _gameData;
        
        public SaveGameTask(ISaveManager saveSaveManager, GameData gameData)
        {
            _saveManager = saveSaveManager;
            _gameData = gameData;
        }

        protected override async UniTask<ETaskResult> OnExecute()
        {
            ZenLog.Info(LogCategory.Gameplay, "[SaveGameTask] -> Saving Game World");

            var success = await _saveManager.SaveGameDataAsync(_gameData);

            if (success)
            {
                return ETaskResult.Success;
            }
            
            return ETaskResult.Failure;
        }

        public override string GetTaskName()
        {
            return $"Save Game Task";
        }
    }
}