using System;
using System.IO;

using Serilog;
using Serilog.Filters;
using Serilog.Sinks.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using Zen.Debug;

namespace StrangeSpace
{
    public class ApplicationRoot : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] 
        private ApplicationSettings _applicationSettings;
        [SerializeField] 
        private GameSettings _gameSettings;
        
        [SerializeField]
        private Camera _applicationCamera;
        
        [SerializeField]
        private SingletonRoot _singletonRoot;
        
        private ZenApplication _application;
        
        // Get / Set
        public SingletonRoot SingletonRoot => _singletonRoot;
        public Camera ApplicationCamera => _applicationCamera;
        public ApplicationSettings ApplicationSettings => _applicationSettings;
        public GameSettings GameSettings => _gameSettings;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            InitializeLogger();

            ZenLog.Info(LogCategory.System,"[ApplicationRoot] -> Initializing Application");

            _application = new ZenApplication();
            
            _application.Initialize(this);
            
            ZenLog.Info(LogCategory.System,"[ApplicationRoot] -> Application Initialized");
        }
        
        private void OnApplicationQuit()
        {
            ZenLog.Info(LogCategory.System,"[ApplicationRoot]: Deinitializing Application");
            
            // The system Deinitialization is only sure to run 1 frame.
            _application.Deinitialize();
            _application = null;
        }
        
        private void Update()
        {
            _application.Update(Time.deltaTime);
        }
        
        private void InitializeLogger()
        {
            var configuration = ZenLog.CreateEmptyConfiguration();

            const string messageTemplate = ZenLog.DefaultMessageTemplate;

            var logFilePath = Path.Combine(Application.persistentDataPath,
                $"{ZenLog.DefaultFileName}{ZenLog.DefaultFileExtension}");

            configuration.WriteTo.File(logFilePath,
                outputTemplate: messageTemplate,
                restrictedToMinimumLevel: ZenLog.LogLevelToLogEventLevel(LogLevel.Verbose));

            configuration.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(Matching.WithProperty("FromUnity"))
                .WriteTo.Unity(outputTemplate: messageTemplate,
                    restrictedToMinimumLevel: ZenLog.LogLevelToLogEventLevel(LogLevel.Info)));

            ZenLog.Initialize(configuration);
        }
        
    }
}