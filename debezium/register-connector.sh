#!/bin/bash
echo "Waiting for Debezium to be ready..."

until curl -s http://localhost:8083/connectors > /dev/null 2>&1; do
    sleep 5
done

echo "Debezium ready. Checking connector..."

STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8083/connectors/pokertrack-sessions-connector)

if [ "$STATUS" = "200" ]; then
    echo "Connector already registered, skipping."
else
    echo "Registering connector..."
    curl -X POST http://localhost:8083/connectors \
        -H "Content-Type: application/json" \
        -d @/connector/connector.json
    echo "Connector registered."
fi