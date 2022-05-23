// Copyright (c) 2022 Jonathan Lang
#if  DISPATCHER_DISABLE_UPDATE && DISPATCHER_DISABLE_LATEUPDATE && DISPATCHER_DISABLE_POSTUPDATE && DISPATCHER_DISABLE_FIXEDUPDATE && DISPATCHER_DISABLE_TICKUPDATE
        #define TICKFALLBACK
#endif

using System;

namespace Baracuda.Threading
{
        
    /// <summary>
    /// Enum representing the main update cycles in which a <see cref="Delegate"/>
    /// can be invoked.
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html#cycle">Documentation</a></footer>
    public enum ExecutionCycle
    {
        /// <summary>
        /// The cycle at which the <see cref="Delegate"/> is executed depends on the active modules of this asset.
        /// </summary>
        Default =
#if !DISPATCHER_DISABLE_TICKUPDATE || TICKFALLBACK
                TickUpdate,
#elif !DISPATCHER_DISABLE_UPDATE
                Update,
#elif !DISPATCHER_DISABLE_LATEUPDATE
                LateUpdate,
#elif !DISPATCHER_DISABLE_FIXEDUPDATE
                FixedUpdate,
#elif !DISPATCHER_DISABLE_EDITORUPDATE
                EditorUpdate,
#else
                TickUpdate,
#endif

#if !DISPATCHER_DISABLE_UPDATE
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next Update call.  
        /// </summary>
        Update = 1,
#endif

#if !DISPATCHER_DISABLE_LATEUPDATE
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next LateUpdate call.  
        /// </summary>
        LateUpdate = 2,
#endif

#if !DISPATCHER_DISABLE_POSTUPDATE
        /// <summary>
        /// <see cref="Delegate"/> is executed at the end of the next LateUpdate call.  
        /// </summary>
        PostUpdate = 3,
#endif

#if !DISPATCHER_DISABLE_FIXEDUPDATE
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next FixedUpdate call.  
        /// </summary>
        FixedUpdate = 4,
#endif

#if !DISPATCHER_DISABLE_TICKUPDATE || TICKFALLBACK
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next Tick call.  
        /// Tick is a custom update loop that is invoked in intervals of 50ms.
        /// This loop is also called during edit mode. 
        /// </summary>
        TickUpdate = 5,
#endif

#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next editor update call.  
        /// </summary>
        /// <footer><a href="https://docs.unity3d.com/ScriptReference/EditorApplication-update.html">Documentation</a></footer>
        EditorUpdate = 6,
#endif
    }
}