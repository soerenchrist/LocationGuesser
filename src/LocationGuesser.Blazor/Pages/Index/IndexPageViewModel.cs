using LocationGuesser.Blazor.Services.Abstractions;
using LocationGuesser.Core.Domain;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Index;

public partial class IndexPageViewModel : ViewModelBase
{
    [Notify]
    private bool _isLoading;

    [Notify]
    private bool _isError;

    [Notify]
    private List<ImageSet>? _imageSets;

    private readonly IImageSetApiService _imageSetApiService;
    public IndexPageViewModel(IImageSetApiService imageSetApiService)
    {
        _imageSetApiService = imageSetApiService;
    }

    public override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var result = await _imageSetApiService.ListImageSetsAsync();
        if (result.IsFailed)
        {
            IsError = true;
        }
        else
        {
            ImageSets = result.Value;
        }
        IsLoading = false;
    }




}