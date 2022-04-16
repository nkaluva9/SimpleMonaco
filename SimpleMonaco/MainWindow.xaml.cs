using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleMonaco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDictionary<string, string> _LanguageMap = new Dictionary<string, string>()
        {
            {".js","javascript" },
            {".ts","typescript" },
            {".xaml","xml" },
            {".cs","csharp" },
            {".md","markdown" },
        };

        private string _FilePath = string.Empty;
        public string FilePath
        {
            get => _FilePath;
            set
            {
                _FilePath = value;
                if (string.IsNullOrEmpty(_FilePath))
                {
                    Title = $"SimpleMonaco";
                }
                else
                {
                    Title = $"SimpleMonaco ({_FilePath})";
                }
            }
        }

        private readonly MonacoModel _Model = new MonacoModel();

        public MainWindow()
        {
            InitializeComponent();
            var htmlUri = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\html\index.html";
            MonacoWebView.Source = new Uri(htmlUri);

            var path = App.Argument.FilePath;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    _Model.Text = File.ReadAllText(path);
                    FilePath = path;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            if (!string.IsNullOrEmpty(FilePath) && string.IsNullOrEmpty(App.Argument.Language))
            {
                var ext = Path.GetExtension(FilePath);
                if (!string.IsNullOrEmpty(ext))
                {

                    if (_LanguageMap.ContainsKey(Path.GetExtension(FilePath)))
                    {
                        _Model.Language = _LanguageMap[ext];
                    }
                    else
                    {
                        _Model.Language = ext.Substring(1);
                    }
                }
            }
            else
            {
                _Model.Language = App.Argument.Language;
            }
            _Model.RequestSave += (model) => Save(model, FilePath);
            _Model.TextChanged += (model) =>
            {
                if (!Title.StartsWith("*"))
                {
                    Title = "*" + Title;
                }
            };
            _Model.Loaded += async (model) =>
              {
                  MonacoWebView.Visibility = Visibility.Visible;
                  await Task.Delay(200);
                  MonacoWebView.UpdateWindowPos();
              };

           SetWindowSetting();
        }

        private void Save(MonacoModel model, string saveFilePath)
        {
            if (string.IsNullOrEmpty(saveFilePath))
            {
                var sfv = new SaveFileDialog();
                sfv.Filter = "すべて|*|テキスト|*.txt";
                if (sfv.ShowDialog() != true)
                {
                    return;
                }
                saveFilePath = sfv.FileName;
            }

            try
            {
                File.WriteAllText(saveFilePath, model.Text);
                FilePath = saveFilePath;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show($"保存できません。別名で保存しますか？{Environment.NewLine}（ {ex.Message}）",
                                    "エラー",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Save(model, string.Empty);
                }
            }
        }

        private void SetWindowSetting()
        {
            var settings = WindowSetting.Default;
            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            { 
                Left = settings.WindowLeft; 
            }
            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            { 
                Top = settings.WindowTop; 
            }
            if (settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width)
            { 
                Width = settings.WindowWidth; 
            }
            if (settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height)
            {
                Height = settings.WindowHeight; 
            }
            if (settings.WindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var settings = WindowSetting.Default;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.Save();
            base.OnClosing(e);
        }

        private void MonacoWebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (MonacoWebView.CoreWebView2 != null)
            {
                MonacoWebView.CoreWebView2.AddHostObjectToScript("model", _Model);
            }
        }
    }

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
