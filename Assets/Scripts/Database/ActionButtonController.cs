using UnityEngine;

public class ActionButtonController : MonoBehaviour
{
    public void SendAction(string comando)
    {
        ActionController.SendAction(comando);
    }
}
