#!/bin/sh
set -e

echo "Waiting for API to be ready..."
while ! curl -s http://blacklist-api:80/health > /dev/null; do
    sleep 1
done

echo "Starting performance tests..."
node test_api.js