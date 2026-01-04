#!/bin/bash

# Test script to verify multi-tenant authentication

echo "=========================================="
echo "Testing Multi-Tenant M365 Authentication"
echo "=========================================="
echo ""

CLIENT_ID="<your-tenant1-client-id>"

# Test Tenant1
echo "TEST 1: Tenant1 Account"
echo "----------------------------------------"
export MS365_MCP_CLIENT_ID="$CLIENT_ID"
export MS365_MCP_TENANT_ID="<your-tenant1-tenant-id>"

echo "Client ID: $MS365_MCP_CLIENT_ID"
echo "Tenant ID: $MS365_MCP_TENANT_ID"
echo "Expected User: user@example.com"
echo ""
echo "Starting MCP server for Tenant1..."
echo "If prompted, authenticate with your Tenant1 account"
echo ""

# Run in background and capture output
timeout 30s npx @softeria/ms-365-mcp-server --org-mode > /tmp/tenant1-test.log 2>&1 &
TENANT1_PID=$!

sleep 5

if ps -p $TENANT1_PID > /dev/null 2>&1; then
    echo "✅ Tenant1 MCP server is running (PID: $TENANT1_PID)"
    kill $TENANT1_PID 2>/dev/null
else
    echo "⚠️  Server may have exited - check /tmp/tenant1-test.log"
fi

echo ""
echo "Tenant1 log output:"
cat /tmp/tenant1-test.log | head -30
echo ""
echo ""

# Test Tenant2
echo "TEST 2: Tenant2 Account"
echo "----------------------------------------"
export MS365_MCP_CLIENT_ID="$CLIENT_ID"
export MS365_MCP_TENANT_ID="<your-tenant2-tenant-id>"

echo "Client ID: $MS365_MCP_CLIENT_ID"
echo "Tenant ID: $MS365_MCP_TENANT_ID"
echo "Expected User: user@example2.com"
echo ""
echo "Starting MCP server for Tenant2..."
echo "If prompted, authenticate with your Tenant2 account"
echo ""

timeout 30s npx @softeria/ms-365-mcp-server --org-mode > /tmp/tenant2-test.log 2>&1 &
TENANT2_PID=$!

sleep 5

if ps -p $TENANT2_PID > /dev/null 2>&1; then
    echo "✅ Tenant2 MCP server is running (PID: $TENANT2_PID)"
    kill $TENANT2_PID 2>/dev/null
else
    echo "⚠️  Server may have exited - check /tmp/tenant2-test.log"
fi

echo ""
echo "Tenant2 log output:"
cat /tmp/tenant2-test.log | head -30
echo ""
echo ""

echo "=========================================="
echo "Authentication Test Complete"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. If both servers started successfully, authentication worked!"
echo "2. Try running MCP Inspector to test actual tool calls:"
echo "   npx @modelcontextprotocol/inspector npx -y @softeria/ms-365-mcp-server --org-mode"
echo ""
echo "3. Or test in HTTP mode for easier testing:"
echo "   # Terminal 1 - Tenant1"
echo "   MS365_MCP_CLIENT_ID=$CLIENT_ID MS365_MCP_TENANT_ID=<your-tenant1-tenant-id> npx @softeria/ms-365-mcp-server --org-mode --http 3001"
echo ""
echo "   # Terminal 2 - Tenant2"
echo "   MS365_MCP_CLIENT_ID=$CLIENT_ID MS365_MCP_TENANT_ID=<your-tenant2-tenant-id> npx @softeria/ms-365-mcp-server --org-mode --http 3002"
echo ""
