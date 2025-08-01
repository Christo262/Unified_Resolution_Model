using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Urm.DimE.Pages
{
    public partial class Home
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            await _js.InvokeVoidAsync("initOverviewAnim", "urm-visual-container");
        }
    }
}
