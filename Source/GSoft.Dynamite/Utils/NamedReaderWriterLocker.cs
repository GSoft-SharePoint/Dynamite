﻿// -----------------------------------------------------------------------
// <copyright file="NamedReaderWriterLocker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    
    /// <summary>
    /// The named reader writer locker.
    /// </summary>
    /// <typeparam name="T">T is the Key type</typeparam>
    public class NamedReaderWriterLocker<T>
    {
        private readonly Dictionary<T, ReaderWriterLockSlim> lockDict = new Dictionary<T, ReaderWriterLockSlim>();

        private object locker = new object();

        /// <summary>
        /// The get lock.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="ReaderWriterLockSlim"/>.
        /// </returns>
        public ReaderWriterLockSlim GetLock(T key)
        {
            lock(locker){
                if(this.lockDict.ContainsKey(key)){
                    return this.lockDict[key];
                }else{
                    var lockSlim = new ReaderWriterLockSlim();
                    this.lockDict.Add(key, lockSlim);
                    return lockSlim;
                }
            }
        }

        /// <summary>
        /// The run with read lock.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        /// The <see cref="TResult"/>.
        /// </returns>
        public TResult RunWithReadLock<TResult>(T key, Func<TResult> body)
        {
            var lockSlim = this.GetLock(key);
            try
            {
                lockSlim.EnterReadLock();
                return body();
            }
            finally
            {
                lockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// The run with read lock.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        public void RunWithReadLock(T key, Action body)
        {
            var lockSlim = this.GetLock(key);
            try
            {
                lockSlim.EnterReadLock();
                body();
            }
            finally
            {
                lockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// The run with write lock.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        /// The <see cref="TResult"/>.
        /// </returns>
        public TResult RunWithWriteLock<TResult>(T key, Func<TResult> body)
        {
            var lockSlim = this.GetLock(key);
            try
            {
                lockSlim.EnterWriteLock();
                return body();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Runs the with upgradeable read lock.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public TResult RunWithUpgradeableReadLock<TResult>(T key, Func<TResult> body)
        {
            var lockSlim = this.GetLock(key);
            try
            {
                lockSlim.EnterUpgradeableReadLock();
                return body();
            }
            finally
            {
                lockSlim.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// The run with write lock.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        public void RunWithWriteLock(T key, Action body)
        {
            var lockSlim = this.GetLock(key);
            try
            {
                lockSlim.EnterWriteLock();
                body();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// The remove lock.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public void RemoveLock(T key)
        {
            ReaderWriterLockSlim o;
            if(this.lockDict.ContainsKey(key))
                this.lockDict.Remove(key);
        }
    }
}
