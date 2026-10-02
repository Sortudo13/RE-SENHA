using UnityEngine;

public class ControladorJanela : MonoBehaviour
{
    public void AbrirJanela()
    {
        this.gameObject.SetActive(true);
    }

    public void FecharJanela()
    {
        this.gameObject.SetActive(false);
    }

    public void DeletarJanela()
    {
        Destroy(this.gameObject);
    }
}
