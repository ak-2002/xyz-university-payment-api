# SendGrid Email Setup Guide

## Step 1: Get SendGrid Account
1. Go to [SendGrid.com](https://sendgrid.com/)
2. Click "Start for Free" 
3. Create an account (100 emails/day free)
4. Verify your email address

## Step 2: Get API Key
1. Log into SendGrid dashboard
2. Go to **Settings** → **API Keys**
3. Click **"Create API Key"**
4. Name: "XYZ University Payment API"
5. Access: **"Restricted Access"**
6. Permissions: **"Mail Send"** only
7. Click **"Create & View"**
8. **Copy the API Key immediately** (you won't see it again!)

## Step 3: Configure Application

### Option A: Development (appsettings.json)
Replace `YOUR_SENDGRID_API_KEY_HERE` in `appsettings.json`:
```json
"SendGrid": {
  "ApiKey": "SG.your-actual-api-key-here",
  "FromEmail": "noreply@xyzuniversity.edu",
  "FromName": "XYZ University Finance Department"
}
```

### Option B: Production (Environment Variables)
Create a `.env` file in project root:
```env
SENDGRID__APIKEY=SG.your-actual-api-key-here
SENDGRID__FROMEMAIL=noreply@xyzuniversity.edu
SENDGRID__FROMNAME=XYZ University Finance Department
```

## Step 4: Test Email Functionality

### Method 1: Make a Payment
1. Start the API: `dotnet run`
2. Go to frontend: `http://localhost:5173`
3. Login as admin
4. Create a payment for a student with email
5. Check logs for email sending status

### Method 2: Use Test Endpoint
```bash
curl -X POST "http://localhost:5260/api/v3/payment/test-email" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "studentEmail": "test@example.com",
    "studentName": "Test Student",
    "studentNumber": "S123456",
    "amount": 100.00
  }'
```

## Step 5: Verify Email Delivery
1. Check SendGrid dashboard → **Activity** → **Email Activity**
2. Look for your test emails
3. Check spam folder if not in inbox

## Troubleshooting

### Email Not Sending
- Verify API key is correct
- Check SendGrid account is verified
- Look at application logs for errors
- Verify from email is valid

### API Key Issues
- Ensure key has "Mail Send" permission
- Check key is not expired
- Verify key format starts with "SG."

### Rate Limits
- Free tier: 100 emails/day
- Check SendGrid dashboard for usage
- Upgrade plan if needed

## Security Notes
- Never commit API keys to version control
- Use environment variables in production
- Rotate API keys regularly
- Monitor SendGrid activity for suspicious usage 