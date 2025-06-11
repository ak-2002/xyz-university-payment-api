
# XYZ University Payment API

## Overview
This API enables Family Bank to securely communicate payment information to XYZ University in real time. It includes:

- **Student Validation Endpoint:** Verify student enrollment status.
- **Payment Notification Endpoint:** Receive and process payment notifications from Family Bank.

## Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- An IDE such as [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/xyz-university-payment-api.git
   cd xyz-university-payment-api
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. The API will be available at `https://localhost:5001` (or the port shown in your console).

### Dependencies

- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.AspNetCore.Mvc.Core
- Swashbuckle.AspNetCore (for Swagger)

These dependencies are automatically restored by `dotnet restore`.

## API Endpoints

### Student Validation
- **POST** `/api/Student/validate`
- **Request Body:**
  ```json
  {
    "studentNumber": "S12345"
  }
  ```
- **Response:**
  ```json
  {
    "isValid": true,
    "studentName": "John Doe",
    "program": "CS"
  }
  ```

### Payment Notification
- **POST** `/api/Payment/notify`
- **Request Body:**
  ```json
  {
    "studentNumber": "S12345",
    "paymentReference": "PAY123456",
    "amountPaid": 50000,
    "paymentDate": "2025-06-11T08:01:07Z"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Payment processed successfully."
  }
  ```

### Get All Payments
- **GET** `/api/Payment`
- **Response:** List of all payments received.

## Testing the API

You can test the API using Swagger UI, available at `https://localhost:5001/swagger` after running the application.

## Notes

- The API uses an in-memory database for ease of testing and development.
- You can replace the database provider with a real SQL database by updating the `AppDbContext` configuration in `Program.cs`.

