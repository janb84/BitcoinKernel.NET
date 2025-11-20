#!/usr/bin/env bash
# Build the handler as a self-contained executable

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$PROJECT_DIR/bin"

echo "Building kernel-bindings-test-handler..."

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Build release version
dotnet publish "$PROJECT_DIR/kernel-bindings-test-handler.csproj" \
    -c Release \
    -o "$OUTPUT_DIR" \
    --self-contained false \
    /p:PublishSingleFile=false

if [ $? -eq 0 ]; then
    echo "✅ Build successful"
    echo "Binary location: $OUTPUT_DIR/kernel-bindings-test-handler"
    echo ""
    echo "Run with: $OUTPUT_DIR/kernel-bindings-test-handler"
else
    echo "❌ Build failed"
    exit 1
fi
