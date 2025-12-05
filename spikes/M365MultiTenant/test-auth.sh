#!/bin/bash

# Test script to verify multi-tenant authentication

echo "=========================================="
echo "Testing Multi-Tenant M365 Authentication"
echo "=========================================="
echo ""

CLIENT_ID="<your-marimer-client-id>"

# Test Marimer
echo "TEST 1: Marimer LLC Tenant"
echo "----------------------------------------"
export MS365_MCP_CLIENT_ID="$CLIENT_ID"
export MS365_MCP_TENANT_ID="<your-marimer-tenant-id>"

echo "Client ID: $MS365_MCP_CLIENT_ID"
echo "Tenant ID: $MS365_MCP_TENANT_ID"
echo "Expected User: rocky@marimer.llc"
echo ""
echo "Starting MCP server for Marimer..."
echo "If prompted, authenticate with rocky@marimer.llc"
echo ""

# Run in background and capture output
timeout 30s npx @softeria/ms-365-mcp-server --org-mode > /tmp/marimer-test.log 2>&1 &
MARIMER_PID=$!

sleep 5

if ps -p $MARIMER_PID > /dev/null 2>&1; then
    echo "✅ Marimer MCP server is running (PID: $MARIMER_PID)"
    kill $MARIMER_PID 2>/dev/null
else
    echo "⚠️  Server may have exited - check /tmp/marimer-test.log"
fi

echo ""
echo "Marimer log output:"
cat /tmp/marimer-test.log | head -30
echo ""
echo ""

# Test Xebia
echo "TEST 2: Xebia Tenant"
echo "----------------------------------------"
export MS365_MCP_CLIENT_ID="$CLIENT_ID"
export MS365_MCP_TENANT_ID="<your-xebia-tenant-id>"

echo "Client ID: $MS365_MCP_CLIENT_ID"
echo "Tenant ID: $MS365_MCP_TENANT_ID"
echo "Expected User: rocky.lhotka@xebia.com"
echo ""
echo "Starting MCP server for Xebia..."
echo "If prompted, authenticate with rocky.lhotka@xebia.com"
echo ""

timeout 30s npx @softeria/ms-365-mcp-server --org-mode > /tmp/xebia-test.log 2>&1 &
XEBIA_PID=$!

sleep 5

if ps -p $XEBIA_PID > /dev/null 2>&1; then
    echo "✅ Xebia MCP server is running (PID: $XEBIA_PID)"
    kill $XEBIA_PID 2>/dev/null
else
    echo "⚠️  Server may have exited - check /tmp/xebia-test.log"
fi

echo ""
echo "Xebia log output:"
cat /tmp/xebia-test.log | head -30
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
echo "   # Terminal 1 - Marimer"
echo "   MS365_MCP_CLIENT_ID=$CLIENT_ID MS365_MCP_TENANT_ID=<your-marimer-tenant-id> npx @softeria/ms-365-mcp-server --org-mode --http 3001"
echo ""
echo "   # Terminal 2 - Xebia"
echo "   MS365_MCP_CLIENT_ID=$CLIENT_ID MS365_MCP_TENANT_ID=<your-xebia-tenant-id> npx @softeria/ms-365-mcp-server --org-mode --http 3002"
echo ""
