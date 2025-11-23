#!/usr/bin/env bash
# Simple test script to verify the handler responds correctly

# Example test case (you'll need to replace with actual test data)
echo "Testing kernel-bindings-test-handler..."

# Build the handler
dotnet build tools/kernel-bindings-test-handler/kernel-bindings-test-handler.csproj > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build successful"

# Test 1: Invalid method
echo '{"id":"test-1","method":"invalid.method","params":{}}' | \
    dotnet run --project tools/kernel-bindings-test-handler 2>/dev/null | \
    grep -q '"error"' && echo "✅ Test 1 passed: Invalid method returns error" || echo "❌ Test 1 failed"


echo ""
echo "Handler is ready to use with conformance test suites."
echo "Run with: dotnet run --project tools/kernel-bindings-test-handler"
