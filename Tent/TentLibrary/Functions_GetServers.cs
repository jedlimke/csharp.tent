using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace TentLibrary
{
    public delegate void GetServersCompletedEventHandler(object sender, GetServersCompletedEventArgs e);

    [Serializable]
    public class GetServersCompletedEventArgs : AsyncCompletedEventArgs
    {
        protected string profile = null;
        protected List<string> servers = null;

        /// <summary>
        /// Profile corresponding to the retrieved Servers
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

        /// <summary>
        /// URIs of the retrieved Servers
        /// </summary>
        public List<string> Servers
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was cancelled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return servers;
            }
        }

        public GetServersCompletedEventArgs(string profile, List<string> servers, Exception e, bool cancelled, object state)
            : base(e, cancelled, state)
        {
            this.profile = profile;
            this.servers = servers;
        }
    }

    public partial class Functions
    {
        #region Synchronous Method
        /// <summary>
        /// Retrieves the Server URIs for a given profile.
        /// </summary>
        /// <param name="profile">Profile URI</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Uri pointing to a Tent server</returns>
        public static List<string> GetServers(string profile, int timeout = DEFAULT_TIMEOUT)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(profile) as HttpWebRequest;

                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = timeout;
                request.ContentType = TENT_CONTENT_TYPE;
                request.Accept = TENT_CONTENT_TYPE;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format(
                            "Server error (HTTP {0}: {1}).",
                            response.StatusCode,
                            response.StatusDescription));
                    }
                    else
                    {
                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ServerResponse));

                        object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());

                        ServerResponse jsonResponse = objResponse as ServerResponse;

                        return new List<string>(jsonResponse.ServerData.Servers);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Asynchronous Method

        protected SendOrPostCallback onGetServersCompletedDelegate;

        protected delegate void GetServersWorkerEventHandler(string profile, int timeout, AsyncOperation asyncOp);

        public event GetServersCompletedEventHandler GetServersCompletedHandler;

        // This method is invoked via the AsyncOperation object,
        // so it is guaranteed to be executed on the correct thread.
        protected void GetServersCompleted(object operationState)
        {
            GetServersCompletedEventArgs e = operationState as GetServersCompletedEventArgs;

            OnGetServersCompleted(e);
        }

        protected void OnGetServersCompleted(GetServersCompletedEventArgs e)
        {
            if (GetServersCompletedHandler != null)
            {
                GetServersCompletedHandler(this, e);
            }
        }

        /// <summary>
        /// Retrieves the Servers URI for a given profile asynchronously.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="taskId"></param>
        /// <remarks>
        /// This method starts an asynchronous command. 
        /// First, it checks the supplied task ID for uniqueness.
        /// If taskId is unique, it creates a new WorkerEventHandler 
        /// and calls its BeginInvoke method to start the calculation.
        /// </remarks>
        public virtual void GetServersAsync(
            string profile,
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
            GetServersWorkerEventHandler workerDelegate = new GetServersWorkerEventHandler(CalculateGetServersWorker);
            workerDelegate.BeginInvoke(profile, timeout, asyncOp, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="servers"></param>
        /// <param name="x"></param>
        /// <param name="cancelled"></param>
        /// <param name="asyncOp"></param>
        /// <remarks>
        /// This is the method that the underlying, free-threaded 
        /// asynchronous behavior will invoke.  This will happen on
        /// an arbitrary thread.
        /// </remarks>
        protected void GetServersCompletionMethod(string profile, List<string> servers, Exception x, bool cancelled, AsyncOperation asyncOp)
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
            // ServersRetrievedEventArgs.
            GetServersCompletedEventArgs e =
                new GetServersCompletedEventArgs(
                profile,
                servers,
                x,
                cancelled,
                asyncOp.UserSuppliedState);

            // End the task. The asyncOp object is responsible 
            // for marshaling the call.
            asyncOp.PostOperationCompleted(onGetServersCompletedDelegate, e);

            // Note that after the call to OperationCompleted, 
            // asyncOp is no longer usable, and any attempt to use it
            // will cause an exception to be thrown.
        }

        // This method performs the actual work.
        // It is executed on the worker thread.
        protected void CalculateGetServersWorker(string profile, int timeout, AsyncOperation asyncOp)
        {
            List<string> servers = null;
            Exception x = null;

            // Check that the task is still active.
            // The operation may have been cancelled before
            // the thread was scheduled.
            if (!TaskCancelled(asyncOp.UserSuppliedState))
            {
                try
                {
                    servers = GetServers(profile, timeout);
                }
                catch (Exception ex)
                {
                    x = ex;
                }
            }

            this.GetServersCompletionMethod(profile, servers, x, TaskCancelled(asyncOp.UserSuppliedState), asyncOp);
        }
    }

        #endregion
}
