using UnityEngine;

public class SwapController : MonoBehaviour
{
    public void Swap()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
