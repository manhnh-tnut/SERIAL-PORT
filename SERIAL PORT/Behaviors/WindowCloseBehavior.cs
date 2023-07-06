using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Behaviors
{
    public class WindowCloseBehavior : Behavior<Control>
    {
        #region DependencyProperties
        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandPropery);
            set => SetValue(ClickCommandPropery, value);
        }

        private static readonly DependencyProperty ClickCommandPropery =
        DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(WindowCloseBehavior));

        private bool Close
        {
            get { return (bool)GetValue(CloseTriggerProperty); }
            set { SetValue(CloseTriggerProperty, value); }
        }

        private static readonly DependencyProperty CloseTriggerProperty =
            DependencyProperty.Register(nameof(Close), typeof(bool), typeof(WindowCloseBehavior),
                new PropertyMetadata(false, OnCloseTriggerChanged));
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }
        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
        }
        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
        }
        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close = true;
            ClickCommand?.Execute(null);
        }
        private static void OnCloseTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WindowCloseBehavior behavior)
            {
                behavior.OnCloseTriggerChanged();
            }
        }
        private void OnCloseTriggerChanged()
        {
            // when closetrigger is true, close the window
            if (Close)
            {
                foreach (var item in Application.Current.Windows)
                {
                    if (item.GetType().Name == typeof(SERIAL_PORT.Views.ErrorWindow).Name)
                    {
                        ((SERIAL_PORT.Views.ErrorWindow)item).Close();
                    }
                    else if (item.GetType().Name == typeof(SERIAL_PORT.Views.MainWindow).Name)
                    {
                        ((SERIAL_PORT.Views.MainWindow)item).Show();
                    }
                }
            }
        }
    }
}
