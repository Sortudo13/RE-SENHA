using UnityEngine;

public class DesktopIcon : MonoBehaviour
{
    public GameObject windowPrefab; 
    public Transform desktopParent;
    private GameObject currentWindowInstance; 

    public void OpenWindow()
    {
        if (currentWindowInstance == null)
        {
            //cria janela pela primeira vez
            currentWindowInstance = Instantiate(windowPrefab, desktopParent);
        }
        else
        {
            //se existe desminimiza e traz pra frente
            currentWindowInstance.SetActive(true); 
            currentWindowInstance.transform.SetAsLastSibling();
        }
    }
}