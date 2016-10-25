using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MojoUi
{
    /// <summary>
    /// IObservedChange is a generic interface that is returned from WhenAny()
    /// Note that it is used for both Changing (i.e.'before change')
    /// and Changed Observables.
    /// </summary>
    public interface IObservedChange<out TSender, out TValue>
    {
        /// <summary>
        /// The object that has raised the change.
        /// </summary>
        TSender Sender { get; }

        /// <summary>
        /// The expression of the member that has changed on Sender.
        /// </summary>
        Expression Expression { get; }

        /// <summary>
        /// The value of the property that has changed. IMPORTANT NOTE: This
        /// property is often not set for performance reasons, unless you have
        /// explicitly requested an Observable for a property via a method such
        /// as ObservableForProperty. To retrieve the value for the property,
        /// use the GetValue() extension method.
        /// </summary>
        TValue Value { get; }
    }

    /// <summary>
    /// A data-only version of IObservedChange
    /// </summary>
    public class ObservedChange<TSender, TValue> : IObservedChange<TSender, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservedChange{TSender, TValue}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="expression">Expression describing the member.</param>
        /// <param name="value">The value.</param>
        public ObservedChange(TSender sender, Expression expression, TValue value = default(TValue))
        {
            this.Sender = sender;
            this.Expression = expression;
            this.Value = value;
        }

        /// <summary>
        ///
        /// </summary>
        public TSender Sender { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public TValue Value { get; private set; }
    }
}
