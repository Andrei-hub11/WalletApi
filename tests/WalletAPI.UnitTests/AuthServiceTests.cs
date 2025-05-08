using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Moq;

using Shouldly;

using WalletAPI.Contracts.DTOs.Auth;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Implementations;

namespace WalletAPI.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Mock do UserManager
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Mock do IConfiguration
        _mockConfiguration = new Mock<IConfiguration>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        mockConfigSection.Setup(x => x.Value).Returns("your-256-bit-secret-key-here-for-testing-only");
        _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("your-256-bit-secret-key-here-for-testing-only");

        // Setup do DbContext em memória
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _authService = new AuthService(_mockUserManager.Object, _mockConfiguration.Object, _context);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User"
        };

        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = "Test@123"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe(user.Email);
        result.Name.ShouldBe(user.Name);
        result.Token.ShouldNotBeNullOrEmpty();
        result.Wallet.Balance.ShouldBe(0);
        result.RecentTransactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test@123"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(false);

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Test@123",
            Name = "New User"
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe(registerRequest.Email);
        result.Name.ShouldBe(registerRequest.Name);
        result.Token.ShouldNotBeNullOrEmpty();
        result.Wallet.Balance.ShouldBe(0);
        result.RecentTransactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenRegistrationFails()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Test@123",
            Name = "New User"
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Registration failed" }));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(registerRequest));
        exception.Message.ShouldContain("Registration failed");
    }
}