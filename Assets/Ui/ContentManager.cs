using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Добавляем для использования List

public class ContentManager : MonoBehaviour
{
    public GameObject characterCellPrefab; // Префаб CharacterCell
    public Transform gridParent;          // Родительский объект для Grid Layout Group
    public GameObject characterPanel;     // Панель персонажей
    public Button deleteButton;           // Кнопка Delete

    private CharacterCellController selectedCell; // Текущая выбранная ячейка

    private void OnEnable()
    {
        CharacterCellController.OnCharacterSelected += OnCharacterSelected;
    }

    private void OnDisable()
    {
        CharacterCellController.OnCharacterSelected -= OnCharacterSelected;
    }

    void Start()
    {
        // *** КРИТИЧНОЕ ИЗМЕНЕНИЕ: Загружаем и обновляем сетку при старте ***
        CharacterFactory.LoadCharacters();
        RefreshCharacterGrid();
    }

    private void OnCharacterSelected(CharacterCellController cell)
    {
        selectedCell = cell;
        deleteButton.interactable = true;
        Debug.Log($"Selected character ID: {selectedCell.GetCharacterId()}");
    }

    // *** ИЗМЕНЕНО: Метод теперь принимает объект Character ***
    public void AddCharacterToGrid(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Character object is null!");
            return;
        }

        GameObject cell = Instantiate(characterCellPrefab, gridParent);

        CharacterCellController controller = cell.GetComponent<CharacterCellController>();
        if (controller != null)
        {
            controller.SetCharacter(character); // *** ИЗМЕНЕНО: Вызываем SetCharacter ***
        }
        else
        {
            Debug.LogError("CharacterCellController component not found on the instantiated prefab!");
        }
    }

    // *** ДОБАВЛЕНО: Метод для полного обновления сетки ***
    public void RefreshCharacterGrid()
    {
        // Удаляем все существующие ячейки
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Добавляем всех персонажей из хранилища
        foreach (var character in CharacterFactory.CharacterStorage.Values)
        {
            AddCharacterToGrid(character);
        }
    }

    public void DeleteSelectedCharacter()
    {
        if (selectedCell == null)
        {
            Debug.LogWarning("No character selected for deletion!");
            return;
        }

        string characterId = selectedCell.GetCharacterId();
        if (characterId != null && CharacterFactory.CharacterStorage.ContainsKey(characterId))
        {
            CharacterFactory.CharacterStorage.Remove(characterId);
            CharacterFactory.SaveCharacters(); // Сохраняем после удаления
        }

        Destroy(selectedCell.gameObject);
        selectedCell = null;

        deleteButton.interactable = false;

        Debug.Log($"Character with ID {characterId} deleted.");
        RefreshCharacterGrid(); // Обновляем UI после удаления
    }

    // Метод для кнопки "Generate" (если она в ContentManager)
    public void GenerateAndRefreshCharacters()
    {
        CharacterFactory.CreateRandomCharacter();
        RefreshCharacterGrid();
    }
}