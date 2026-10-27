#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

public class ChangeAllFontsEditor : EditorWindow
{
    private TMP_FontAsset selectedFont;

    [MenuItem("Tools/ReSenha - Trocador de Fontes")]
    public static void ShowWindow()
    {
        GetWindow<ChangeAllFontsEditor>("Trocador de Fontes");
    }

    private void OnGUI()
    {
        GUILayout.Label("Substituir Fonte de Todos os Textos da Cena", EditorStyles.boldLabel);

        selectedFont = (TMP_FontAsset)
            EditorGUILayout.ObjectField(
                "Nova Fonte (SDF):",
                selectedFont,
                typeof(TMP_FontAsset),
                false
            );

        if (GUILayout.Button("Aplicar em Todos os Textos da Cena Atual"))
            ApplyFontToScene();
    }

    private void ApplyFontToScene()
    {
        if (selectedFont == null)
        {
            Debug.LogError("Por favor, selecione um Font Asset antes de aplicar!");
            return;
        }

        int changedCount = 0;
        foreach (TMP_Text text in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (!text.gameObject.scene.isLoaded)
                continue;

            Undo.RecordObject(text, "Trocar Fonte TMP");
            text.font = selectedFont;
            EditorUtility.SetDirty(text);
            changedCount++;
        }

        Debug.Log($"Sucesso! A fonte foi atualizada em {changedCount} objetos de texto da cena.");
    }
}
#endif
