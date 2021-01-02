using System;
using Autofac;
using H.Core;
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
    TestModules.CreateSelectRunner(),
    TestModules.CreateRunnerWithPrintCommand(),
    new IntegrationRunner(),
    TestModules.CreateAliasRunnerCommand("torrent", "смотреть")
);
using var exceptions = container.EnableLoggingForServices();

var moduleService = container.Resolve<StaticModuleService>();
var recognitionService = container.Resolve<RecognitionService>();
var runnerService = container.Resolve<RunnerService>();
var deskbandService = container.Resolve<IpcClientService>();

moduleService.Add(new RecognitionServiceRunner(recognitionService));
moduleService.Add(new IpcClientServiceRunner("deskband", deskbandService));

while (true)
{
    var line = Console.ReadLine() ?? string.Empty;

    try
    {
        await runnerService.RunAsync(Command.Parse(line)).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Exception: {exception}");
    }
}
