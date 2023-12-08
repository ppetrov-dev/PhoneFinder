using Microsoft.Extensions.DependencyInjection;
using PhoneFinder;

var services = new ServiceCollection();
var startup = new Startup();
startup.ConfigureServices(services);

services.BuildServiceProvider()
    .GetRequiredService<IApplicationRunner>()
    .Run();
