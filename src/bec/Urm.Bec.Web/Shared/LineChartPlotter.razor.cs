using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Urm.Bec.Contracts;
using Urm.Bec.Models;

namespace Urm.Bec.Web.Shared
{
    public partial class LineChartPlotter : IDisposable
    {
        [Parameter] public string ChartTitle { get; set; } = "Line Chart";
        [Parameter] public string XAxisTitle { get; set; } = "Time";
        [Parameter] public string YAxisTitle { get; set; } = "Y Axis";
        [Parameter] public string LegendLabel { get; set; } = "Data Series";
        [Parameter, EditorRequired] public Func<SimulatorState, double> DataSelector { get; set; }

        [Inject] private ISimulator _sim { get; set; } = default!;

        private Chart _chart = default!;
        private List<double> _data { get; set; } = default!;
        private List<int> _time { get; set; } = default!;
        protected override void OnInitialized()
        {
            _data = new List<double>();
            _time = new List<int>();
            _time.AddRange(Enumerable.Range(0, 20));
            _sim.OnStateSnapshot += _sim_OnStateSnapshot;
        }

        private void _sim_OnStateSnapshot(object? sender, SimulatorState e)
        {
            if (_data.Count >= 20)
            {
                _data.RemoveAt(0);
                _time.RemoveAt(0);
                _time.Add(_time.Last() + 1);
            }
            var d = DataSelector(e);
            _data.Add(d);
            this.InvokeAsync(async () =>
            {
                await _chart.Reload();
            });
        }

        public void Dispose()
        {
            _sim.OnStateSnapshot -= _sim_OnStateSnapshot;
        }


        //public void UpdateState(SimulatorState state)
        //{
        //    if (_data.Count >= 20)
        //    {
        //        _data.RemoveAt(0);
        //        _time.RemoveAt(0);
        //        _time.Add(_time.Last() + 1);
        //    }
        //    var d = DataSelector(state);
        //    _data.Add(d);
        //    this.InvokeAsync(async () =>
        //    {
        //        await _chart.Reload();
        //    });
        //}

        private Task<ChartDataSource> OnChartInitAsync()
        {
            var ds = new ChartDataSource();
            ds.Options.Title = ChartTitle;
            ds.Options.LegendLabelsFontSize = 16;
            ds.Options.X.Title = XAxisTitle;
            ds.Options.Y.Title = YAxisTitle;

            ds.Labels = _time.Select(x => x.ToString());
            ds.Data.Add(new ChartDataset()
            {
                BorderWidth = 1,
                Label = LegendLabel,
                Data = _data.Select(i => (object)i),
                ShowPointStyle = false,
                PointStyle = ChartPointStyle.Circle,
                PointRadius = 1,
                PointHoverRadius = 4
            });
            return Task.FromResult(ds);
        }
    }
}
