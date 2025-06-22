using UnityEngine;
using UnityEngine.UI;

public class ButtonSetup : MonoBehaviour
{
    public Button clearStorageButton;

    private void Start()
    {
        clearStorageButton.onClick.AddListener(() =>
        {
            CharacterFactory.ClearCharacterStorage();
            Debug.Log("Character storage cleared via button.");
        });
    }
}