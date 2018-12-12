using NLog;
using NRules.RuleModel;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Distance.Rules
{
    public static class ContextExtensions
    {
        static ContextExtensions()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            LogManager.Configuration = config;
        }

        static Logger m_logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Prints the Warning message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Warn(this IContext context, string message)
        {
            m_logger.Warn(message);
        }

        /// <summary>
        /// Prints the Error message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Error(this IContext context, string message)
        {
            m_logger.Error(message);
        }

        /// <summary>
        /// Prints the Information message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public static void Info(this IContext context, string message)
        {
            m_logger.Info(message);            
        }
    }
}
