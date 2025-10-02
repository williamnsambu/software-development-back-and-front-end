using System.Threading.Tasks;
using DevPulse.Api.Controllers;
using DevPulse.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace DevPulse.Tests.Api
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WithTokenPair()
        {
            // Arrange
            var email = "user@test.com";
            var password = "Pass123!";
            var expectedAccess = "access123";
            var expectedRefresh = "refresh123";

            _authServiceMock
                .Setup(x => x.RegisterAsync(email, password))
                .ReturnsAsync((expectedAccess, expectedRefresh));

            var request = new AuthController.RegisterRequest(email, password);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeOfType<AuthController.TokenPairResponse>()
                .Which.Should().BeEquivalentTo(new AuthController.TokenPairResponse(expectedAccess, expectedRefresh));
        }
    }
}
