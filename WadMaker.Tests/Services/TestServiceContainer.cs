using WadMaker.Queries;

namespace WadMaker.Tests.Services;

public static class TestServiceContainer
{
    public static ServiceProvider CreateWithTestAnnotator()
  {
    return ServiceContainer.CreateServiceProvider(services =>
    {
      ServiceContainer.StandardDependencies(services);
      services.AddSingleton<IAnnotator, TestAnnotator>();
    });
  }

}
