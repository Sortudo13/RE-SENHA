using System;
using System.Collections.Generic;

public static class ActionController
{
    private static readonly Dictionary<string, Action> signalRegistry = new();

    public static void SendAction(string comando)
    {
        if (signalRegistry.TryGetValue(comando, out Action action))
        {
            action?.Invoke();
        }
    }

    public static void SyncSignal(string comando, Action function)
    {
        if (function == null) return;

        if (signalRegistry.ContainsKey(comando))
        {
            signalRegistry[comando] += function;
        }
        else
        {
            signalRegistry[comando] = function;
        }
    }

    public static void UnsyncSignal(string comando, Action function)
    {
        if (function == null) return;

        if (signalRegistry.ContainsKey(comando))
        {
            signalRegistry[comando] -= function;

            if (signalRegistry[comando] == null)
            {
                signalRegistry.Remove(comando);
            }
        }
    }
}
