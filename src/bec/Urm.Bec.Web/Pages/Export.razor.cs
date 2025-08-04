using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Urm.Bec.Contracts;

namespace Urm.Bec.Web.Pages
{
    public partial class Export
    {
        [Inject] private ISimulator _sim { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private IJSRuntime _js { get; set; } = default!;

        protected override void OnAfterRender(bool firstRender)
        {
            if(!firstRender)
                return;
            if(_sim.History.Count == 0)
            {
                Restart();
                return;
            }
        }

        private void Restart()
        {
            _nav.NavigateTo("", true);
        }

        private async Task DownloadJson()
        {
            var json = JsonSerializer.Serialize(_sim.History);
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            var filename = $"bec_snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            await _js.InvokeVoidAsync("downloadJsonFile", filename, base64);
        }
    }
}
