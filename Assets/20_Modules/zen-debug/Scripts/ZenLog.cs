using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Serilog.Events;

namespace Zen.Debug
{
    [System.Serializable, System.Flags]
    public enum LogCategory : uint
    {
        None          = 0x0,        // Logs that do not have any category and cannot be filtered
        System        = 0x1,        // Logs related to core systems.
        Gameplay      = 0x2,        // Logs related to gameplay
        Network       = 0x4,        // Logs related to Network/UDP/TCP
        GUI           = 0x8,        // Logs related to GUI Systems
        OS            = 0x10,       // Logs related to Operating System
        Editor        = 0x20,       // Logs related to Zone
        World         = 0x40,       // Logs related to World
        GameLib       = 0x80,       // Logs related to Game Library
        Scripting     = 0x100,      // Logs related to Scripting
        ScriptObject  = 0x200,      // Logs related to ScriptObject
        Performance   = 0x8000,     // Logs related to Performance
        Memory        = 0x200000,   // Logs related to Memory
        
        
        AllProfile  = Memory | Performance, // Logs related to all profiler
        All = 0xffffffff            // All Categories.
    }

    public enum LogLevel
    {
        Verbose = 0,    // Logs that produce noise and are only useful in heavy debug scenarios. Only enabled in debug builds.
        Debug = 1,      // Logs used to track and debug something. Only enabled in debug builds.
        Info = 2,       // Information about the app. This level is the minimal for release app.
        Warning = 3,    // Warning information that need to be hightlighted, but that do not affect the application.
        Error = 4,      // Errors that can be handled.
        Fatal = 5,      // Errors that can't be handled and should close the app.
    }
    
    public static class ZenLog
    {
        private const string kCategoryTag    = "Category";
        private const string kEnvironmentTag = "Environment";

        public const string DefaultFileName = "zen-log";
        public const string DefaultFileExtension = ".txt";
        public const string DefaultMessageTemplate = "[{Environment}][>][{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{ThreadId,-3}][{Category,-11}] -> {Message:lj}";

        public static LoggerConfiguration CreateEmptyConfiguration()
        {
            var config = new LoggerConfiguration();

            config.MinimumLevel.Verbose();   // Set the Minimum level to Verbose
            config.Enrich.WithThreadId();    // Add Thread information 
            config.Enrich.FromLogContext();  // Add Context information

            return config;
        }

        public static void Initialize(LoggerConfiguration configuration = null)
        {
            // If not configuration is provided, create an empty one.
            configuration ??= CreateEmptyConfiguration();
            
            Log.Logger = configuration.CreateLogger();
        }
        
        // Close and flush the logs. Call when shutting down.
        public static void Deinitialize()
        {
            Log.CloseAndFlush();
        }

        // Transform a serilog event to a TimLog Event
        public static LogEventLevel LogLevelToLogEventLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Verbose:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Info:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
            }

