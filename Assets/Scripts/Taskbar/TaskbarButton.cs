using UnityEngine;

public class TaskbarButton : MonoBehaviour
{
    [HideInInspector]
    public GameObject myWindow;

    //funcao ligado ao evento OnClick deste botao
    public void OnClickTaskbar()
    {
        //se a janela estiver visivel na tela ele esconde
        if (myWindow.activeSelf)
        {
            myWindow.SetActive(false);
        }
        //se ela estiver escondida ele mostra e traz para frente
        else
        {
            myWindow.SetActive(true);
            myWindow.transform.SetAsLastSibling();
        }
    }
}