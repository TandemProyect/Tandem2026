using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin.UI.Views
{
    /// <summary>
    /// Popup compacto reutilizable (selección, formulario, reloj).
    /// Misma tarjeta para convertir bloques, render, etc.
    /// </summary>
    public partial class TandemMiniPopup : Window
    {
        public bool IsCancelled { get; private set; }
        public string ChosenId { get; private set; }
        private DispatcherFrame _waitFrame;

        public TandemMiniPopup()
        {
            InitializeComponent();
        }

        public static TandemMiniPopup ShowTop(string title)
        {
            var win = new TandemMiniPopup();
            win.TitleText.Text = string.IsNullOrWhiteSpace(title) ? "Tandem" : title;
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(win)
                {
                    Owner = AcadApp.MainWindow.Handle
                };
            }
            catch
            {
            }

            PlaceTop(win);

            try { AcadApp.ShowModelessWindow(win); }
            catch { win.Show(); }

            return win;
        }

        public void ShowPrompt(string message)
        {
            ChosenId = null;
            RunOnUi(() =>
            {
                PromptText.Text = message ?? "";
                PromptPanel.Visibility = Visibility.Visible;
                ChoicePanel.Visibility = Visibility.Collapsed;
                ProgressPanel.Visibility = Visibility.Collapsed;
            });
        }

        public void ShowChoices(string message, IList<KeyValuePair<string, string>> choices)
        {
            ChosenId = null;
            RunOnUi(() =>
            {
                ChoiceText.Text = message ?? "";
                ChoiceHost.Children.Clear();
                if (choices != null)
                {
                    for (var i = 0; i < choices.Count; i++)
                    {
                        var item = choices[i];
                        var box = new CheckBox
                        {
                            Content = item.Value,
                            Tag = item.Key,
                            FontSize = 13,
                            Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(0x43, 0x3C, 0x65)),
                            Cursor = Cursors.Hand,
                            Margin = new Thickness(0, 0, 0, i == choices.Count - 1 ? 0 : 8)
                        };
                        box.Checked += OnChoiceChecked;
                        ChoiceHost.Children.Add(box);
                    }
                }
                PromptPanel.Visibility = Visibility.Collapsed;
                ChoicePanel.Visibility = Visibility.Visible;
                ProgressPanel.Visibility = Visibility.Collapsed;
            });
        }

        public string WaitForChoice()
        {
            if (IsCancelled || !string.IsNullOrWhiteSpace(ChosenId) || !IsVisible)
                return IsCancelled ? null : ChosenId;

            _waitFrame = new DispatcherFrame();
            Closed += OnClosedEndWait;
            try
            {
                try { Activate(); } catch { }
                Dispatcher.PushFrame(_waitFrame);
            }
            finally
            {
                Closed -= OnClosedEndWait;
                _waitFrame = null;
            }
            return IsCancelled ? null : ChosenId;
        }

        public void ShowProgress(string message)
        {
            RunOnUi(() =>
            {
                ProgressText.Text = message ?? "";
                PromptPanel.Visibility = Visibility.Collapsed;
                ChoicePanel.Visibility = Visibility.Collapsed;
                ProgressPanel.Visibility = Visibility.Visible;
            });
        }

        public void CloseSafe()
        {
            try
            {
                if (Dispatcher.CheckAccess())
                    Close();
                else
                    Dispatcher.BeginInvoke(new Action(Close));
            }
            catch
            {
            }
        }

        private void EndWait()
        {
            if (_waitFrame == null)
                return;
            _waitFrame.Continue = false;
        }

        private void OnClosedEndWait(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChosenId))
                IsCancelled = true;
            EndWait();
        }

        private void OnChoiceChecked(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            var id = box == null ? null : box.Tag as string;
            if (string.IsNullOrWhiteSpace(id))
                return;
            ChosenId = id;
            EndWait();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            IsCancelled = true;
            EndWait();
            CloseSafe();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;
            IsCancelled = true;
            EndWait();
            CloseSafe();
        }

        private static void PlaceTop(Window window)
        {
            try
            {
                var main = AcadApp.MainWindow;
                var origin = main.DeviceIndependentLocation;
                var size = main.DeviceIndependentSize;
                window.Left = origin.X + Math.Max(24, (size.Width - window.Width) / 2);
                window.Top = origin.Y + 96;
            }
            catch
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void RunOnUi(Action action)
        {
            if (action == null) return;
            try
            {
                if (Dispatcher.CheckAccess())
                    action();
                else
                    Dispatcher.Invoke(action);
            }
            catch
            {
            }
        }
    }
}
