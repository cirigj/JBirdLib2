// Comment out the following line to not show JDebug logs.
#define SHOW_MESSAGES

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBirdLib
{

    /// <summary>
    /// Helper functions for debugging with consistent formatting.
    /// </summary>
    public static class JDebug
    {
        /// <summary>
        /// Extension method for formatting log messages.
        /// </summary>
        public static void Log(this Object obj, string message) {
            Debug.Log(obj.MessageFormat(message));
        }

        /// <summary>
        /// Extension method for formatting log warning messages.
        /// </summary>
        public static void Warning(this Object obj, string message) {
            Debug.LogWarning(obj.MessageFormat(message));
        }

        /// <summary>
        /// Extension method for formatting log error messages.
        /// </summary>
        public static void Error(this Object obj, string message) {
            Debug.LogError(obj.MessageFormat(message));
        }

        /// <summary>
        /// Internal method for formatting debug messages.
        /// </summary>
        internal static string MessageFormat(this Object obj, string message) {
            return string.Format("{0}: {1}", obj.GetType().Name, message);
        }
    }
}