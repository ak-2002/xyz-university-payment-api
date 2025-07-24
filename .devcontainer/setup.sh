#!/bin/bash

echo "Setting up XYZ University Payment API development environment..."

# Install .NET dependencies
echo "Installing .NET dependencies..."
cd /workspaces/xyz-university-payment-api
dotnet restore

# Install Node.js dependencies
echo "Installing Node.js dependencies..."
cd /workspaces/xyz-university-frontend
npm install

# Create environment file if it doesn't exist
if [ ! -f /workspaces/xyz-university-payment-api/.env ]; then
    echo "Creating .env file..."
    cp /workspaces/xyz-university-payment-api/environment-variables.example /workspaces/xyz-university-payment-api/.env
fi

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
sleep 30

echo "Setup complete! You can now run:"
echo "  - Backend: cd xyz-university-payment-api && dotnet run"
echo "  - Frontend: cd xyz-university-frontend && npm run dev"
echo "  - Docker: docker-compose up" 