using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Urm.Bec.Contracts;
using Urm.Bec.Models;

namespace Urm.Bec.Web.Pages
{
    public partial class Home
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ISimulator _sim { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private ToastService _toast { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private IJSObjectReference? _module { get; set; }
        private System.Threading.Timer? _timer { get; set; }

        protected override void OnInitialized()
        {
            _sim.Reset();
        }

        private async Task StartAsync()
        {
            if (_module != null) return;
            var option = new BootstrapBlazor.Components.EditDialogOption<SimulatorStateSettings>()
            {
                Title = "Configure",
                Model = new SimulatorStateSettings(),
                RowType = BootstrapBlazor.Components.RowType.Normal,
                ShowLoading = true,
                ItemsPerRow = 1,
                ItemChangedType = BootstrapBlazor.Components.ItemChangedType.Update,
                OnEditAsync = async context =>
                {
                    bool e = context.Validate();
                    if (e)
                        await OnSettingsSubmit((SimulatorStateSettings)context.Model);
                    return e;
                }
            };
            await _dialog.ShowEditDialog(option);
        }

        private async Task OnSettingsSubmit(SimulatorStateSettings config)
        {
            _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/particle_sim.js");
            var obj = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("setGas", config.Gas.ToString());
            await _module.InvokeVoidAsync("setParticleCount", config.ParticleCount);
            await _module.InvokeVoidAsync("initBECSimulator", obj, "bec_sim", config.TrapShape.ToString().ToLower());
            _sim.State.TrapShape = config.TrapShape;
            _sim.State.Gas = config.Gas.ToString();
            _sim.State.ParticleCount = config.ParticleCount;
            _timer = new System.Threading.Timer(OnTick, null, 0, 1000);
            this.StateHasChanged();
        }

        private async Task StopAsync()
        {
            if (_module == null) return;
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            await _module.DisposeAsync();
            _module = null;
            _nav.NavigateTo("export");
        }

        private async void OnTick(object? state)
        {
            if (_module == null) return;
            var snap = await _module.InvokeAsync<SimulatorState>("recordObservables");
            _sim.UpdateSnaphsot(snap);
        }

        private async Task SetCoherence(double t)
        {
            if (_module == null) return;
            _sim.State.TargetCoherence = t / 100;
            await _module.InvokeVoidAsync("setCoherence", t / 100);
            this.StateHasChanged();
        }

        private bool _expansionMode { get; set; }
        private async Task ToggleExpansionMode(bool e)
        {
            if (_module == null) return;
            _expansionMode = e;
            await _module.InvokeVoidAsync("toggleExpansionMode", e);
        }

        private async Task Reheat()
        {
            if (_module == null) return;
            await _module.InvokeVoidAsync("reheat");
        }

        private async Task TakeParticleSnapshot()
        {
            if (_module == null) return;
            var snapshot = await _module.InvokeAsync<ParticleSnapshot[]>("getParticleSnapshot");
            _sim.AddParticleSnapshot(snapshot);
            await _toast.Success("Particle Snapshot", "Particle snapshot taken successfully.");
        }
    }
}
