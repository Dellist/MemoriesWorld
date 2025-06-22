using UnityEngine;

public class BattleStarter : MonoBehaviour
{
    public void StartBattle()
    {
        var playerSlots = AssignmentManager.Instance.playerTeamManager.GetCharacterSlots();
        var enemySlots = AssignmentManager.Instance.enemyTeamManager.GetCharacterSlots();

        // Проверяем, есть ли хотя бы один персонаж в обеих командах
        bool hasPlayerCharacters = playerSlots.Exists(slot => slot.GetAssignedCharacter() != null);
        bool hasEnemyCharacters = enemySlots.Exists(slot => slot.GetAssignedCharacter() != null);

        if (!hasPlayerCharacters && !hasEnemyCharacters)
        {
            Debug.LogWarning("At least one character must be present in either team to start the battle!");
            return;
        }

        BattleSetup battleSetup = Object.FindFirstObjectByType<BattleSetup>();
        if (battleSetup != null)
        {
            Debug.Log($"Starting battle with {playerSlots.Count} player slots and {enemySlots.Count} enemy slots.");
            battleSetup.SetupBattle(playerSlots, enemySlots);
        }
        else
        {
            Debug.LogError("BattleSetup not found in the scene!");
        }
    }
}