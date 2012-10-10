using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace TentLibrary
{
    public delegate void GetProfileCompletedEventHandler(object sender, GetProfileCompletedEventArgs e);

    [Serializable]
    public class GetProfileCompletedEventArgs : AsyncCompletedEventArgs
    {
        protected string entity = null;
        protected string profile = null;

        /// <summary>
        /// Entity corresponding to the retrieved profile
        /// </summary>
        public string Entity
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was cancelled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return entity;
            }
        }

        /// <summary>
        /// URI of the retrieved profile
        /// </summary>
        public string Profile
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was cancelled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return profile;
            }
        }

        public GetProfileCompletedEventArgs(string entity, string profile, Exception e, bool cancelled, object state)
            : base(e, cancelled, state)
        {
            this.entity = entity;
            this.profile = profile;
        }
    }

    public partial class Functions
    {
        #region Synchronous Method
        /// <summary>
        /// Retrieves the Profile URI for a given entity.
        /// </summary>
        /// <param name="entity">Entity (user) URI</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Uri pointing to a Tent profile (e.g. https://tent.johnsmith.me/profile )</returns>
        public static string GetProfile(string entity, int timeout = DEFAULT_TIMEOUT)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(entity) as HttpWebRequest;

                request.Method = WebRequestMethods.Http.Head;
                request.Timeout = timeout;

                using (WebResponse response = request.GetResponse())
                {
                    string linkHeader = response.Headers.GetValues("Link")[0];

                    return Regex.Match(linkHeader, "[^<](.*?)(?=>)").Value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Asynchronous Method

        protected SendOrPostCallback onGetProfileCompletedDelegate;

        protected delegate void GetProfileWorkerEventHandler(string entity, int timeout, AsyncOperation asyncOp);

        public event GetProfileCompletedEventHandler GetProfileCompletedHandler;

        // This method is invoked via the AsyncOperation object,
        // so it is guaranteed to be executed on the correct thread.
        protected void GetProfileCompleted(object operationState)
        {
            GetProfileCompletedEventArgs e = operationState as GetProfileCompletedEventArgs;

            OnGetProfileCompleted(e);
        }

        protected void OnGetProfileCompleted(GetProfileCompletedEventArgs e)
        {
            if (GetProfileCompletedHandler != null)
            {
                GetProfileCompletedHandler(this, e);
            }
        }

        /// <summary>
        /// Retrieves the Profile URI for a given entity asynchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="taskId"></param>
        /// <remarks>
        /// This method starts an asynchronous command. 
        /// First, it checks the supplied task ID for uniqueness.
        /// If taskId is unique, it creates a new WorkerEventHandler 
        /// and calls its BeginInvoke method to start the calculation.
        /// </remarks>
        public virtual void GetProfileAsync(
            string entity,
            int timeout = DEFAULT_TIMEOUT,
            object taskId = null)
        {
            if (taskId == null)
            {
                taskId = System.Guid.NewGuid();
            }

            // Create an AsyncOperation for taskId.
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(taskId);

            // Multiple threads will access the task dictionary,
            // so it must be locked to serialize access.
            lock (userStateToLifetime.SyncRoot)
            {
                if (userStateToLifetime.Contains(taskId))
                {
                    throw new ArgumentException(
                        "Task ID parameter must be unique",
                        "taskId");
                }

                userStateToLifetime[taskId] = asyncOp;
            }

            // Start the asynchronous operation.
            GetProfileWorkerEventHandler workerDelegate = new GetProfileWorkerEventHandler(CalculateGetProfileWorker);
            workerDelegate.BeginInvoke(entity, timeout, asyncOp, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="result"></param>
        /// <param name="x"></param>
        /// <param name="cancelled"></param>
        /// <param name="asyncOp"></param>
        /// <remarks>
        /// This is the method that the underlying, free-threaded 
        /// asynchronous behavior will invoke.  This will happen on
        /// an arbitrary thread.
        /// </remarks>
        protected void GetProfileCompletionMethod(string entity, string profile, Exception x, bool cancelled, AsyncOperation asyncOp)
        {
            // If the task was not previously cancelled,
            // remove the task from the lifetime collection.
            if (!cancelled)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(asyncOp.UserSuppliedState);
                }
            }

            // Package the results of the operation in a 
            // ProfileRetrievedEventArgs.
            GetProfileCompletedEventArgs e =
                new GetProfileCompletedEventArgs(
                entity,
                profile,
                x,
                cancelled,
                asyncOp.UserSuppliedState);

            // End the task. The asyncOp object is responsible 
            // for marshaling the call.
            asyncOp.PostOperationCompleted(onGetProfileCompletedDelegate, e);

            // Note that after the call to OperationCompleted, 
            // asyncOp is no longer usable, and any attempt to use it
            // will cause an exception to be thrown.
        }

        // This method performs the actual work.
        // It is executed on the worker thread.
        protected void CalculateGetProfileWorker(string entity, int timeout, AsyncOperation asyncOp)
        {
            string profile = null;
            Exception x = null;

            // Check that the task is still active.
            // The operation may have been cancelled before
            // the thread was scheduled.
            if (!TaskCancelled(asyncOp.UserSuppliedState))
            {
                try
                {
                    profile = GetProfile(entity, timeout);
                }
                catch (Exception ex)
                {
                    x = ex;
                }
            }

            this.GetProfileCompletionMethod(entity, profile, x, TaskCancelled(asyncOp.UserSuppliedState), asyncOp);
        }

        #endregion
    }
}
