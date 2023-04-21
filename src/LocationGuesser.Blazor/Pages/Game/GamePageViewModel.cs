using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Game;

public partial class GamePageViewModel : ViewModelBase
{
    public string SetSlug { get; set; } = string.Empty;

    [Notify] private bool _notFound;
    [Notify] private bool _isError;
    [Notify] private List<Image>? _images;
    [Notify] private int _currentIndex;
    [Notify] private Image? _currentImage;
    private readonly IGameApiService _gameSetService;

    public GamePageViewModel(IGameApiService gameSetService)
    {
        _gameSetService = gameSetService;
    }

    public override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await FetchGameSet();
    }

    private async Task FetchGameSet()
    {
        var result = await _gameSetService.GetGameSetAsync(SetSlug, 5, CancellationToken.None);
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
            CurrentImage = Images[0];
        }
    }

    public void Next()
    {
        if (Images == null) return;
        if (CurrentIndex == Images?.Count - 1) return;
        CurrentIndex++;
        CurrentImage = Images![CurrentIndex];
    }
}