using LocationGuesser.Blazor.Services.Abstractions;
using MvvmBlazor;
using MvvmBlazor.ViewModel;

namespace LocationGuesser.Blazor.Pages.Index;

public partial class IndexPageViewModel : ViewModelBase
{
    [Notify]
    private bool _isLoading;

    private readonly IImageSetApiService _imageSetApiService;
    public IndexPageViewModel(IImageSetApiService imageSetApiService)
    {
        _imageSetApiService = imageSetApiService;
    }

    public override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var result = await _imageSetApiService.ListImageSetsAsync();
        IsLoading = false;
    }




}