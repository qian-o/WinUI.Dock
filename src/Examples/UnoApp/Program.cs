﻿using Uno.UI.Hosting;
using UnoApp;

App.InitializeLogging();

UnoPlatformHost host = UnoPlatformHostBuilder.Create()
                                             .App(() => new App())
                                             .UseX11()
                                             .UseLinuxFrameBuffer()
                                             .UseMacOS()
                                             .UseWin32()
                                             .Build();

host.Run();