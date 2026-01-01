using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

using System;
using System.IO;
using UnityEngine;

namespace Serilog.Sinks.Unity
{
    public sealed class UnityLogEventSink : ILogEventSink
    {
        private MessageTemplateTextFormatter _formatter;

        public UnityLogEventSink(string outputTemplate, IFormatProvider formatProvider)
        {
            _formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        }

        public void Emit(LogEvent logEvent)
        {
            using var writer = new StringWriter();
            _formatter.Format(logEvent, writer);
            var message = writer.ToString();

            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                case LogEventLevel.Information:
                    Debug.Log(message);
                    break;

                case LogEventLevel.Warning:
                    Debug.LogWarning(message);
                    break;

                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    Debug.LogError(message);
                    break;
            }
        }
    }

    public static class UnityLogEventSinkExtensions
    {
        public static LoggerConfiguration Unity(this LoggerSinkConfiguration loggerConfiguration,
            string outputTemplate, 
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            return loggerConfiguration.Sink(new UnityLogEventSink(outputTemplate,formatProvider), 
                restrictedToMinimumLevel: restrictedToMinimumLevel);
        }
    }
}