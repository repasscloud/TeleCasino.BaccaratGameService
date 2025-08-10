#!/bin/bash

# Runs the full bet sweep 100 times, pretty-prints each JSON,
# and prints running totals of .wager and .netGain at the end,
# plus house profit, customer loss %, and payout ratio.

betArgs=(
  tie banker player
)

# Possible wager amounts
wagerValues=(0.05 0.1 0.5 1 2 5 10 25 50)

TOTAL_WAGER=0
TOTAL_NET=0

for round in $(seq 1 100); do
  echo -e "\n====================== Round ${round} ======================\n"
  for betArg in "${betArgs[@]}"; do

    # Pick a random wager from the list
    wager=${wagerValues[$RANDOM % ${#wagerValues[@]}]}

    echo "🎯 Bet: $betArg | 💵 Wager: $wager"
    resp=$(curl -s -X POST \
      "http://170.64.191.107/baccarat/api/Baccarat/play?wager=${wager}&bet=${betArg}&gameSessionId=21")

    # Pretty print response
    echo "$resp" | jq .

    # Extract numbers (fallback to 0 if missing/null)
    w=$(echo "$resp" | jq -r '.wager // 0')
    netGain=$(echo "$resp" | jq -r '.netGain // 0')

    # Accumulate
    TOTAL_WAGER=$(echo "$TOTAL_WAGER + $w" | bc -l)
    TOTAL_NET=$(echo "$TOTAL_NET + $netGain" | bc -l)

    echo -e "\n----------------------------------------\n"
  done
done

# Final totals
HOUSE_PROFIT=$(echo "scale=2; -1 * $TOTAL_NET" | bc)
CUSTOMER_LOSS_PCT=$(echo "scale=2; (($HOUSE_PROFIT / $TOTAL_WAGER) * 100)" | bc)
PAYOUT_RATIO=$(echo "scale=2; (100 - $CUSTOMER_LOSS_PCT)" | bc)

echo -e "\n====================== Totals ======================\n"
printf "💵 Total Wager: %0.2f\n" "$TOTAL_WAGER"
printf "📈 Total NetGain: %0.2f\n" "$TOTAL_NET"
printf "🏦 House Profit: %0.2f\n" "$HOUSE_PROFIT"
printf "📉 Customer Loss %%: %0.2f%%\n" "$CUSTOMER_LOSS_PCT"
printf "💰 Payout Ratio: %0.2f%%\n" "$PAYOUT_RATIO"