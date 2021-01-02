using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Runners;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class IntegrationRunner : Runner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IntegrationRunner()
        {
            Add(AsyncAction.WithSingleArgument("say", SayAsync, "Arguments: text"));
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SayAsync(string text, CancellationToken cancellationToken = default)
        {
            var values = await RunAsync(new Command("synthesize", text), cancellationToken)
                .ConfigureAwait(false);
            var value = values.First();

            await RunAsync(new Command("play", value), cancellationToken)
                .ConfigureAwait(false);
        }

        #endregion
    }
}
