using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Sample.WebApi
{
    public class SampleJobData
    {
        public string Value1 { get; set; }
        public int Value2 { get; set; }
        public bool Value3 { get; set; }
        public DateTime Value4 { get; set; }
    }

    public class SampleJobs
    {
        private readonly ILogger<SampleJobs> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly Random _random;

        public SampleJobs(ILogger<SampleJobs> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _random = new Random();
        }

        private Task RandomDelay()
        {
            return Task.Delay(_random.Next(30000, 30000 * 3));
        }

        private bool ShouldThrowError()
        {
            return _random.Next(2) == 1;
        }

        private void FakeRequestDependency()
        {
            using var test = _telemetryClient.StartOperation<DependencyTelemetry>("FakeDependencyTelemetry");

            test.Telemetry.Success = true;
            test.Telemetry.ResultCode = "200";
            test.Telemetry.Duration = TimeSpan.FromSeconds(6);
        }

        public async Task Run()
        {
            _logger.LogInformation($"Attempting to run {nameof(Run)}..");

            await RandomDelay();
            FakeRequestDependency();

            if (ShouldThrowError())
            {
                throw new Exception("Oh no!");
            }
        }

        public async Task RunWithParams(int id, SampleJobData data)
        {
            _logger.LogInformation($"Attempting to run {nameof(RunWithParams)}..");

            await RandomDelay();
            FakeRequestDependency();
            if (ShouldThrowError())
            {
                throw new Exception("Oh no! BAD THINGS");
            }
        }

        public async Task<string> RunWithReturn()
        {
            _logger.LogInformation($"Attempting to run {nameof(RunWithReturn)}..");

            await RandomDelay();
            FakeRequestDependency();
            return "Success!";
        }
    }
}