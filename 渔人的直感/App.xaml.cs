using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using 渔人的直感.Models;

namespace 渔人的直感
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DebugLog.Initialize();
            DebugLog.Write("Application OnStartup.");

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DebugLog.Write($"Application OnExit (code={e.ApplicationExitCode}).");
            DebugLog.Shutdown();
            base.OnExit(e);
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DebugLog.Exception(e.Exception, "UI thread unhandled");
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                DebugLog.Exception(ex, e.IsTerminating ? "Fatal unhandled" : "AppDomain unhandled");
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DebugLog.Exception(e.Exception, "Unobserved task");
            e.SetObserved();
        }
    }
}
