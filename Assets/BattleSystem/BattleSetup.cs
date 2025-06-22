using System.Collections.Generic;
using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    public TeamManager playerTeamManager;
    public TeamManager enemyTeamManager;

    public void SetupBattle(List<CharacterSlot> playerSlots, List<CharacterSlot> enemySlots)
    {
        var playerCharacters = new List<Character>();
        var enemyCharacters = new List<Character>();

        // Извлекаем персонажей из слотов
        foreach (var slot in playerSlots)
        {
            var character = slot.GetAssignedCharacter();
            if (character != null)
            {
                character.InitializeStats(); // Начальные значения
                playerCharacters.Add(character);
            }
        }

        foreach (var slot in enemySlots)
        {
            var character = slot.GetAssignedCharacter();
            if (character != null)
            {
                character.InitializeStats(); // Начальные значения
                enemyCharacters.Add(character);
            }
        }

        if (playerCharacters.Count == 0 && enemyCharacters.Count == 0)
        {
            Debug.LogError("Нужно хотя бы по одному персонажу в команде для начала боя!");
            return;
        }

        // Получаем ссылку на боевую систему
        var battleSystem = Object.FindFirstObjectByType<TurnBasedBattleSystem>();
        if (battleSystem != null)
        {
            // Передаём только списки персонажей — командная логика будет внутри battleSystem
            battleSystem.InitializeCharacters(playerCharacters, enemyCharacters);
            battleSystem.StartBattle(); // Запускаем битву после инициализации
        }
        else
        {
            Debug.LogError("Не найдена система боя (TurnBasedBattleSystem) на сцене!");
        }
    }
}