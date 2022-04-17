using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SimpleMonaco
{
    public class MainWindowViewModel : ObservableObject
    {
        private const string TITLE_BASE = "SimpleMonaco";

        private readonly IDictionary<string, string> _LanguageMap = new Dictionary<string, string>()
        {
            {".js","javascript" },
            {".ts","typescript" },
            {".xaml","xml" },
            {".cs","csharp" },
            {".py","python" },
            {".md","markdown" },
        };

        public MonacoModel Model { get; }

        private string _Title = TITLE_BASE;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _HtmlUri = string.Empty;
        public string HtmlUri
        {
            get => _HtmlUri;
            set => SetProperty(ref _HtmlUri, value);
        }

        private string _FilePath = string.Empty;
        public string FilePath
        {
            get => _FilePath;
            set
            {
                if(value != null)
                {
                    _FilePath = value;
                    OnPropertyChanged();
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        Title = TITLE_BASE;
                    }
                    else
                    {
                        Title = TITLE_BASE + $" ( {FilePath} )";
                    }
                }
            }
        }

        public MainWindowViewModel()
        {
            Model = GetModel();
            HtmlUri = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\html\index.html";
        }

        private MonacoModel GetModel()
        {
            var model = new MonacoModel();
            if (!string.IsNullOrEmpty(App.Argument.FilePath))
            {
                try
                {
                    model.Text = File.ReadAllText(App.Argument.FilePath);
                    FilePath = App.Argument.FilePath;
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage("Error", ex.Message));
                }
            }

            if (!string.IsNullOrEmpty(FilePath) && string.IsNullOrEmpty(App.Argument.Language))
            {
                var ext = Path.GetExtension(FilePath);
                if (!string.IsNullOrEmpty(ext))
                {

                    if (_LanguageMap.ContainsKey(Path.GetExtension(FilePath)))
                    {
                        model.Language = _LanguageMap[ext];
                    }
                    else
                    {
                        model.Language = ext.Substring(1);
                    }
                }
            }
            else
            {
                model.Language = App.Argument.Language;
            }
            model.RequestSave += Model_RequestSave;
            model.TextChanged += Model_TextChanged;

            return model;
        }

        private void Model_RequestSave(MonacoModel model)
        {
            Save(model, FilePath);
        }

        private void Model_TextChanged(MonacoModel obj)
        {
            if (!Title.StartsWith("*"))
            {
                Title = "*" + Title;
            }
        }

        private void Save(MonacoModel model, string saveFilePath)
        {
            if (string.IsNullOrEmpty(saveFilePath))
            {
                var result = WeakReferenceMessenger.Default.Send(new ShowCommonDialogMessage(
                    typeof(SaveFileDialog),
                    "すべて|*|テキスト|*.txt"));

                if (!result.Result)
                {
                    return;
                }
                saveFilePath = ((SaveFileDialog)result.Dialog).FileName;
            }

            try
            {
                File.WriteAllText(saveFilePath, model.Text);
                FilePath = saveFilePath;
            }
            catch (Exception ex)
            {
                var result = WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage("Error",
                    $"保存できません。別名で保存しますか？{Environment.NewLine}（ {ex.Message}）",
                    MessageBoxButton.YesNo));

                if (result.Result == MessageBoxResult.Yes)
                {
                    Save(model, string.Empty);
                }
            }
        }
    }
}
