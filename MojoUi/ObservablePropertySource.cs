using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MojoUi
{
    public class ObservablePropertySource<T> : IDisposable
    {
        private int _activated;
        private IDisposable _inner;
        private T _lastValue;
        private IConnectableObservable<T> _source;

        /// <summary>
        /// Constructs an ObservablePropertySource object.
        /// </summary>
        /// <param name="observable">The Observable to base the property on.</param>
        /// <param name="onChanged">The action to take when the property
        /// changes, typically this will call the ViewModel's
        /// RaisePropertyChanged method.</param>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="deferSubscription">
        /// A value indicating whether the <see cref="ObservablePropertySource{T}"/> 
        /// should defer the subscription to the <paramref name="observable"/> source 
        /// until the first call to <see cref="Value"/>, or if it should immediately 
        /// subscribe to the the <paramref name="observable"/> source.
        /// </param>
        /// <param name="scheduler">The scheduler that the notifications will be
        /// provided on - this should normally be a Dispatcher-based scheduler
        /// </param>
        public ObservablePropertySource(
            IObservable<T> observable,
            Action<T> onChanged,
            T initialValue = default(T),
            bool deferSubscription = false,
            IScheduler scheduler = null) : this(observable, onChanged, null, initialValue, deferSubscription, scheduler) { }

        /// <summary>
        /// Constructs an ObservablePropertySource object.
        /// </summary>
        /// <param name="observable">The Observable to base the property on.</param>
        /// <param name="onChanged">The action to take when the property
        /// changes, typically this will call the ViewModel's
        /// RaisePropertyChanged method.</param>
        /// <param name="onChanging">The action to take when the property
        /// changes, typically this will call the ViewModel's
        /// RaisePropertyChanging method.</param>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="deferSubscription">
        /// A value indicating whether the <see cref="ObservablePropertySource{T}"/> 
        /// should defer the subscription to the <paramref name="observable"/> source 
        /// until the first call to <see cref="Value"/>, or if it should immediately 
        /// subscribe to the the <paramref name="observable"/> source.
        /// </param>
        /// <param name="scheduler">The scheduler that the notifications will be
        /// provided on - this should normally be a Dispatcher-based scheduler
        /// </param>
        public ObservablePropertySource(
            IObservable<T> observable,
            Action<T> onChanged,
            Action<T> onChanging = null,
            T initialValue = default(T),
            bool deferSubscription = false,
            IScheduler scheduler = null)
        {
            Contract.Requires(observable != null);
            Contract.Requires(onChanged != null);

            scheduler = scheduler ?? CurrentThreadScheduler.Instance;
            onChanging = onChanging ?? (_ => { });

            var subj = new Subject<T>();
            var exSubject = new Subject<Exception>();

            subj.ObserveOn(scheduler)
                .Subscribe(x => {
                onChanging(x);
                _lastValue = x;
                onChanged(x);
            }, exSubject.OnNext);

            ThrownExceptions = exSubject;

            _lastValue = initialValue;
            _source = observable.StartWith(initialValue).DistinctUntilChanged().Multicast(subj);
            if (!deferSubscription)
            {
                _inner = _source.Connect();
                _activated = 1;
            }
        }

        /// <summary>
        /// Fires whenever an exception would normally terminate ReactiveUI 
        /// internal state.
        /// </summary>
        public IObservable<Exception> ThrownExceptions { get; private set; }

        /// <summary>
        /// The last provided value from the Observable. 
        /// </summary>
        public T Value
        {
            get
            {
                if (Interlocked.CompareExchange(ref _activated, 1, 0) == 0)
                {
                    _inner = _source.Connect();
                }

                return _lastValue;
            }
        }

        /// <summary>
        /// Constructs a "default" ObservablePropertySource object. This is
        /// useful for when you will initialize the OAPH later, but don't want
        /// bindings to access a null OAPH at startup.
        /// </summary>
        /// <param name="initialValue">The initial (and only) value of the property.</param>
        /// <param name="scheduler">The scheduler that the notifications will be
        /// provided on - this should normally be a Dispatcher-based scheduler
        /// </param>
        public static ObservablePropertySource<T> Default(T initialValue = default(T), IScheduler scheduler = null)
        {
            return new ObservablePropertySource<T>(Observable.Never<T>(), _ => { }, initialValue, false, scheduler);
        }

        public void Dispose()
        {
            (_inner ?? Disposable.Empty).Dispose();
            _inner = null;
        }
    }

    public static class ObservablePropertySourceMixin
    {
        static ObservablePropertySource<TRet> observableToProperty<TObj, TRet>(
        this TObj This,
        IObservable<TRet> observable,
        Expression<Func<TObj, TRet>> property,
        TRet initialValue = default(TRet),
        bool deferSubscription = false,
        IScheduler scheduler = null)
    where TObj : IMojoObject
        {
            Contract.Requires(This != null);
            Contract.Requires(observable != null);
            Contract.Requires(property != null);

            Expression expression = Reflection.Rewrite(property.Body);

            if (expression.GetParent().NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
            }

            var name = expression.GetMemberInfo().Name;
            if (expression is IndexExpression)
                name += "[]";

            var ret = new ObservablePropertySource<TRet>(observable,
                _ => This.raisePropertyChanged(name),
                initialValue, deferSubscription, scheduler);

            return ret;
        }
    }

    public static class OAPHCreationHelperMixin
    {
        static ObservablePropertySource<TRet> observableToProperty<TObj, TRet>(
                this TObj This,
                IObservable<TRet> observable,
                Expression<Func<TObj, TRet>> property,
                TRet initialValue = default(TRet),
                bool deferSubscription = false,
                IScheduler scheduler = null)
            where TObj : IMojoObject
        {
            Contract.Requires(This != null);
            Contract.Requires(observable != null);
            Contract.Requires(property != null);

            Expression expression = Reflection.Rewrite(property.Body);

            if (expression.GetParent().NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
            }

            var name = expression.GetMemberInfo().Name;
            if (expression is IndexExpression)
                name += "[]";

            var ret = new ObservablePropertySource<TRet>(observable,
                _ => This.raisePropertyChanged(name),
                _ => This.raisePropertyChanging(name),
                initialValue, deferSubscription, scheduler);

            return ret;
        }

        /// <summary>
        /// Converts an Observable to an ObservablePropertySource and
        /// automatically provides the onChanged method to raise the property
        /// changed notification.         
        /// </summary>
        /// <param name="source">The ReactiveObject that has the property</param>
        /// <param name="property">An Expression representing the property (i.e.
        /// 'x => x.SomeProperty'</param>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="deferSubscription">
        /// A value indicating whether the <see cref="ObservablePropertySource{T}"/> 
        /// should defer the subscription to the <paramref name="observable"/> source 
        /// until the first call to <see cref="Value"/>, or if it should immediately 
        /// subscribe to the the <paramref name="observable"/> source.
        /// </param>
        /// <param name="scheduler">The scheduler that the notifications will be
        /// provided on - this should normally be a Dispatcher-based scheduler
        /// </param>
        /// <returns>An initialized ObservablePropertySource; use this as the
        /// backing field for your property.</returns>
        public static ObservablePropertySource<TRet> ToProperty<TObj, TRet>(
            this IObservable<TRet> This,
            TObj source,
            Expression<Func<TObj, TRet>> property,
            TRet initialValue = default(TRet),
            bool deferSubscription = false,
            IScheduler scheduler = null)
            where TObj : IMojoObject
        {
            return source.observableToProperty(This, property, initialValue, deferSubscription, scheduler);
        }

        /// <summary>
        /// Converts an Observable to an ObservablePropertySource and
        /// automatically provides the onChanged method to raise the property
        /// changed notification.         
        /// </summary>
        /// <param name="source">The ReactiveObject that has the property</param>
        /// <param name="property">An Expression representing the property (i.e.
        /// 'x => x.SomeProperty'</param>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="deferSubscription">
        /// A value indicating whether the <see cref="ObservablePropertySource{T}"/> 
        /// should defer the subscription to the <paramref name="observable"/> source 
        /// until the first call to <see cref="Value"/>, or if it should immediately 
        /// subscribe to the the <paramref name="observable"/> source.
        /// </param>
        /// <param name="scheduler">The scheduler that the notifications will be
        /// provided on - this should normally be a Dispatcher-based scheduler
        /// </param>
        /// <returns>An initialized ObservablePropertySource; use this as the
        /// backing field for your property.</returns>
        public static ObservablePropertySource<TRet> ToProperty<TObj, TRet>(
            this IObservable<TRet> This,
            TObj source,
            Expression<Func<TObj, TRet>> property,
            out ObservablePropertySource<TRet> result,
            TRet initialValue = default(TRet),
            bool deferSubscription = false,
            IScheduler scheduler = null)
            where TObj : IMojoObject
        {
            var ret = source.observableToProperty(This, property, initialValue, deferSubscription, scheduler);

            result = ret;
            return ret;
        }
    }
}
