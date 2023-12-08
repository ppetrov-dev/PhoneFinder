using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using PhoneFinder.Repositories;
using PhoneFinder.Services;
using PhoneFinder.States;

namespace PhoneFinder;

internal class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        RegisterServices(services);
        RegisterRepositories(services);
        RegisterStates(services);
        services.AddTransient<IApplicationRunner, ApplicationRunner>();

        services.AddLogging(
            builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddNLog("NLog.Config");
            });
    }

    private void RegisterStates(IServiceCollection services)
    {
        services.AddSingleton<IStateFactory, StateFactory>();
        services.AddSingleton<IStateMachine, StateMachine>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISmsService, SmsService>();
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IBalanceChecker, BalanceChecker>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddSingleton<IGoalPhoneRangeRepository, GoalPhoneRangeRepository>();
        services.AddSingleton<IAccountRepository, AccountRepository>();
    }
}
