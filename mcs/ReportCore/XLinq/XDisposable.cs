using System;
using System.Threading;

namespace Mono.XLinq
{
    // Origin: Reactive.Disposables

    // System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
    // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile7\System.Runtime.dll
    public interface IObserver<in T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }

    public interface IObservable<out T>
    {
        // Notifies the provider that an observer is to receive notifications.
        IDisposable Subscribe(IObserver<T> observer);
    }


    public interface ICancelable : IDisposable
    {
        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Provides a set of static methods for creating Disposables.
    /// </summary>
    public static class XDisposable
    {
        /// <summary>
        /// Gets the disposable that does nothing when disposed.
        /// </summary>
        public static IDisposable Empty
        {
            get { return DefaultDisposable.Instance; }
        }

        /// <summary>
        /// Creates a disposable object that invokes the specified action when disposed.
        /// </summary>
        /// <param name="dispose">Action to run during the first call to <see cref="IDisposable.Dispose"/>. The action is guaranteed to be run at most once.</param>
        /// <returns>The disposable object that runs the given action upon disposal.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dispose"/> is null.</exception>
        public static IDisposable Create(Action dispose)
        {
            if (dispose == null)
                throw new ArgumentNullException("dispose");

            return new AnonymousDisposable(dispose);
        }
    }

    class DefaultDisposable : IDisposable
    {
        /// <summary>
        /// Singleton default disposable.
        /// </summary>
        public static readonly DefaultDisposable Instance = new DefaultDisposable();

        private DefaultDisposable() { }
        public void Dispose() { }
    }

    internal sealed class AnonymousDisposable : ICancelable
    {
        private volatile Action _dispose;

        /// <summary>
        /// Constructs a new disposable with the given action used for disposal.
        /// </summary>
        /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
        public AnonymousDisposable(Action dispose)
        {
            System.Diagnostics.Debug.Assert(dispose != null);

            _dispose = dispose;
        }

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _dispose == null; }
        }
        /// <summary>
        /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
        /// </summary>
        public void Dispose()
        {
#pragma warning disable 0420
            var dispose = System.Threading.Interlocked.Exchange(ref _dispose, null);
#pragma warning restore 0420
            if (dispose != null)
            {
                dispose();
            }
        }
    }


    /// <summary>
    /// Abstract base class for implementations of the IObserver interface.
    /// </summary>
    public abstract class ObserverBase<T> : IObserver<T>, IDisposable
    {
        private int isStopped;

        protected ObserverBase()
        {
            isStopped = 0;
        }

        /// <summary>
        /// Notifies the observer of a new element in the sequence.
        /// </summary>
        /// <param name="value">Next element in the sequence.</param>
        public void OnNext(T value)
        {
            if (isStopped == 0)
                OnNextCore(value);
        }

        protected abstract void OnNextCore(T value);

        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            // System.Threading.Interlocked 
            if (Interlocked.Exchange(ref isStopped, 1) == 0)
            {
                OnErrorCore(error);
            }
        }

        protected abstract void OnErrorCore(Exception error);

        public void OnCompleted()
        {
            if (Interlocked.Exchange(ref isStopped, 1) == 0)
            {
                OnCompletedCore();
            }
        }

        protected abstract void OnCompletedCore();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                isStopped = 1;
            }
        }

        internal bool Fail(Exception error)
        {
            if (Interlocked.Exchange(ref isStopped, 1) == 0)
            {
                OnErrorCore(error);
                return true;
            }

            return false;
        }
    }
}