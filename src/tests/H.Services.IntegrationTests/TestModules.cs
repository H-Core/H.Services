using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Notifiers;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Runners;
using H.Notifiers;
using H.Recognizers;
using H.Recorders;
using H.Runners;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    public static class TestModules
    {
        public static IRecorder CreateDefaultRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            return new NAudioRecorder();
        }
        
        public static IRecognizer CreateDefaultRecognizer()
        {
            return new WitAiRecognizer
            {
                Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
            };
        }

        public static INotifier CreateTimerNotifierWithPrintHelloWorldEach3Seconds()
        {
            return new TimerNotifier
            {
                Command = new Command("print", "Hello, World!"),
                Interval = TimeSpan.FromSeconds(3),
            };
        }

        public static INotifier CreateTimerNotifierWithDeskbandDateTimeEach1Seconds()
        {
            return new TimerNotifier
            {
                CommandFactory = () => new Command("deskband", "preview", $"{DateTime.Now:T}"),
                Interval = TimeSpan.FromSeconds(1),
            };
        }

        public static INotifier CreateTimerNotifierWithSleep5000Each5Seconds()
        {
            return new TimerNotifier
            {
                Command = new Command("sleep", "5000"),
                Interval = TimeSpan.FromSeconds(5),
            };
        }

        public static INotifier CreateTimerNotifierWithSyncSleep5000Each3Seconds()
        {
            return new TimerNotifier
            {
                Command = new Command("sync-sleep", "5000"),
                Interval = TimeSpan.FromSeconds(3),
            };
        }

        public static IRunner CreateRunnerWithPrintCommand()
        {
            return new Runner
            {
                SyncAction.WithSingleArgument("print", Console.WriteLine, "value"),
            };
        }

        public static IRunner CreateRunnerWithRunCommand()
        {
            var runner = new Runner();
            runner.Add(SyncAction.WithCommand(
                "run",
                command => runner.Run(
                    new Command(
                        command.Value.Arguments.ElementAt(0),
                        command.Value.Arguments.Skip(1).ToArray())),
                "command"));
            runner.Add(AsyncAction.WithCommand(
                "run-async", 
                (command, token) => runner.RunAsync(
                    new Command(
                        command.Value.Arguments.ElementAt(0), 
                        command.Value.Arguments.Skip(1).ToArray()), 
                    token), 
                "command"));
            
            return runner;
        }

        public static IRunner CreateAliasRunnerCommand(string name, params string[] aliases)
        {
            var runner = new Runner();
            foreach (var alias in aliases)
            {
                runner.Add(SyncAction.WithCommand(
                    alias,
                    command => runner.Run(new Command(name, command.Value.Arguments)),
                    "arguments"));
            }

            return runner;
        }

        public static IRunner CreateRunnerWithSyncSleepCommand()
        {
            return new Runner
            {
                SyncAction.WithSingleArgument(
                    "sync-sleep",
                    argument => Thread.Sleep(Convert.ToInt32(argument)),
                    "millisecondsDelay"),
            };
        }

        public static IRunner CreateRunnerWithSleepCommand()
        {
            return new Runner
            {
                AsyncAction.WithSingleArgument(
                    "sleep", 
                    (argument, cancellationToken) => Task.Delay(Convert.ToInt32(argument), cancellationToken),
                    "millisecondsDelay"),
            };
        }

        public static IRunner CreateLongJobRunnerCommand()
        {
            return new Runner
            {
                AsyncAction.WithSingleArgument(
                    "long-job",
                    async (argument, cancellationToken) =>
                    {
                        await Task.Delay(Convert.ToInt32(argument), cancellationToken).ConfigureAwait(false);
                        
                        return new Value(argument);
                    },
                    "millisecondsDelay"),
            };
        }

        public static IRunner CreateProcessJobRunnerCommand()
        {
            return new Runner
            {
                new ProcessAction(
                    "process-job",
                    async (process, command, cancellationToken) =>
                    {
                        await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                        return command.Value;
                    }),
            };
        }

        public static IRunner CreateTelegramRunner()
        {
            return new TelegramRunner
            {
                Token = Environment.GetEnvironmentVariable("TELEGRAM_HOMECENTER_BOT_TOKEN")
                        ?? throw new AssertInconclusiveException("TELEGRAM_HOMECENTER_BOT_TOKEN environment variable is not found."),
                DefaultUserId = 482553595,
            };
        }
    }
}
