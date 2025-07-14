# Data Seeding Guide for XYZ University Payment API

This guide explains how to seed the database with sample fee schedules and student balances for testing and development purposes.

## Overview

The data seeding system provides realistic sample data for:
- **Fee Schedules**: Different programs with varying fee structures
- **Student Balances**: Realistic balance scenarios for existing students
- **Payment Statuses**: Various payment states (Paid, Outstanding, Partial, Overdue)

## Automatic Seeding

The system automatically seeds data when the application starts if no seed data exists. This happens in `Program.cs` during application startup.

## Manual Seeding via API

### 1. Check Seed Data Status

```bash
GET /api/v3/dataseeding/status
```

**Response:**
```json
{
  "success": true,
  "message": "Seed data status retrieved successfully",
  "data": {
    "hasSeedData": false,
    "message": "No seed data found"
  }
}
```

### 2. Seed Fee Schedules Only

```bash
POST /api/v3/dataseeding/seed-fee-schedules
Authorization: Bearer {admin_token}
```

### 3. Seed Student Balances Only

```bash
POST /api/v3/dataseeding/seed-student-balances
Authorization: Bearer {admin_token}
```

### 4. Seed All Data

```bash
POST /api/v3/dataseeding/seed-all
Authorization: Bearer {admin_token}
```

### 5. View Sample Fee Schedule Data

```bash
GET /api/v3/dataseeding/sample-fee-schedules
```

## Sample Data Structure

### Fee Schedules

The system creates fee schedules for Summer 2025 with the following programs:

| Program | Tuition | Registration | Library | Lab | Other | Total |
|---------|---------|--------------|---------|-----|-------|-------|
| Computer Science | $4,500 | $500 | $200 | $300 | $100 | $5,600 |
| Information Technology | $4,200 | $500 | $200 | $250 | $100 | $5,250 |
| Business Administration | $4,000 | $500 | $200 | $0 | $100 | $4,800 |
| Accounting | $4,200 | $500 | $200 | $150 | $100 | $5,150 |
| Engineering | $5,000 | $500 | $200 | $400 | $150 | $6,250 |
| Sociology | $3,800 | $500 | $200 | $0 | $100 | $4,600 |

### Student Balance Scenarios

The system generates realistic balance scenarios:

- **0% Payment**: No payment received (Outstanding)
- **25% Payment**: Partial payment (Partial)
- **50% Payment**: Half payment (Partial)
- **75% Payment**: Most payment (Partial)
- **100% Payment**: Full payment (Paid)

## Programmatic Usage

### Using the DataSeedingService

```csharp
// Inject the service
private readonly IDataSeedingService _dataSeedingService;

// Check if seed data exists
var hasData = await _dataSeedingService.HasSeedDataAsync();

// Seed all data
await _dataSeedingService.SeedAllDataAsync();

// Seed specific data
await _dataSeedingService.SeedFeeSchedulesAsync();
await _dataSeedingService.SeedStudentBalancesAsync();
```

### Database Context Usage

```csharp
// Check existing data
var hasFeeSchedules = await context.FeeSchedules.AnyAsync();
var hasStudentBalances = await context.StudentBalances.AnyAsync();

// Get seeded data
var feeSchedules = await context.FeeSchedules.ToListAsync();
var studentBalances = await context.StudentBalances
    .Include(sb => sb.Student)
    .Include(sb => sb.FeeSchedule)
    .ToListAsync();
```

## Testing the Seeded Data

### 1. Check Student Balances

```bash
GET /api/v3/dashboard/student-balance-summary/{studentNumber}
```

### 2. View Outstanding Balance Report

```bash
GET /api/v3/dashboard/outstanding-balance-report
Authorization: Bearer {admin_token}
```

### 3. Test Balance Calculations

```bash
GET /api/v3/students/{studentNumber}/balance
```

## Development Workflow

1. **Initial Setup**: Run the application - data will be seeded automatically
2. **Reset Data**: Delete existing data and restart the application
3. **Manual Seeding**: Use API endpoints for specific seeding needs
4. **Testing**: Use seeded data to test balance calculations and reports

## Notes

- Seeding is idempotent - running it multiple times won't create duplicates
- Student balances are generated based on existing students in the database
- Fee schedules are matched to student programs automatically
- Cache is cleared after seeding to ensure fresh data
- All monetary values are in USD with 2 decimal places

## Troubleshooting

### Common Issues

1. **No Students Found**: Ensure students are seeded before student balances
2. **No Fee Schedules**: Fee schedules must exist before student balances
3. **Cache Issues**: Clear cache manually if data seems stale
4. **Authorization**: Admin role required for manual seeding endpoints

### Logs

Check application logs for seeding information:
```
[INF] Seeding fee schedules...
[INF] Seeded 6 fee schedules
[INF] Seeding student balances...
[INF] Seeded 8 student balances
[INF] Data seeding completed successfully
``` 