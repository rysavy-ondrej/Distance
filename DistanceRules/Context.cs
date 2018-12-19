using NLog;
using NRules.RuleModel;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Distance.Rules
{
    public static class Context
    {
        static Logger m_logger = LogManager.GetLogger("DISTANCE");
        
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
    }
}
