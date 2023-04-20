using LocationGuesser.Core.Services;

namespace LocationGuesser.Tests.Services.Abstractions;

public class RandomGeneratorTests
{
    private readonly RandomGenerator _cut;

    public RandomGeneratorTests()
    {
        _cut = new RandomGenerator(42);
        // returns values in this order:
        // 6, 1, 1, 5, 1, 2, 7, 5, 1, 7, 2, 5, 3, 3, 2
    }

    [Fact]
    public void Next_ShouldReturnNumber_WhenNumberIsNotExcluded()
    {
        var result = _cut.Next(0, 10, new HashSet<int> { });
        result.Should().Be(6);
    }

    [Fact]
    public void Next_ShouldReturnNextNumber_WhenNumberIsExcluded()
    {
        var result = _cut.Next(0, 10, new HashSet<int> { 6 });
        result.Should().Be(1);
    }

    [Fact]
    public void Next_ShouldReturnMissingNumber_WhenAllButOneAreExcluded()
    {
        var result = _cut.Next(0, 10, new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 8, 9 });
        result.Should().Be(7);
    }

    [Fact]
    public void Next_ShouldThrowException_WhenNoNumberIsAvailable()
    {
        Action action = () => _cut.Next(0, 10, new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        action.Should().Throw<InvalidOperationException>();
    }

}