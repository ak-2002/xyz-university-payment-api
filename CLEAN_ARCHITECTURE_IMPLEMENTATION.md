# Clean Architecture & REST API Conventions Implementation

## 🏗️ **Clean Architecture Structure**

### **Current Structure → Clean Architecture**

```
xyz-university-payment-api/
├── 📁 Core/                           # Domain & Application Layers
│   ├── 📁 Domain/                     # Enterprise Business Rules
│   │   ├── 📁 Entities/               # Business entities
│   │   ├── 📁 ValueObjects/           # Value objects
│   │   ├── 📁 Enums/                  # Domain enums
│   │   └── 📁 Exceptions/             # Domain exceptions
│   ├── 📁 Application/                # Application Business Rules
│   │   ├── 📁 Interfaces/             # Repository & Service interfaces
│   │   ├── 📁 DTOs/                   # Data Transfer Objects
│   │   ├── 📁 Commands/               # Command objects
│   │   ├── 📁 Queries/                # Query objects
│   │   ├── 📁 Validators/             # FluentValidation rules
│   │   └── 📁 Services/               # Application services
│   └── 📁 Shared/                     # Shared utilities
│       ├── 📁 Extensions/             # Extension methods
│       └── 📁 Constants/              # Application constants
├── 📁 Infrastructure/                 # External concerns
│   ├── 📁 Data/                       # Data access layer
│   │   ├── 📁 Context/                # DbContext
│   │   ├── 📁 Repositories/           # Repository implementations
│   │   ├── 📁 Migrations/             # EF Core migrations
│   │   └── 📁 Configurations/         # Entity configurations
│   ├── 📁 External/                   # External services
│   │   ├── 📁 Messaging/              # RabbitMQ, etc.
│   │   ├── 📁 Caching/                # Redis implementation
│   │   └── 📁 Authentication/         # JWT, Identity Server
│   └── 📁 Logging/                    # Serilog configuration
├── 📁 Presentation/                   # User Interface Layer
│   ├── 📁 Controllers/                # API Controllers
│   ├── 📁 Middleware/                 # Custom middleware
│   ├── 📁 Filters/                    # Action filters
│   └── 📁 Attributes/                 # Custom attributes
└── 📁 Tests/                          # Test projects
    ├── 📁 UnitTests/                  # Unit tests
    ├── 📁 IntegrationTests/           # Integration tests
    └── 📁 E2ETests/                   # End-to-end tests
```

## 🚀 **REST API Conventions**

### **1. URL Structure**
```
✅ GOOD:
GET    /api/v1/students              # Get all students
GET    /api/v1/students/{id}         # Get student by ID
POST   /api/v1/students              # Create new student
PUT    /api/v1/students/{id}         # Update student
DELETE /api/v1/students/{id}         # Delete student
GET    /api/v1/students/{id}/payments # Get student's payments
POST   /api/v1/students/{id}/payments # Add payment to student

❌ BAD:
GET    /api/v1/getStudents
POST   /api/v1/createStudent
PUT    /api/v1/updateStudent
DELETE /api/v1/deleteStudent
```

### **2. HTTP Status Codes**
```csharp
// Success Responses
200 OK                    // GET, PUT, PATCH
201 Created              // POST
204 No Content           // DELETE

// Client Error Responses
400 Bad Request          // Validation errors
401 Unauthorized         // Authentication required
403 Forbidden           // Authorization failed
404 Not Found           // Resource not found
409 Conflict            // Resource conflict
422 Unprocessable Entity // Business rule violations

// Server Error Responses
500 Internal Server Error // Unexpected errors
503 Service Unavailable   // Service temporarily unavailable
```

### **3. Response Format**
```json
{
  "success": true,
  "message": "Student created successfully",
  "data": {
    "id": 1,
    "studentNumber": "S12345",
    "fullName": "John Doe",
    "program": "Computer Science"
  },
  "metadata": {
    "timestamp": "2025-07-01T12:00:00Z",
    "version": "1.0"
  },
  "errors": null
}
```

### **4. Query Parameters**
```
GET /api/v1/students?page=1&size=10&sort=fullName&order=asc
GET /api/v1/students?program=CS&isActive=true
GET /api/v1/students?search=john&fields=id,fullName,program
```

### **5. Pagination**
```json
{
  "data": [...],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

## 🔧 **Implementation Steps**

### **Step 1: Reorganize Project Structure**
1. Create new folders following Clean Architecture
2. Move existing files to appropriate layers
3. Update namespaces and references

### **Step 2: Implement Domain Layer**
1. Create pure domain entities
2. Add value objects
3. Define domain exceptions

### **Step 3: Implement Application Layer**
1. Create application services
2. Define CQRS pattern (Commands/Queries)
3. Add validators and DTOs

### **Step 4: Update Infrastructure Layer**
1. Implement repositories
2. Configure external services
3. Add data access logic

### **Step 5: Update Presentation Layer**
1. Refactor controllers
2. Implement proper REST conventions
3. Add proper error handling

### **Step 6: Update API Endpoints**
1. Follow REST naming conventions
2. Implement proper HTTP status codes
3. Add comprehensive documentation

## 📋 **Next Steps**

Would you like me to:
1. **Start with the folder restructuring**?
2. **Implement the Domain layer first**?
3. **Update the API endpoints to follow REST conventions**?
4. **Create a step-by-step migration plan**?

Let me know which approach you'd prefer, and I'll help you implement it! 