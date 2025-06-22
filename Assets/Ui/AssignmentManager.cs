using UnityEngine;
using System.Collections.Generic; // Добавляем для использования List

public class AssignmentManager : MonoBehaviour
{
    public static AssignmentManager Instance;

    public TeamManager playerTeamManager;
    public TeamManager enemyTeamManager;

    private CharacterSlot currentSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartAssignment(CharacterSlot slot)
    {
        currentSlot = slot;
        Debug.Log("Starting assignment for a slot.");
    }

    public void AssignSelectedCharacter(CharacterCellController selectedCharacterCell) // Параметр теперь CharacterCellController
    {
        if (currentSlot == null)
        {
            Debug.LogWarning("No slot selected for assignment.");
            return;
        }

        // *** ИЗМЕНЕНО: Получаем объект Character напрямую из CharacterCellController ***
        Character character = selectedCharacterCell.GetCharacter();

        if (character == null)
        {
            Debug.LogError("Selected character is null from CharacterCellController.");
            return;
        }

        // Проверяем, есть ли персонаж в хранилище (хотя по логике он должен быть, если выбран)
        if (!CharacterFactory.CharacterStorage.ContainsKey(character.Id)) // *** ИЗМЕНЕНО: Используем ContainsKey для Character ***
        {
            Debug.LogError($"Character with ID {character.Id} not found in CharacterFactory storage.");
            return;
        }

        // Определяем, в какую команду назначить персонажа
        TeamManager teamManager = GetTeamManagerForSlot(currentSlot);

        if (teamManager == null)
        {
            Debug.LogError("Could not determine the team for the selected slot.");
            return;
        }

        // Проверяем, не назначен ли персонаж уже в этой команде
        if (teamManager.IsCharacterAlreadyAssigned(character))
        {
            Debug.LogWarning($"Character {character.Name} is already assigned to another slot in this team!");
            return;
        }

        // Назначаем персонажа в слот
        currentSlot.AssignCharacter(character);
        Debug.Log($"Character {character.Name} assigned to the slot.");

        currentSlot = null; // Сбрасываем выбранный слот
    }

    private TeamManager GetTeamManagerForSlot(CharacterSlot slot)
    {
        if (playerTeamManager.characterSlots.Contains(slot))
        {
            return playerTeamManager;
        }
        else if (enemyTeamManager.characterSlots.Contains(slot))
        {
            return enemyTeamManager;
        }
        return null;
    }
}