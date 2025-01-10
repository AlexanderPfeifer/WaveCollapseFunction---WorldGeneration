using TMPro;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown generationDropdown;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Generator")]
    [SerializeField] private GameObject dungeonGenerator;
    [SerializeField] private GameObject noiseMapsWorldGenerator;
    [SerializeField] private GameObject waveCollapseFunctionWorldGenerator;

    public void StartGeneration()
    {
        switch (generationDropdown.value)
        {
            case 0 :
                dungeonGenerator.SetActive(true);
            break;
            case 1 :
                noiseMapsWorldGenerator.SetActive(true);
            break;
            case 2 :
                waveCollapseFunctionWorldGenerator.SetActive(true);
            break;
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
    }
}
