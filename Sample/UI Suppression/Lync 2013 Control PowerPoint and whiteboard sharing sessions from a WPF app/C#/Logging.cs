using System;
using System.Diagnostics;

namespace Lync
{
    /// <summary>
    /// A logger.
    /// </summary>
    public interface ILog
    {
        void Info(string message);

        void Info(string format, params object[] args);

        void Warn(string message);

        void Warn(string format, params object[] args); 

        void WarnException(string message, System.Exception exp);
        

        void Error(string message);

        void Error(string format, params object[] args);

        void ErrorException(string message, System.Exception exp);

        void Debug(string format, params object[] args);

        void Debug(string message);




    }

    /// <summary>
    /// Used to manage logging.
    /// </summary>
    public static class LogManager
    {


        /// <summary>
        /// Creates an <see cref="ILog"/> for the provided type.
        /// </summary>
        public static Func<Type, ILog> GetLog = type => NullLogInstance(type);

        private static ILog NullLogInstance(Type type)
        {
            return new NullLog(type);
        }

        #region Nested type: NullLog

        private class NullLog : ILog
        {

            private Type _targetType;
            internal NullLog(Type type)
            {
                _targetType = type;
            }

            private void WriteLog(string msg)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("TypeName: {0}  DateTime: {1} \n Message: {2} ", _targetType, DateTime.Now, msg));
            }

            public void Info(string msg)
            {
                WriteLog(msg);
            }

            public void Error(string msg)
            {
                WriteLog(msg);
            }

            public void ErrorException(string msg, Exception exp)
            {
                WriteLog(msg+exp.Message+" \n "+exp.StackTrace);
            }

            public void Debug(string msg)
            {
                WriteLog(msg);
            }

            public void Warn(string msg)
            {
                WriteLog(msg);
            }


            public void Info(string message, params object[] args)
            {
                Info(string.Format(message, args));
            }

            public void Warn(string message, params object[] args)
            {
                Warn(string.Format(message, args));
            }

            public void Error(string message, params object[] args)
            {
                Error(string.Format(message, args));
            }

            public void Debug(string message, params object[] args)
            {
                Debug(string.Format(message, args));
            }


            public void WarnException(string message, Exception exp)
            {
                WriteLog(message + exp.Message);
            }
        }

        #endregion
    }
}