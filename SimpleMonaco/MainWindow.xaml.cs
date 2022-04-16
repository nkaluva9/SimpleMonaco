using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace SimpleMonaco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            var htmlUri = Directory.GetCurrentDirectory() + @"\html\index.html";
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
            _Model.Language = App.Argument.Language;
            _Model.RequestSave += (model) => Save(model, FilePath);
            _Model.TextChanged += (model) =>
            {
                if (!Title.StartsWith("*"))
                {
                    Title = "*" + Title;
                }
            };
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

        private string _Text = string.Empty;
        public string Text
        { 
            get => _Text;
            set
            {
                if(_Text != value)
                {
                    _Text = value;
                    TextChanged?.Invoke(this);
                }
            }
        }

        public string Language { get; set; } = "";

        public void OnRequestSave()
        {
            RequestSave?.Invoke(this);
        }
    }
}
