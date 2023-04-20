using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Game;

public partial class GamePageViewModel : ViewModelBase
{
    public Guid SetId { get; set; }

    [Notify] private bool _notFound;
    [Notify] private bool _isError;
    [Notify] private List<Image>? _images;
    [Notify] private int _currentIndex;
    [Notify] private string? _imageUrl;
    private readonly IGameApiService _gameSetService;

    public GamePageViewModel(IGameApiService gameSetService)
    {
        _gameSetService = gameSetService;
    }

    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Console.WriteLine(SetId);
        await FetchGameSet();
    }

    private async Task FetchGameSet()
    {
        var result = await _gameSetService.GetGameSetAsync(SetId, 5, CancellationToken.None);
        if (result.IsFailed)
        {
            if (result.Errors.First() is NotFoundError)
                NotFound = true;
            else IsError = true;
            return;
        }
        Images = result.Value;
        if (Images.Count > 0)
        {
            LoadUrl(Images[0]);
        }
    }

    public void Next()
    {
        Console.WriteLine("Next");
        if (Images == null) return;
        Console.WriteLine("Next 1");
        if (CurrentIndex == Images?.Count - 1) return;
        Console.WriteLine("Next 2");
        CurrentIndex++;

        LoadUrl(Images![CurrentIndex]);
    }

    private void LoadUrl(Image image)
    {
        Console.WriteLine("Next 3");
        ImageUrl = _gameSetService.GetImageContentUrl(image.SetId, image.Number);
    }
}