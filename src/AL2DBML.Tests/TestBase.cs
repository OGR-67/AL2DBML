using AL2DBML.DI;
using Microsoft.Extensions.DependencyInjection;

public abstract class TestBase
{
    protected IServiceProvider Services { get; }

    protected TestBase()
    {
        Services = new ServiceCollection()
            .AddAL2Dbml()
            .BuildServiceProvider();
    }
}
