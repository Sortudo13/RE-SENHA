using UnityEngine;

public class ActionButtonController : MonoBehaviour
{
    public void SendAction(string command)
    {
        ActionController.SendAction(command);
    }
}
