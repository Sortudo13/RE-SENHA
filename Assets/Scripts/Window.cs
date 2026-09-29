using UnityEngine;

public class Window : MonoBehaviour
{

    public GameObject taskbarButtonPrefab; 
    private GameObject myTaskbarButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform taskbarPanel = GameObject.Find("TaskbarPanel").transform;
        myTaskbarButton = Instantiate(taskbarButtonPrefab, taskbarPanel);
        myTaskbarButton.GetComponent<TaskbarButton>().myWindow = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseWindow()
    {
        //Destroy(myTaskbarButton);
        this.gameObject.SetActive(false); 
        Destroy(myTaskbarButton);
    }

    public void OpenWindow()
    {
        this.gameObject.SetActive(true);
    }

    public void MinimizeWindow()
    {
        this.gameObject.SetActive(false);
    }
}
