using IdentityService.IntegrationTests.Infrastructure;
using Xunit;

namespace IdentityService.IntegrationTests;

[CollectionDefinition("AuthController Integration Tests")]
public class AuthControllerTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
