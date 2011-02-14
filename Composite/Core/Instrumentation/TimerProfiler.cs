using System;


namespace Composite.Core.Instrumentation
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public abstract class TimerProfiler : IDisposable
    {
        internal TimerProfiler()
        {
        }



        /// <exclude />
        public abstract void Dispose();
    }
}
