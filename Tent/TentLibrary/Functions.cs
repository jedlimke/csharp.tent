using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace TentLibrary
{
    public partial class Functions
    {
        public const int DEFAULT_TIMEOUT = -1;
        public const string TENT_CONTENT_TYPE = "application/vnd.tent.v0+json";

        /// <summary>
        /// A collection for managing lifetimes of pending asynchronous operations.
        /// The client needs a way to track operations as they are executed and completed,
        /// and this tracking is done by requiring the client to pass a unique token, or task ID,
        /// when the client makes the call to the asynchronous method. 
        /// </summary>
        protected HybridDictionary userStateToLifetime = new HybridDictionary();

        public Functions()
        {
            #region Initialize Delegates for Database Commands
            onGetProfileCompletedDelegate = new SendOrPostCallback(GetProfileCompleted);
            onGetServersCompletedDelegate = new SendOrPostCallback(GetServersCompleted);
            #endregion
        }

        // This method cancels a pending asynchronous operation.
        public void CancelAsync(object taskId)
        {
            AsyncOperation asyncOp = userStateToLifetime[taskId] as AsyncOperation;
            if (asyncOp != null)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(taskId);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Utility method for determining if a task has been cancelled.
        /// </remarks>
        protected bool TaskCancelled(object taskId)
        {
            return (userStateToLifetime[taskId] == null);
        }
    }
}