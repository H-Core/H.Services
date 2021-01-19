using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Runners;
using H.Core.Utilities;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProcessSequenceRunner : Runner
    {
        #region Properties

        private RunnerService RunnerService { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public ProcessSequenceRunner(RunnerService runnerService)
        {
            RunnerService = runnerService ?? throw new ArgumentNullException(nameof(runnerService));

            Add(new ProcessAction("process-sequence", async (process, command, cancellationToken) =>
            {
                var count = int.Parse(command.Input.Arguments.ElementAt(0), CultureInfo.InvariantCulture);
                var commands = command.Input.Arguments
                    .Skip(1)
                    .Take(count)
                    .Select(Command.Parse)
                    .Cast<ICommand>()
                    .ToArray();
                var arguments = command.Input.Arguments
                    .Skip(1 + count)
                    .ToArray();

                await ProcessSequenceAsync(commands, process, arguments, cancellationToken)
                    .ConfigureAwait(false);

                return Value.Empty;
            }));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="externalProcess"></param>
        /// <param name="arguments"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IValue> ProcessSequenceAsync(
            ICommand[] commands, 
            IProcess<ICommand> externalProcess,
            string[]? arguments = null, 
            CancellationToken cancellationToken = default)
        {
            commands = commands ?? throw new ArgumentNullException(nameof(commands));
            externalProcess = externalProcess ?? throw new ArgumentNullException(nameof(externalProcess));

            var value = (IValue)new Value(arguments ?? EmptyArray<string>.Value);

            var process = RunnerService.Start(
                commands.First().WithMergedInput(value),
                cancellationToken);

            await externalProcess.WaitAsync(cancellationToken).ConfigureAwait(false);

            var output = await process.StopAsync(cancellationToken)
                .ConfigureAwait(false);

            value = output.Output;

            foreach (var command in commands.Skip(1))
            {
                var values = await RunAsync(
                        command.WithMergedInput(value), 
                        cancellationToken)
                    .ConfigureAwait(false);

                value = values.FirstOrDefault() ?? Value.Empty;
            }

            return value;
        }

        #endregion
    }
}
