using UnityEngine;

public class Window : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }

    public void OpenWindow()
    {
        this.gameObject.SetActive(true);
    }
}
