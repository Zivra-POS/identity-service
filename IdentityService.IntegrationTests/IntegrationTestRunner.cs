using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IdentityService.IntegrationTests.Infrastructure;
using System.Reflection;
using Xunit;

namespace IdentityService.IntegrationTests;

/// <summary>
/// Test runner for executing all integration tests programmatically
/// Provides detailed test results and database cleanup
/// </summary>
public class IntegrationTestRunner
{
    private readonly ILogger<IntegrationTestRunner> _logger;
    private readonly IntegrationTestWebAppFactory _factory;

    public IntegrationTestRunner()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<IntegrationTestRunner>();
        _factory = new IntegrationTestWebAppFactory();
    }

    public async Task<TestRunResult> RunAllTestsAsync()
    {
        _logger.LogInformation("Starting Integration Test Run...");
        
        var startTime = DateTime.UtcNow;
        var testResults = new List<TestResult>();
        
        try
        {
            await _factory.InitializeAsync();
            
            // Get all test classes
            var testClasses = GetTestClasses();
            
            foreach (var testClass in testClasses)
            {
                _logger.LogInformation($"Running tests in {testClass.Name}...");
                
                var classResults = await RunTestsInClass(testClass);
                testResults.AddRange(classResults);
                
                // Reset database after each test class
                await _factory.ResetDatabaseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test execution");
            return new TestRunResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime
            };
        }
        finally
        {
            await _factory.DisposeAsync();
        }

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;
        
        var summary = new TestRunResult
        {
            Success = testResults.All(t => t.Passed),
            TestResults = testResults,
            Duration = duration,
            TotalTests = testResults.Count,
            PassedTests = testResults.Count(t => t.Passed),
            FailedTests = testResults.Count(t => !t.Passed)
        };
        
        LogTestSummary(summary);
        return summary;
    }

    private List<Type> GetTestClasses()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<Xunit.FactAttribute>() != null))
            .ToList();
    }

    private async Task<List<TestResult>> RunTestsInClass(Type testClass)
    {
        var results = new List<TestResult>();
        
        try
        {
            var instance = Activator.CreateInstance(testClass, _factory);
            var testMethods = testClass.GetMethods()
                .Where(m => m.GetCustomAttribute<Xunit.FactAttribute>() != null);

            foreach (var method in testMethods)
            {
                var testResult = await RunSingleTest(instance!, method);
                results.Add(testResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating test class {testClass.Name}");
            results.Add(new TestResult
            {
                TestName = $"{testClass.Name}.*",
                Passed = false,
                ErrorMessage = ex.Message
            });
        }

        return results;
    }

    private async Task<TestResult> RunSingleTest(object testInstance, MethodInfo testMethod)
    {
        var testName = $"{testInstance.GetType().Name}.{testMethod.Name}";
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug($"Running test: {testName}");
            
            // Initialize test if it implements IAsyncLifetime
            if (testInstance is IAsyncLifetime asyncLifetime)
            {
                await asyncLifetime.InitializeAsync();
            }
            
            // Run the test method
            var result = testMethod.Invoke(testInstance, null);
            if (result is Task task)
            {
                await task;
            }
            
            // Cleanup test
            if (testInstance is IAsyncLifetime asyncLifetimeCleanup)
            {
                await asyncLifetimeCleanup.DisposeAsync();
            }
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogDebug($"✅ {testName} passed in {duration.TotalMilliseconds:F0}ms");
            
            return new TestResult
            {
                TestName = testName,
                Passed = true,
                Duration = duration
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, $"❌ {testName} failed in {duration.TotalMilliseconds:F0}ms");
            
            return new TestResult
            {
                TestName = testName,
                Passed = false,
                ErrorMessage = ex.Message,
                Duration = duration
            };
        }
    }

    private void LogTestSummary(TestRunResult summary)
    {
        _logger.LogInformation("=".PadRight(80, '='));
        _logger.LogInformation("TEST EXECUTION SUMMARY");
        _logger.LogInformation("=".PadRight(80, '='));
        _logger.LogInformation($"Total Tests: {summary.TotalTests}");
        _logger.LogInformation($"Passed: {summary.PassedTests} ✅");
        _logger.LogInformation($"Failed: {summary.FailedTests} ❌");
        _logger.LogInformation($"Success Rate: {(summary.PassedTests / (double)summary.TotalTests * 100):F1}%");
        _logger.LogInformation($"Duration: {summary.Duration.TotalSeconds:F2} seconds");
        
        if (summary.FailedTests > 0)
        {
            _logger.LogInformation("");
            _logger.LogInformation("FAILED TESTS:");
            foreach (var failedTest in summary.TestResults.Where(t => !t.Passed))
            {
                _logger.LogInformation($"  ❌ {failedTest.TestName}: {failedTest.ErrorMessage}");
            }
        }
        
        _logger.LogInformation("=".PadRight(80, '='));
    }
}

public class TestRunResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public List<TestResult> TestResults { get; set; } = new();
}

public class TestResult
{
    public string TestName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}
