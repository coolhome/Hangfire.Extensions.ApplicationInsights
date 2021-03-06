[![Latest version](https://img.shields.io/nuget/v/Hangfire.Extensions.ApplicationInsights.svg)](https://www.nuget.org/packages/Hangfire.Extensions.ApplicationInsights/) ![GitHub](https://img.shields.io/github/license/coolhome/Hangfire.Extensions.ApplicationInsights)

# Hangfire.Extensions.ApplicationInsights
Provides Jobs as Application Insight requests.

![Example 1](./example.png)

![Example 2](./example2.png)

## Install Package
This assumes you already have `Hangfire` and `Microsoft.ApplicationInsights` already installed and configured.

First install the `Hangfire.Extensions.ApplicationInsights` NuGet package:
```ps
Install-Package Hangfire.Extensions.ApplicationInsights
```

Add the following line `ConfigureService` add the following
```cs
services.AddHangfireApplicationInsights();
```

See [Sample Project](./Sample) for full example.
