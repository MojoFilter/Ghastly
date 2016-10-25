using MojoUi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MojoUi
{
    public interface IMojoObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        void RaisePropertyChanging(PropertyChangingEventArgs args);
        void RaisePropertyChanged(PropertyChangedEventArgs args);
    }

    public class MojoObject : IMojoObject
    {
        private event PropertyChangedEventHandler _PropertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this._PropertyChanged += value; }
            remove { this._PropertyChanged -= value; }
        }


        private event PropertyChangingEventHandler _PropertyChanging;
        event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging
        {
            add { this._PropertyChanging += value; }
            remove { this._PropertyChanging -= value; }
        }

        void IMojoObject.RaisePropertyChanged(PropertyChangedEventArgs args) =>
            this._PropertyChanged?.Invoke(this, args);

        void IMojoObject.RaisePropertyChanging(PropertyChangingEventArgs args) =>
            this._PropertyChanging?.Invoke(this, args);
    }

    public static class MojoExtensions
    {
        static ConditionalWeakTable<IMojoObject, IExtensionState<IMojoObject>> state = new ConditionalWeakTable<IMojoObject, IExtensionState<IMojoObject>>();

        internal static void raisePropertyChanged<TSender>(this TSender This, string propertyName) where TSender : IMojoObject
        {
            Contract.Requires(propertyName != null);

            var s = state.GetValue(This, key => (IExtensionState<IMojoObject>)new ExtensionState<TSender>(This));

            s.raisePropertyChanged(propertyName);
        }

        internal static void raisePropertyChanging<TSender>(this TSender This, string propertyName) where TSender : IMojoObject
        {
            Contract.Requires(propertyName != null);

            var s = state.GetValue(This, key => (IExtensionState<IMojoObject>)new ExtensionState<TSender>(This));

            s.raisePropertyChanging(propertyName);
        }

        // Filter a list of change notifications, returning the last change for each PropertyName in original order.
        static IEnumerable<IReactivePropertyChangedEventArgs<TSender>> dedup<TSender>(IList<IReactivePropertyChangedEventArgs<TSender>> batch)
        {
            if (batch.Count <= 1)
            {
                return batch;
            }

            var seen = new HashSet<string>();
            var unique = new LinkedList<IReactivePropertyChangedEventArgs<TSender>>();

            for (int i = batch.Count - 1; i >= 0; i--)
            {
                if (seen.Add(batch[i].PropertyName))
                {
                    unique.AddFirst(batch[i]);
                }
            }

            return unique;
        }

        interface IExtensionState<out TSender> where TSender : IMojoObject
        {
            IObservable<IReactivePropertyChangedEventArgs<TSender>> Changing { get; }

            IObservable<IReactivePropertyChangedEventArgs<TSender>> Changed { get; }

            void raisePropertyChanging(string propertyName);

            void raisePropertyChanged(string propertyName);

            IObservable<Exception> ThrownExceptions { get; }

            bool areChangeNotificationsEnabled();

            IDisposable suppressChangeNotifications();

            bool areChangeNotificationsDelayed();

            IDisposable delayChangeNotifications();
        }

        class ExtensionState<TSender> : IExtensionState<TSender> where TSender : IMojoObject
        {
            long changeNotificationsSuppressed;
            long changeNotificationsDelayed;
            ISubject<IReactivePropertyChangedEventArgs<TSender>> changingSubject;
            IObservable<IReactivePropertyChangedEventArgs<TSender>> changingObservable;
            ISubject<IReactivePropertyChangedEventArgs<TSender>> changedSubject;
            IObservable<IReactivePropertyChangedEventArgs<TSender>> changedObservable;
            ISubject<Exception> thrownExceptions;
            ISubject<Unit> startDelayNotifications;

            TSender sender;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExtensionState{TSender}"/> class.
            /// </summary>
            public ExtensionState(TSender sender)
            {
                this.sender = sender;
                this.changingSubject = new Subject<IReactivePropertyChangedEventArgs<TSender>>();
                this.changedSubject = new Subject<IReactivePropertyChangedEventArgs<TSender>>();
                this.startDelayNotifications = new Subject<Unit>();
                this.thrownExceptions = new Subject<Exception>();

                this.changedObservable = changedSubject
                    .Buffer(
                        Observable.Merge(
                            changedSubject.Where(_ => !areChangeNotificationsDelayed()).Select(_ => Unit.Default),
                            startDelayNotifications)
                    )
                    .SelectMany(batch => dedup(batch))
                    .Publish()
                    .RefCount();

                this.changingObservable = changingSubject
                    .Buffer(
                        Observable.Merge(
                            changingSubject.Where(_ => !areChangeNotificationsDelayed()).Select(_ => Unit.Default),
                            startDelayNotifications)
                    )
                    .SelectMany(batch => dedup(batch))
                    .Publish()
                    .RefCount();
            }

            public IObservable<IReactivePropertyChangedEventArgs<TSender>> Changing
            {
                get { return this.changingObservable; }
            }

            public IObservable<IReactivePropertyChangedEventArgs<TSender>> Changed
            {
                get { return this.changedObservable; }
            }

            public IObservable<Exception> ThrownExceptions
            {
                get { return thrownExceptions; }
            }

            public bool areChangeNotificationsEnabled()
            {
                return (Interlocked.Read(ref changeNotificationsSuppressed) == 0);
            }

            public bool areChangeNotificationsDelayed()
            {
                return (Interlocked.Read(ref changeNotificationsDelayed) > 0);
            }

            /// <summary>
            /// When this method is called, an object will not fire change
            /// notifications (neither traditional nor Observable notifications)
            /// until the return value is disposed.
            /// </summary>
            /// <returns>An object that, when disposed, reenables change
            /// notifications.</returns>
            public IDisposable suppressChangeNotifications()
            {
                Interlocked.Increment(ref changeNotificationsSuppressed);
                return Disposable.Create(() => Interlocked.Decrement(ref changeNotificationsSuppressed));
            }

            public IDisposable delayChangeNotifications()
            {
                if (Interlocked.Increment(ref changeNotificationsDelayed) == 1)
                {
                    startDelayNotifications.OnNext(Unit.Default);
                }

                return Disposable.Create(() =>
                {
                    if (Interlocked.Decrement(ref changeNotificationsDelayed) == 0)
                    {
                        startDelayNotifications.OnNext(Unit.Default);
                    };
                });
            }

            public void raisePropertyChanging(string propertyName)
            {
                if (!this.areChangeNotificationsEnabled())
                    return;

                var changing = new ReactivePropertyChangingEventArgs<TSender>(sender, propertyName);
                sender.RaisePropertyChanging(changing);

                this.notifyObservable(sender, changing, this.changingSubject);
            }

            public void raisePropertyChanged(string propertyName)
            {
                if (!this.areChangeNotificationsEnabled())
                    return;

                var changed = new ReactivePropertyChangedEventArgs<TSender>(sender, propertyName);
                sender.RaisePropertyChanged(changed);

                this.notifyObservable(sender, changed, this.changedSubject);
            }

            internal void notifyObservable<T>(TSender rxObj, T item, ISubject<T> subject)
            {
                try
                {
                    subject.OnNext(item);
                }
                catch (Exception ex)
                {
                    thrownExceptions.OnNext(ex);
                }
            }
        }
    }
}
