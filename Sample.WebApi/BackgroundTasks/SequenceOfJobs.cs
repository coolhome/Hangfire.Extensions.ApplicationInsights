using Hangfire;

namespace Sample.WebApi.BackgroundTasks;

public class SequenceOfJobs
{
    private readonly ILogger<SequenceOfJobs> _logger;
    private readonly IBackgroundJobClientV2 _backgroundJob;

    public SequenceOfJobs(ILogger<SequenceOfJobs> logger, IBackgroundJobClientV2 backgroundJob)
    {
        _logger = logger;
        _backgroundJob = backgroundJob;
    }

    public void StartSequence()
    {
        var job1 = _backgroundJob.Enqueue(() => Stage1());
        var job2 = _backgroundJob.ContinueJobWith(job1, () => Stage2());
        var job3 = _backgroundJob.ContinueJobWith(job2, () => Stage3());
    }

    public bool Stage1()
    {
        _logger.LogInformation("Stage 1");
        return true;
    }

    public bool Stage2()
    {
        _logger.LogInformation("Stage 2");
        return true;
    }

    public bool Stage3()
    {
        _logger.LogInformation("Stage 3");
        return true;
    }
}