using System.ComponentModel;

namespace MojoUi
{
    /// <summary>
    /// IReactivePropertyChangedEventArgs is a generic interface that
    /// is used to wrap the NotifyPropertyChangedEventArgs and gives
    /// information about changed properties. It includes also
    /// the sender of the notification.
    /// Note that it is used for both Changing (i.e.'before change')
    /// and Changed Observables.
    /// </summary>
    public interface IReactivePropertyChangedEventArgs<out TSender>
    {
        /// <summary>
        /// The name of the property that has changed on Sender.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// The object that has raised the change.
        /// </summary>
        TSender Sender { get; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TSender"></typeparam>
    public class ReactivePropertyChangingEventArgs<TSender> : PropertyChangingEventArgs, IReactivePropertyChangedEventArgs<TSender>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactivePropertyChangingEventArgs{TSender}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Name of the property.</param>
        public ReactivePropertyChangingEventArgs(TSender sender, string propertyName)
            : base(propertyName)
        {
            this.Sender = sender;
        }

        /// <summary>
        ///
        /// </summary>
        public TSender Sender { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TSender"></typeparam>
    public class ReactivePropertyChangedEventArgs<TSender> : PropertyChangedEventArgs, IReactivePropertyChangedEventArgs<TSender>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactivePropertyChangedEventArgs{TSender}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="propertyName">Name of the property.</param>
        public ReactivePropertyChangedEventArgs(TSender sender, string propertyName)
            : base(propertyName)
        {
            this.Sender = sender;
        }

        /// <summary>
        ///
        /// </summary>
        public TSender Sender { get; private set; }
    }
}