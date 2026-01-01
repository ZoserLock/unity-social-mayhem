using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zen.Debug;

namespace StrangeSpace
{
    public class LoadSceneTask : ZenTask
    {
        private string _sceneName;
        private Scene _scene;
        
        // Get / Set
        public Scene Scene => _scene;

        public LoadSceneTask(string sceneName)
        {
            _sceneName = sceneName;
        }
        
        protected override async UniTask<ETaskResult> OnExecute()
        {
            var scene = SceneManager.GetSceneByName(_sceneName);
            if (scene.isLoaded)
            {
                ZenLog.Warning($"Scene {_sceneName} is already loaded!");
                return ETaskResult.Success;
            }
            
            var asyncOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);

            // Wait until the scene is loaded up to 90%
            while (asyncOperation.progress < 0.9f)
            {
                var loadingProgress = Mathf.Round(asyncOperation.progress * 100);
                ZenLog.Warning($"Loading scene: {loadingProgress}%");
                
                await UniTask.NextFrame();
            }
            
            while (!asyncOperation.isDone)
            {
                await UniTask.NextFrame();
            }

            _scene = SceneManager.GetSceneByName(_sceneName);
            
            SceneManager.SetActiveScene(_scene);
            
            return ETaskResult.Success;
        }

        protected override void OnCompleted(ETaskResult result)
        {
            if (result != ETaskResult.Success)
            {
                ZenLog.Error($"Failed to load scene {_sceneName} this needs cleanup!");
            }
        }
    }
}