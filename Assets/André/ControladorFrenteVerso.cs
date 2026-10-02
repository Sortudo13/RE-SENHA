using UnityEngine;

public class ControladorFrenteVerso : MonoBehaviour
{
    // Arraste os grupos no Inspector
    [SerializeField] private GameObject painelFrente;
    [SerializeField] private GameObject painelVerso;

    // A flag que controla o estado (True = Frente, False = Verso)
    private bool mostrarFrente = true;

    void Start()
    {
        // Garante que o jogo comece mostrando o lado certo
        AtualizarVisibilidade();
    }

    // Função que o seu botão vai chamar no clique
    public void AlternarLado()
    {
        // Inverte o valor da flag
        mostrarFrente = !mostrarFrente;

        // Atualiza quem aparece e quem some
        AtualizarVisibilidade();
    }

    private void AtualizarVisibilidade()
    {
        if (mostrarFrente)
        {
            // Se a flag for TRUE: Mostra frente, esconde verso
            painelFrente.SetActive(true);
            painelVerso.SetActive(false);
        }
        else
        {
            // Se a flag for FALSE: Esconde frente, mostra verso
            painelFrente.SetActive(false);
            painelVerso.SetActive(true);
        }
    }
}
