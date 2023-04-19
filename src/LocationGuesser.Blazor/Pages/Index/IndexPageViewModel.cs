using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Index;

public partial class IndexPageViewModel : ViewModelBase
{
    private readonly IImageSetApiService _imageSetApiService;

    [Notify] private List<ImageSet>? _imageSets;

    [Notify] private bool _isError;

    [Notify] private bool _isLoading;

    public IndexPageViewModel(IImageSetApiService imageSetApiService)
    {
        _imageSetApiService = imageSetApiService;
    }

    public override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var result = await _imageSetApiService.ListImageSetsAsync();
        if (result.IsFailed)
            IsError = true;
        else
            ImageSets = result.Value;
        IsLoading = false;
    }
}