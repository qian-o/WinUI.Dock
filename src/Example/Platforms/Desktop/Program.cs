using Example;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.UI.Adapter.Microsoft.Extensions.Logging;
using Uno.UI.Hosting;

LogExtensionPoint.AmbientLoggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();

    builder.SetMinimumLevel(LogLevel.Information);
});

LoggingAdapter.Initialize();

UnoPlatformHost host = UnoPlatformHostBuilder.Create()
                                             .App(() => new App())
                                             .UseX11()
                                             .UseMacOS()
                                             .UseWin32()
                                             .Build();

host.Run();