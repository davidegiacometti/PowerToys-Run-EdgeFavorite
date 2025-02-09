// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite
{
    public class WoxLogger : ILogger
    {
        public void LogDebug(string message, Type fullClassName, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log.Debug(message, fullClassName, methodName, sourceFilePath, sourceLineNumber);
        }

        public void LogError(string message, Type fullClassName, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log.Error(message, fullClassName, methodName, sourceFilePath, sourceLineNumber);
        }

        public void LogError(Exception exception, string message, Type fullClassName, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log.Exception(message, exception, fullClassName, methodName, sourceFilePath, sourceLineNumber);
        }

        public void LogInformation(string message, Type fullClassName, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log.Info(message, fullClassName, methodName, sourceFilePath, sourceLineNumber);
        }

        public void LogWarning(string message, Type fullClassName, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log.Warn(message, fullClassName, methodName, sourceFilePath, sourceLineNumber);
        }
    }
}
