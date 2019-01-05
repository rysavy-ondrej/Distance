using NLog;
using NRules.RuleModel;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Distance.Runtime
{
    public static class Context
    {
        static readonly string DistanceLog = "DISTANCE.LOGS";
        static readonly string DistanceEvt = "DISTANCE.EVTS";


        static Logger m_logger; 
        static Logger m_eventLogger;

        public static void ConfigureLog(string filename)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget(DistanceLog) { FileName = filename };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile, DistanceLog);

            var logconsole = new NLog.Targets.ColoredConsoleTarget(DistanceEvt);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, logconsole, DistanceEvt);

            LogManager.Configuration = config;

            m_logger = LogManager.GetLogger(DistanceLog);
            m_eventLogger = LogManager.GetLogger(DistanceEvt);
        }

        /// <summary>
        /// Prints the Warning message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Warn(this IContext context, string message)
        {                  
            m_logger.Warn($"{context.Rule.Name}: {message}");
        }

        /// <summary>
        /// Prints the Error message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Error(this IContext context, string message)
        {
            m_logger.Error($"{context.Rule.Name}: {message}");
        }

        /// <summary>
        /// Prints the Information message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Info(this IContext context, string message)
        {
            m_logger.Info($"{context.Rule.Name}: {message}");            
        }

        public static void Event(this IContext context, DistanceEvent @event)
        {
            switch(@event.Severity)
            {
                case EventSeverity.Information:
                    m_eventLogger.Info($"{@event.Name}: {@event.Message}");
                    break;
                case EventSeverity.Warning:
                    m_eventLogger.Warn($"{@event.Name}: {@event.Message}");
                    break;
                case EventSeverity.Error:
                    m_eventLogger.Error($"{@event.Name}: {@event.Message}");
                    break;
            }
        }
    }
}
