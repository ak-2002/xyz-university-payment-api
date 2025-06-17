using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace xyz_university_payment_api.Tests.IntegrationTests
{
    public class PaymentIntegrationTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public PaymentIntegrationTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostPayment_ShouldReturnOk()
        {
            // Arrange
            var payment = new { PaymentReference = "REF456", StudentNumber = "S66002", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            response.EnsureSuccessStatusCode(); // Returns 200 OK
        }

        [Fact]
        public async Task PostPayment_WithInvalidStudent_ShouldReturnBadRequest()
        {
            // Arrange
            var payment = new { PaymentReference = "REF789", StudentNumber = "INVALID123", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPayment_WithMissingFields_ShouldReturnBadRequest()
        {
            // Arrange - missing PaymentDate
            var payment = new { PaymentReference = "REF999", StudentNumber = "S002", AmountPaid = 5000 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPayment_WithDuplicateReference_ShouldReturnBadRequest()
        {
            // Arrange - send the same payment twice
            var payment = new { PaymentReference = "REFDUPLICATE", StudentNumber = "S66002", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };

            // First request should succeed
            var firstResponse = await _client.PostAsJsonAsync("/api/payment/notify", payment);
            firstResponse.EnsureSuccessStatusCode();

            // Second request should fail (if the API correctly handles duplicates)
            var secondResponse = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            Assert.False(secondResponse.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }
    }
}
