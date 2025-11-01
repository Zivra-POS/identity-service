using IdentityService.IntegrationTests;

namespace IdentityService.IntegrationTests;

/// <summary>
/// Console application to run integration tests programmatically
/// This allows running tests outside of test runners for CI/CD scenarios
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("🚀 Starting IdentityService Integration Tests");
        
        try
        {
            var runner = new IntegrationTestRunner();
            var result = await runner.RunAllTestsAsync();
            
            if (result.Success)
            {
                Console.WriteLine("✅ All tests passed successfully!");
                return 0; // Success exit code
            }
            else
            {
                Console.WriteLine("❌ Some tests failed.");
                Console.WriteLine($"Error: {result.ErrorMessage}");
                return 1; // Failure exit code
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Fatal error during test execution: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 2; // Fatal error exit code
        }
    }
}
