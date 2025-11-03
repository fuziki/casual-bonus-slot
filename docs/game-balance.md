# Game Balance

## Initial Settings

- **Starting Credits**: 100 coins
- **Bet Amount**: 1 coin (fixed, automatic)

## Winning Odds

| Symbol | Probability | Payout |
|--------|-------------|--------|
| Bonus (777) | 5% | 100 coins |
| Bell | 45% | 2 coins |
| Cherry | 30% | 1 coin |
| Miss | 20% | 0 coins |

## Random Number Generation

- **Range**: 0-65535 (16-bit)
- **Timing**: On lever pull (reel spin start)
- **Implementation**: Unity's `Random.Range()`

### Probability Distribution

| Result | Random Value Range |
|--------|--------------------|
| Bonus | 0-3276 |
| Bell | 3277-32767 |
| Cherry | 32768-52428 |
| Miss | 52429-65535 |

## Cut-in Effects

### Bonus Win
- **Hot Cut-in**: 80% probability
- **Normal Cut-in**: 20% probability

### Miss (No Win)
- **Normal Cut-in**: 10% probability
- **No Cut-in**: 90% probability

### Small Wins (Bell, Cherry)
- **No Cut-in**: 100%
