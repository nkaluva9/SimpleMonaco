using System;
using System.Runtime.InteropServices;

namespace SimpleMonaco
{
    [ComVisible(true)]
    public class MonacoModel
    {
        public event Action<MonacoModel>? TextChanged;

        public event Action<MonacoModel>? RequestSave;

        public event Action<MonacoModel>? Loaded;

        private string _Text = string.Empty;
        public string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    TextChanged?.Invoke(this);
                }
            }
        }

        public string Language { get; set; } = string.Empty;

        public void OnRequestSave()
        {
            RequestSave?.Invoke(this);
        }

        public void OnLoaded()
        {
            Loaded?.Invoke(this);
        }
    }
}
