using System;
using System.Threading;

namespace HomeCenter.NET.Utilities
{
    public static class SafeActions
    {
        public static Action<Exception> DefaultExceptionAction { get; set; } = 
            exception => Console.WriteLine($@"Exception: {exception}");
        
        public static void OnUnhandledException(object exceptionObject, Action<Exception>? exceptionAction = null)
        {
            if (!(exceptionObject is Exception exception))
            {
                exception = new NotSupportedException($"Unhandled exception doesn't derive from System.Exception: {exceptionObject}");
            }

            // Ignore ThreadAbortException
            if (exception is ThreadAbortException)
            {
                return;
            }

            if (exceptionAction != null)
            {
                exceptionAction(exception);
            }
            else
            {
                DefaultExceptionAction?.Invoke(exception);
            }
        }
    }
}
