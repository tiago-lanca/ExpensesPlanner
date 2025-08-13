using Microsoft.JSInterop;
using System.Text.Json;

namespace ExpensesPlanner.Client.Utilities
{
    public class DialogSettingsService
    {
        private readonly IJSRuntime _jsRuntime;
        private DialogSettings _settings = new DialogSettings();

        public DialogSettingsService(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public DialogSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    _ = SaveStateAsync();
                }
            }
        }

        public async Task LoadStateAsync()
        {
            await Task.CompletedTask;

            var result = await _jsRuntime.InvokeAsync<string>("window.localStorage.getItem", "DialogSettings");
            if (!string.IsNullOrEmpty(result))
            {
                _settings = JsonSerializer.Deserialize<DialogSettings>(result)!;
            }
        }

        public async Task SaveStateAsync()
        {
            await Task.CompletedTask;

            await _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", "DialogSettings", JsonSerializer.Serialize<DialogSettings>(Settings));
        }

        void OnDrag(System.Drawing.Point point)
        {
            _jsRuntime.InvokeVoidAsync("eval", $"console.log('Dialog drag. Left:{point.X}, Top:{point.Y}')");

            if (Settings == null)
            {
                Settings = new DialogSettings();
            }

            Settings.Left = $"{point.X}px";
            Settings.Top = $"{point.Y}px";

            _ = SaveStateAsync();
        }

        void OnResize(System.Drawing.Size size)
        {
            _jsRuntime.InvokeVoidAsync("eval", $"console.log('Dialog resize. Width:{size.Width}, Height:{size.Height}')");

            if (Settings == null)
            {
                Settings = new DialogSettings();
            }

            Settings.Width = $"{size.Width}px";
            Settings.Height = $"{size.Height}px";

            _ = SaveStateAsync();
        }

        public class DialogSettings
        {
            public string Left { get; set; } = string.Empty;
            public string Top { get; set; } = string.Empty;
            public string Width { get; set; } = string.Empty;
            public string Height { get; set; } = string.Empty;
        }
    }
}
