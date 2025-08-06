public interface IConfig
{
  bool HandleYOffet { get; }
}

public class ConfigV0 : IConfig
{
  public bool HandleYOffet { get; } = false;
}

public class ConfigV1 : IConfig
{
  public bool HandleYOffet { get; } = true;
}
