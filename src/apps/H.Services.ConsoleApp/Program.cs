using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Services;
using H.Services.Core;
using H.Services.IntegrationTests;

await using var container = IoCTests.CreateContainer(
    TestModules.CreateDefaultRecorder(),
    TestModules.CreateDefaultRecognizer(),
    TestModules.CreateDefaultSynthesizer(),
    TestModules.CreateDefaultPlayer(),
    TestModules.CreateDefaultSearcher(),
    TestModules.CreateTorrentRunner(),
    TestModules.CreateTelegramRunner(),
    TestModules.CreateRunnerWithPrintCommand(),
    new IntegrationRunner(),
    TestModules.CreateAliasRunnerCommand("torrent", "смотреть")
);
using var exceptions = container.EnableLoggingForServices();

var moduleService = container.Resolve<StaticModuleService>();
var recognitionService = container.Resolve<RecognitionService>();
var hookService = container.Resolve<HookService>();

await hookService.InitializeAsync();

moduleService.Add(new RecognitionServiceRunner(recognitionService));

await Task.Delay(Timeout.InfiniteTimeSpan).ConfigureAwait(false);
