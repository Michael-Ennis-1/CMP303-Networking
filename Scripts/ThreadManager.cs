using System;
using System.Collections.Generic;
using UnityEngine;
// This is *NOT* my own code, the credit goes to Tom Weiland for this specific script
// Reference:
//https://github.com/tom-weiland/tcp-udp-networking/blob/tutorial-part4/GameServer/GameServer/ThreadManager.cs

public class ThreadManager : MonoBehaviour
{
    // Makes two lists, one to store what is to be completed on the main thread, and the temporary copy of it. 
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();

    // Stores boolean information on if a function is ready to be run on the mainThread.
    private static bool actionToExecuteOnMainThread = false;

    // Updates the thread manager every update loop
    private void Update()
    {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="_action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(Action _action)
    {
        // If no action, return
        if (_action == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        // Locks the action whilst it's being added to the list, and makes sure it gets executed in the thread
        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public static void UpdateMain()
    {
        // Checks if there is a function ready to be run
        if (actionToExecuteOnMainThread)
        {
            // Clears the temporary function list
            executeCopiedOnMainThread.Clear();

            // Copies the current functions to be completed to the temporary function list, whilst locked so no information gets overwritten
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            // Executes all functions on the temporary function list
            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
}

