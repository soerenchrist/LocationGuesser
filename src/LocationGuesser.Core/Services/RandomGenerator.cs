using LocationGuesser.Core.Services.Abstractions;

namespace LocationGuesser.Core.Services;

public class RandomGenerator : IRandom
{
    private readonly Random _random;
    public RandomGenerator(int seed)
    {
        _random = new Random(seed);
    }

    public RandomGenerator()
    {
        _random = new Random();
    }

    public int Next(int min, int max, HashSet<int> excludedNumbers)
    {
        var excludedNumbersInBounds = excludedNumbers.Where(x => x >= min && x < max).Count();
        var numberOfPossibleNumbers = max - min - excludedNumbersInBounds;
        if (numberOfPossibleNumbers <= 0)
        {
            throw new InvalidOperationException("No number available");
        }
        do
        {
            var number = _random.Next(min, max);
            if (!excludedNumbers.Contains(number))
            {
                return number;
            }
        } while (true);
    }
}