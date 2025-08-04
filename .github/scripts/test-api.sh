#!/usr/bin/env bash
set -euo pipefail

API_URL="http://localhost:8080"
BET_TYPE="tie"

echo "🃏 Testing Baccarat API endpoint..."
curl -v -X POST \
  "$API_URL/api/Baccarat/play?wager=1&bet=$BET_TYPE&gameSessionId=23" \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json'

echo "🃏 Testing Baccarat video availability..."
ID=$(curl -s -X POST \
  "$API_URL/api/Baccarat/play?wager=1&bet=$BET_TYPE&gameSessionId=23" \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' | jq -r '.id')

if [ -z "$ID" ] || [ "$ID" == "null" ]; then
  echo "❌ No game ID returned from API"
  exit 1
fi

STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/${ID}.mp4")

if [ "$STATUS" -ne 200 ]; then
  echo "❌ Video file not available (HTTP $STATUS)"
  exit 1
fi

echo "✅ Baccarat API tests passed successfully!"
