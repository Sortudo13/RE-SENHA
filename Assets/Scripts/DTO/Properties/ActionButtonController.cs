using UnityEngine;

public class ActionButtonController : MonoBehaviour
{
    public void SendAction(string command)
    {
        ActionController.SendAction(command);
    }

    public void SendAction(string command, string data)
    {
        ActionController.SendAction(command, data);
    }

    public void ResetDayAndReload()
    {
        DataController.ResetDayAndReload();
    }
}
