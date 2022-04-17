using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleMonaco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IRecipient<ShowMessageBoxMessage>, IRecipient<ShowCommonDialogMessage>
    {
        public static readonly DependencyProperty ViewModelProperty
                                    = DependencyProperty.Register(
                                                        nameof(ViewModel),
                                                        typeof(MainWindowViewModel),
                                                        typeof(MainWindow),
                                                        new PropertyMetadata(null));

        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();

            SetWindowSetting();

            WeakReferenceMessenger.Default.RegisterAll(this);

            ViewModel = new MainWindowViewModel();
            ViewModel.Model.Loaded += async (model) =>
            {
                MonacoWebView.Visibility = Visibility.Visible;
                await Task.Delay(200);
                MonacoWebView.UpdateWindowPos();
            };
        }

        private void SetWindowSetting()
        {
            var settings = WindowSetting.Default;
            if (settings.WindowLeft > SystemParameters.VirtualScreenLeft
                 && settings.WindowLeft + settings.WindowWidth < SystemParameters.VirtualScreenWidth)
            {
                Left = settings.WindowLeft;
            }
            if (settings.WindowTop > SystemParameters.VirtualScreenTop
                 && settings.WindowTop + settings.WindowHeight < SystemParameters.VirtualScreenHeight)
            {
                Top = settings.WindowTop;
            }
            if (settings.WindowWidth < SystemParameters.WorkArea.Width)
            {
                Width = settings.WindowWidth;
            }
            if (settings.WindowHeight < SystemParameters.WorkArea.Height)
            {
                Height = settings.WindowHeight;
            }
            WindowState = settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var settings = WindowSetting.Default;
            settings.IsMaximized = WindowState == WindowState.Maximized;
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
                MonacoWebView.CoreWebView2.AddHostObjectToScript("model", ViewModel.Model);
            }
        }

        public void Receive(ShowMessageBoxMessage message)
        {
            message.Result = MessageBox.Show(message.Message, message.Title, message.Buttons, message.Icon);
        }

        public void Receive(ShowCommonDialogMessage message)
        {
            var dialog = (CommonDialog)Activator.CreateInstance(message.DialogType);
            message.Result = dialog.ShowDialog() == true;
            message.Dialog = dialog;
        }
    }

    public class ShowMessageBoxMessage
    {
        public string Title { get; }

        public string Message { get; }

        public MessageBoxButton Buttons { get; }

        public MessageBoxImage Icon { get; }

        public MessageBoxResult Result { get; set; }

        public ShowMessageBoxMessage(string title, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            Title = title;
            Message = message;
            Buttons = buttons;
            Icon = icon;
        }
    }

    public class ShowCommonDialogMessage
    {
        public Type DialogType { get; }

        public string Filter { get; }

        public bool Result { get; set; }

        public CommonDialog Dialog { get; set; }

        public ShowCommonDialogMessage(Type dialogType, string fillter)
        {
            DialogType = dialogType;
            Filter = fillter;
        }
    }
}
