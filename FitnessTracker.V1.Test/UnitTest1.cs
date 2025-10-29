using Xunit;
using FitnessTracker.V1.Services;
using FluentAssertions;
using Moq;
using Blazored.LocalStorage;
using FitnessTracker.V1.Services.Data;

public class PoidsServiceTests
{
    [Fact]
    public void PoidsService_Should_Initialize_Correctly()
    {
        // Arrange
        var mockLocalStorage = new Mock<ILocalStorageService>();
        var mockSupabase = new Mock<SupabaseService2>();

        var service = new PoidsService(mockLocalStorage.Object, mockSupabase.Object);

        // Act & Assert
        service.Should().NotBeNull();
    }
}
