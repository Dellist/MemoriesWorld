using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // ��������� ��� ������������� List

public class ContentManager : MonoBehaviour
{
    public GameObject characterCellPrefab; // ������ CharacterCell
    public Transform gridParent;          // ������������ ������ ��� Grid Layout Group
    public GameObject characterPanel;     // ������ ����������
    public Button deleteButton;           // ������ Delete

    private CharacterCellController selectedCell; // ������� ��������� ������

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
        // *** ��������� ���������: ��������� � ��������� ����� ��� ������ ***
        CharacterFactory.LoadCharacters();
        RefreshCharacterGrid();
    }

    private void OnCharacterSelected(CharacterCellController cell)
    {
        selectedCell = cell;
        deleteButton.interactable = true;
        Debug.Log($"Selected character ID: {selectedCell.GetCharacterId()}");
    }

    // *** ��������: ����� ������ ��������� ������ Character ***
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
            controller.SetCharacter(character); // *** ��������: �������� SetCharacter ***
        }
        else
        {
            Debug.LogError("CharacterCellController component not found on the instantiated prefab!");
        }
    }

    // *** ���������: ����� ��� ������� ���������� ����� ***
    public void RefreshCharacterGrid()
    {
        // ������� ��� ������������ ������
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // ��������� ���� ���������� �� ���������
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
            CharacterFactory.SaveCharacters(); // ��������� ����� ��������
        }

        Destroy(selectedCell.gameObject);
        selectedCell = null;

        deleteButton.interactable = false;

        Debug.Log($"Character with ID {characterId} deleted.");
        RefreshCharacterGrid(); // ��������� UI ����� ��������
    }

    // ����� ��� ������ "Generate" (���� ��� � ContentManager)
    public void GenerateAndRefreshCharacters()
    {
        CharacterFactory.CreateRandomCharacter();
        RefreshCharacterGrid();
    }
}