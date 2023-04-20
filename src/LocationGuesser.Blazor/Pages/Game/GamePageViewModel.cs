using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using LocationGuesser.Core.Domain.Errors;
using Microsoft.AspNetCore.Components;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Game;

public partial class GamePageViewModel : ViewModelBase
{
    [Parameter]
    public string? SetIdString { get; set; }

    [Notify] private bool _notFound;
    [Notify] private bool _isError;
    [Notify] private Guid _setId;
    [Notify] private List<Image>? _images;
    private readonly IGameApiService _gameSetService;

    public GamePageViewModel(IGameApiService gameSetService)
    {
        _gameSetService = gameSetService;
    }

    public override async Task OnInitializedAsync()
    {
        if (ValidateSetId())
            await FetchGameSet();
    }

    private bool ValidateSetId()
    {
        if (Guid.TryParse(SetIdString, out var setId))
        {
            SetId = setId;
            return true;
        }
        NotFound = true;
        return false;
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
    }

}