            return LogEventLevel.Information;
        }

        // VERBOSE MESSAGE
        public static void Verbose(string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Verbose(messageTemplate);
            }
        }
        
        public static void Verbose<T0>(string messageTemplate, T0 arg0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Verbose(messageTemplate, arg0);
            }
        }
        
        public static void Verbose<T0, T1>(string messageTemplate, T0 arg0, T1 arg1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Verbose(messageTemplate, arg0, arg1);
            }
        }
        
        public static void Verbose<T0, T1,T2>(string messageTemplate, T0 arg0, T1 arg1, T2 arg2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Verbose(messageTemplate, arg0, arg1, arg2);
            }
        }
        
        public static void Verbose(LogCategory category, string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Verbose(messageTemplate);
            }
        }
        
        public static void Verbose<T0>(LogCategory category, string messageTemplate, T0 arg0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Verbose(messageTemplate, arg0);
            }
        }
        
        public static void Verbose<T0, T1>(LogCategory category, string messageTemplate, T0 arg0, T1 arg1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Verbose(messageTemplate, arg0, arg1);
            }
        }
        
        public static void Verbose<T0, T1,T2>(LogCategory category, string messageTemplate, T0 arg0, T1 arg1, T2 arg2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Verbose(messageTemplate, arg0, arg1, arg2);
            }
        }
        
        // DEBUG MESSAGE
        // [System.Diagnostics.Conditional(NOT RELEASE)]
        public static void Debug(string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Debug(messageTemplate);
            }
        }
        
        public static void Debug<T0>(string messageTemplate, T0 args0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Debug(messageTemplate,args0);
            }
        }
        
        public static void Debug<T0,T1>(string messageTemplate,T0 args0,T1 args1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Debug(messageTemplate,args0,args1);
            }
        }

        public static void Debug<T0,T1,T2>(string messageTemplate,T0 args0,T1 args1,T2 args2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Debug(messageTemplate,args0,args1,args2);
            }
        }
        
        public static void Debug(LogCategory category, string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Debug(messageTemplate);
            }
        }
        
        public static void Debug<T0>(LogCategory category, string messageTemplate, T0 args0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Debug(messageTemplate,args0);
            }
        }
        
        public static void Debug<T0,T1>(LogCategory category, string messageTemplate,T0 args0,T1 args1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Debug(messageTemplate,args0,args1);
            }
        }

        public static void Debug<T0,T1,T2>(LogCategory category, string messageTemplate,T0 args0,T1 args1,T2 args2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Debug(messageTemplate,args0,args1,args2);
            }
        }
        
        // INFO MESSAGE
        public static void Info(string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Information(messageTemplate);
            }
        }
        
        public static void Info<T0>(string messageTemplate,T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Information(messageTemplate, Args0);
            }
        }
        
        public static void Info<T0,T1>(string messageTemplate,T0 Args0,T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Information(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Info<T0,T1,T2>(string messageTemplate,T0 Args0,T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Information(messageTemplate, Args0, Args1, Args2);
            }
        }
        
        public static void Info(LogCategory category, string messageTemplate)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Information(messageTemplate);
            }
        }
        
        public static void Info<T0>(LogCategory category, string messageTemplate,T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Information(messageTemplate, Args0);
            }
        }
        
        public static void Info<T0,T1>(LogCategory category, string messageTemplate,T0 Args0,T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Information(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Info<T0,T1,T2>(LogCategory category, string messageTemplate,T0 Args0,T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Information(messageTemplate, Args0, Args1, Args2);
            }
        }
        
        // WARNING MESSAGE
        public static void Warning(string data)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Warning(data);
            }
        }
        
        public static void Warning<T0>(string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Warning(messageTemplate, Args0);
            }
        }
        
        public static void Warning<T0,T1>(string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Warning(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Warning<T0,T1,T2>(string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Warning(messageTemplate, Args0, Args1, Args2);
            }
        }
        
        public static void Warning(LogCategory category, string data)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Warning(data);
            }
        }
        
        public static void Warning<T0>(LogCategory category, string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Warning(messageTemplate, Args0);
            }
        }
        
        public static void Warning<T0,T1>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Warning(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Warning<T0,T1,T2>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Warning(messageTemplate, Args0, Args1, Args2);
            }
        }

        // ERROR MESSAGE
        public static void Error(string data)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Error(data);
            }
        }
        
        public static void Error<T0>(string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Error(messageTemplate, Args0);
            }
        }
        
        public static void Error<T0,T1>(string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Error(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Error<T0,T1,T2>(string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Error(messageTemplate, Args0, Args1, Args2);
            }
        }
        public static void Error(LogCategory category, string data)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Error(data);
            }
        }
        
        public static void Error<T0>(LogCategory category, string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Error(messageTemplate, Args0);
            }
        }
        
        public static void Error<T0,T1>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Error(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Error<T0,T1,T2>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Error(messageTemplate, Args0, Args1, Args2);
            }
        }
        
        // FATAL MESSAGE
        public static void Fatal(string data)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Fatal(data);
            }
        }
        
        public static void Fatal<T0>( string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Fatal(messageTemplate, Args0);
            }
        }
        
        public static void Fatal<T0,T1>(string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Fatal(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Fatal<T0,T1,T2>(string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, LogCategory.All))
            {
                Log.Fatal(messageTemplate, Args0, Args1, Args2);
            }
        }
        public static void Fatal(LogCategory category, string data)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Fatal(data);
            }
        }
        
        public static void Fatal<T0>(LogCategory category, string messageTemplate, T0 Args0)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Fatal(messageTemplate, Args0);
            }
        }
        
        public static void Fatal<T0,T1>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Fatal(messageTemplate, Args0, Args1);
            }
        }
        
        public static void Fatal<T0,T1,T2>(LogCategory category, string messageTemplate, T0 Args0, T1 Args1, T2 Args2)
        {
            using (LogContext.PushProperty(kCategoryTag, category))
            {
                Log.Fatal(messageTemplate, Args0, Args1, Args2);
            }
        }
        
        public static IDisposable PushEnvironment(string environment)
        {
            return LogContext.PushProperty(kEnvironmentTag, environment);
        }
    }
}
