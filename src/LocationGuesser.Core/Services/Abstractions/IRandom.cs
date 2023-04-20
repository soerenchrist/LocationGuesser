namespace LocationGuesser.Core.Services.Abstractions;
public interface IRandom
{
    int Next(int min, int max, HashSet<int> excludedNumbers);
}