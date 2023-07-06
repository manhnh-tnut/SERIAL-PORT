using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Extensions
{
    public interface INotifyPropertyChangedWithRaise : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName);
    }

    /// <summary>
    /// <see cref="http://dotnet.dzone.com/news/silverlightwpf-implementing"/>
    /// </summary>
    public static class NotifyPropertyChangedExtensions
    {
        /// <summary>
        /// Raises PropertyChanged event.
        /// To use: call the extension method with this: this.OnPropertyChanged(n => n.Title);
        /// </summary>
        /// <typeparam name="T">Property owner</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="observableBase"></param>
        /// <param name="expression">Property expression like 'n => n.Property'</param>
        public static void OnPropertyChanged<T, TProperty>(this T observableBase, Expression<Func<T, TProperty>> expression) where T : INotifyPropertyChangedWithRaise
        {
            observableBase.OnPropertyChanged(GetPropertyName(expression));
        }

        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression) where T : INotifyPropertyChanged
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
                throw new ArgumentException("Please provide a lambda expression like 'n => n.PropertyName'");

            MemberInfo memberInfo = memberExpression.Member;

            if (string.IsNullOrEmpty(memberInfo.Name))
                throw new ArgumentException("'expression' did not provide a property name.");

            return memberInfo.Name;
        }
    }
}
