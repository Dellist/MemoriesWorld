using UnityEngine;
using System.Collections.Generic; // ��������� ��� ������������� List

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

    public void AssignSelectedCharacter(CharacterCellController selectedCharacterCell) // �������� ������ CharacterCellController
    {
        if (currentSlot == null)
        {
            Debug.LogWarning("No slot selected for assignment.");
            return;
        }

        // *** ��������: �������� ������ Character �������� �� CharacterCellController ***
        Character character = selectedCharacterCell.GetCharacter();

        if (character == null)
        {
            Debug.LogError("Selected character is null from CharacterCellController.");
            return;
        }

        // ���������, ���� �� �������� � ��������� (���� �� ������ �� ������ ����, ���� ������)
        if (!CharacterFactory.CharacterStorage.ContainsKey(character.Id)) // *** ��������: ���������� ContainsKey ��� Character ***
        {
            Debug.LogError($"Character with ID {character.Id} not found in CharacterFactory storage.");
            return;
        }

        // ����������, � ����� ������� ��������� ���������
        TeamManager teamManager = GetTeamManagerForSlot(currentSlot);

        if (teamManager == null)
        {
            Debug.LogError("Could not determine the team for the selected slot.");
            return;
        }

        // ���������, �� �������� �� �������� ��� � ���� �������
        if (teamManager.IsCharacterAlreadyAssigned(character))
        {
            Debug.LogWarning($"Character {character.Name} is already assigned to another slot in this team!");
            return;
        }

        // ��������� ��������� � ����
        currentSlot.AssignCharacter(character);
        Debug.Log($"Character {character.Name} assigned to the slot.");

        currentSlot = null; // ���������� ��������� ����
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