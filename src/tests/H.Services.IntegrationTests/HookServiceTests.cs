using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class HookServiceTests
    {
        [TestMethod]
        public async Task BaseTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var service = new HookService();
            var exceptions = new ExceptionsBag();
            service.EnableLogging(exceptions, cancellationTokenSource);

            await service.InitializeAsync(cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            exceptions.EnsureNoExceptions();
        }
    }
}
