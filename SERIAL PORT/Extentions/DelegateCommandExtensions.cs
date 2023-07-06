using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace Extensions
{
    public static class DelegateCommandExtensions
    {
        private class ThreadTools
        {
            public static void RunInDispatcher(Dispatcher dispatcher, Action action)
            {
                RunInDispatcher(dispatcher, DispatcherPriority.Normal, action);
            }

            public static void RunInDispatcher(Dispatcher dispatcher, DispatcherPriority priority, Action action)
            {
                if (action == null) { return; }

                if (dispatcher.CheckAccess())
                {
                    // we are already on thread associated with the dispatcher -> just call action
                    try
                    {
                        action();
                    }
                    catch
                    {
                        //Log error here!
                    }
                }
                else
                {
                    // we are on different thread, invoke action on dispatcher's thread
                    dispatcher.BeginInvoke(
                        priority,
                        (Action)(
                        () =>
                        {
                            try
                            {
                                action();
                            }
                            catch
                            {
                                //Log error here!
                            }
                        })
                    );
                }
            }
        }

        /// <summary>
        /// Makes DelegateCommnand listen on PropertyChanged events of some object,
        /// so that DelegateCommnand can update its IsEnabled property.
        /// </summary>
        public static BaseViewModel.DelegateCommand<T> ListenOn<T, ObservedType, PropertyType>
            (this BaseViewModel.DelegateCommand<T> delegateCommand,
            ObservedType observedObject,
            Expression<Func<ObservedType, PropertyType>> propertyExpression,
            Dispatcher dispatcher)
            where ObservedType : INotifyPropertyChanged
        {
            //string propertyName = observedObject.GetPropertyName(propertyExpression);
            string propertyName = NotifyPropertyChangedExtensions.GetPropertyName(propertyExpression);

            observedObject.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    if (dispatcher != null)
                    {
                        ThreadTools.RunInDispatcher(dispatcher, delegateCommand.RaiseCanExecuteChanged);
                    }
                    else
                    {
                        delegateCommand.RaiseCanExecuteChanged();
                    }
                }
            };

            return delegateCommand; //chain calling
        }
    }
}
