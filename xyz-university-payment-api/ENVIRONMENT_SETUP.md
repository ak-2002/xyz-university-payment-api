# Environment Setup Guide

This guide explains how to set up environment variables for the XYZ University Payment API.

## Quick Setup

### Option 1: Using PowerShell Script (Recommended)
1. Open PowerShell in the project directory
2. Run: `.\setup-env.ps1`
3. Then run: `dotnet run --project xyz-university-payment-api`

### Option 2: Using Batch File
1. Open Command Prompt in the project directory
2. Run: `setup-env.bat`
3. Then run: `dotnet run --project xyz-university-payment-api`

## Manual Setup

If you prefer to set environment variables manually, you can set them in your system:

### SendGrid Configuration
- `SENDGRID_API_KEY`: Your SendGrid API key
- `SENDGRID_FROM_EMAIL`: Verified sender email address
- `SENDGRID_FROM_NAME`: Sender display name
- `SENDGRID_REPLY_TO_EMAIL`: Reply-to email address

### Database Configuration
- `DB_CONNECTION_STRING`: SQL Server connection string

### JWT Configuration
- `JWT_KEY`: Secret key for JWT tokens (minimum 32 characters)
- `JWT_ISSUER`: JWT issuer
- `JWT_AUDIENCE`: JWT audience
- `JWT_EXPIRY_HOURS`: Token expiry time in hours

### RabbitMQ Configuration
- `RABBITMQ_USERNAME`: RabbitMQ username
- `RABBITMQ_PASSWORD`: RabbitMQ password

## SendGrid Setup

To fix the "Forbidden" error with SendGrid:

1. **Verify Your Sender Email**:
   - Log into SendGrid at https://app.sendgrid.com
   - Go to Settings → Sender Authentication
   - Add and verify your sender email address

2. **Alternative: Use a Verified Email**:
   - Use an email address that's already verified in SendGrid
   - Update the `SENDGRID_FROM_EMAIL` environment variable

3. **Check API Key Permissions**:
   - Ensure your API key has "Mail Send" permissions
   - Create a new API key if needed

## Current Configuration

The scripts are configured with:
- **From Email**: `andrew.wanyonyi1@gmail.com` (should be verified in SendGrid)
- **From Name**: "XYZ University Finance Department"
- **Reply To**: `finance@xyzuniversity.edu`

## Troubleshooting

### SendGrid "Forbidden" Error
- Verify the sender email in SendGrid
- Check API key permissions
- Ensure the email domain is verified

### Duplicate Payment Error
- Use a unique payment reference
- Clear existing payments from database if needed

### Environment Variables Not Working
- Restart your terminal/IDE after setting environment variables
- Check that the variables are set correctly: `echo $env:SENDGRID_API_KEY` (PowerShell) or `echo %SENDGRID_API_KEY%` (CMD) 