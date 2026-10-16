using System;
using System.Collections.Generic;

public static class ActionController
{
    private static readonly Dictionary<string, Delegate> signalRegistry = new();

    public static void SendAction(string command)
    {
        if (signalRegistry.TryGetValue(command, out Delegate del) && del is Action action)
        {
            action?.Invoke();
        }
    }

    public static void SendAction<T>(string command, T data)
    {
        if (signalRegistry.TryGetValue(command, out Delegate del) && del is Action<T> action)
            action?.Invoke(data);
    }

    public static void SyncSignal(string command, Action function)
    {
        if (function == null) return;

        if (!signalRegistry.ContainsKey(command))
            signalRegistry[command] = null;

        signalRegistry[command] = (Action)signalRegistry[command] + function;
    }

    public static void SyncSignal<T>(string command, Action<T> function)
    {
        if (function == null) return;

        if (!signalRegistry.ContainsKey(command))
            signalRegistry[command] = null;

        signalRegistry[command] = (Action<T>)signalRegistry[command] + function;
    }

    public static void UnsyncSignal(string command, Action function)
    {
        if (function == null) return;

        if (signalRegistry.ContainsKey(command))
        {
            signalRegistry[command] = (Action)signalRegistry[command] - function;

            if (signalRegistry[command] == null)
                signalRegistry.Remove(command);
        }
    }

    public static void UnsyncSignal<T>(string command, Action<T> function)
    {
        if (function == null) return;

        if (signalRegistry.ContainsKey(command))
        {
            signalRegistry[command] = (Action<T>)signalRegistry[command] - function;

            if (signalRegistry[command] == null)
                signalRegistry.Remove(command);
        }
    }
}
