using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostPayment_ShouldReturnBadRequest_ForNonExistentStudent()
        {
            // Arrange
            var payment = new { PaymentReference = "REF789", StudentNumber = "NONEXISTENT", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPayment_ShouldReturnBadRequest_ForInactiveStudent()
        {
            // Arrange
            var payment = new { PaymentReference = "REF101", StudentNumber = "INACTIVE", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payment/notify", payment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPayment_ShouldReturnBadRequest_ForDuplicatePaymentReference()
        {
            // Arrange: Post a valid payment first
            var initialPayment = new { PaymentReference = "REF111", StudentNumber = "S66002", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };
            var initialResponse = await _client.PostAsJsonAsync("/api/payment/notify", initialPayment);
            initialResponse.EnsureSuccessStatusCode();

            // Try posting the same reference again
            var duplicatePayment = new { PaymentReference = "REF111", StudentNumber = "S66002", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };
            var duplicateResponse = await _client.PostAsJsonAsync("/api/payment/notify", duplicatePayment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task PostPayment_ShouldReturnBadRequest_ForInvalidPaymentData()
        {
            // Arrange: Missing StudentNumber
            var invalidPayment = new { PaymentReference = "REF202", AmountPaid = 5000, PaymentDate = DateTime.UtcNow };
            var response = await _client.PostAsJsonAsync("/api/payment/notify", invalidPayment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
