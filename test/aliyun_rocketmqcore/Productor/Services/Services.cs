namespace Productor.Services;

public class ScopedService:IScopedService
{
    private readonly ILogger<ScopedService> _logger;

    public ScopedService(ILogger<ScopedService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public int OutPutHashCode()
    {
        var code=this.GetHashCode();
        _logger.LogInformation("作用域服务，hashcode：{code}", code);
        return code;
    }
}


public class SignalService:ISignalService
{
    private readonly ILogger<SignalService> _logger;

    public SignalService(ILogger<SignalService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public int OutPutHashCode()
    {
        var code=this.GetHashCode();
        _logger.LogInformation("单例服务，hashcode：{code}", code);
        return code;
    }
}


public class TranService:ITranService
{
    private readonly ILogger<TranService> _logger;

    public TranService(ILogger<TranService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public int OutPutHashCode()
    {
        var code=this.GetHashCode();
        _logger.LogInformation("瞬态服务，hashcode：{code}", code);
        return code;
    }
}