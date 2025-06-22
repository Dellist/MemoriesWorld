using System.Collections;
using System.Collections.Generic;
using System.Linq; // Добавляем using System.Linq
using UnityEngine;
using SkillSystem; // Добавлено, чтобы SkillData и SkillInstance были доступны

// Определение Team должно быть ТОЛЬКО здесь (если оно не в другом общем файле)
public enum Team
{
    Player,
    Enemy
}

public class TurnBasedBattleSystem : MonoBehaviour
{
    [SerializeField] private float aiDecisionDelay = 1.0f;

    private List<Character> allCharacters = new List<Character>();
    private List<Character> playerCharacters = new List<Character>();
    private List<Character> enemyCharacters = new List<Character>();

    private Dictionary<Character, float> speedCounters = new Dictionary<Character, float>();

    private BattleAIController aiController;


    public void InitializeCharacters(List<Character> players, List<Character> enemies)
    {
        allCharacters.Clear();
        playerCharacters.Clear();
        enemyCharacters.Clear();
        speedCounters.Clear();

        playerCharacters.AddRange(players);
        enemyCharacters.AddRange(enemies);
        allCharacters.AddRange(players);
        allCharacters.AddRange(enemies);

        foreach (var character in allCharacters)
        {
            character.InitializeStats(); // Убеждаемся, что характеристики инициализированы
            speedCounters[character] = 0f;
        }

        Debug.Log($"Битва инициализирована. Игроков: {playerCharacters.Count}, Врагов: {enemyCharacters.Count}");
    }

    /// <summary>
    /// Запускает основной цикл битвы. Вызывается после инициализации персонажей.
    /// </summary>
    public void StartBattle()
    {
        if (allCharacters.Count == 0)
        {
            Debug.LogError("Невозможно начать битву: персонажи не инициализированы. Убедитесь, что InitializeCharacters был вызван.");
            return;
        }

        if (aiController == null)
        {
            aiController = new BattleAIController(GetTeam);
        }

        StartCoroutine(BattleLoop());
    }


    private IEnumerator BattleLoop()
    {
        while (true)
        {
            RemoveDeadCharacters(); // Проверяем мертвых в начале каждого цикла
            if (CheckBattleEnd())
            {
                Debug.Log("Битва завершена. Выход из BattleLoop.");
                yield break; // Битва завершена
            }

            Character next = GetNextTurnCharacter();

            if (next == null) // Все персонажи сделали свой ход или нет активных
            {
                Debug.Log("Нет активных персонажей для хода. Сброс счетчиков скорости и новый раунд.");
                ResetSpeedCounters(); // Если никто не может ходить, сбрасываем счетчики для нового раунда
                continue; // Начинаем новый цикл для следующего хода
            }

            Debug.Log($"Ход: {next.Name} (Скорость: {next.Speed})");

            // *** ИЗМЕНЕНИЕ ЗДЕСЬ: Обе команды теперь контролируются ИИ ***
            Debug.Log($"AI {next.Name} обдумывает ход...");
            yield return new WaitForSeconds(aiDecisionDelay); // Задержка для видимости действия AI

            // Используем aiController для получения лучшего действия для ЛЮБОГО персонажа (игрока или врага)
            (SkillInstance selectedSkillInstance, List<Character> bestTargets) = aiController.DecideAction(next, allCharacters);

            if (selectedSkillInstance != null && bestTargets != null && bestTargets.Any())
            {
                Debug.Log($"AI {next.Name} использует {selectedSkillInstance.Data.Name} на {string.Join(", ", bestTargets.Select(t => t.Name))}");
                selectedSkillInstance.Execute(bestTargets);
            }
            else
            {
                Debug.LogWarning($"AI {next.Name} не смог найти подходящее действие. Пропускает ход.");
            }
            // *** КОНЕЦ ИЗМЕНЕНИЯ ***

            // Увеличиваем счетчик хода для персонажа
            speedCounters[next] -= 100f; // Уменьшаем счетчик после хода
        }
    }

    private Character GetNextTurnCharacter()
    {
        // Увеличиваем счетчики скорости только для живых персонажей
        foreach (var character in allCharacters.Where(c => c.IsAlive()))
        {
            speedCounters[character] += character.Speed;
        }

        // Находим персонажа с наибольшим счетчиком скорости
        // Исключаем мертвых персонажей из рассмотрения
        return allCharacters
            .Where(c => c.IsAlive())
            .OrderByDescending(c => speedCounters[c])
            .FirstOrDefault();
    }

    private void ResetSpeedCounters()
    {
        foreach (var character in allCharacters)
        {
            // Сбрасываем счетчик только если он больше порога для хода (например, 100)
            // Иначе можно просто сбросить все до 0, если это начало нового "раунда"
            speedCounters[character] = 0f;
        }
        Debug.Log("Счетчики скорости сброшены.");
    }

    private void RemoveDeadCharacters()
    {
        // Создаем временный список мертвых персонажей для удаления
        var deadCharacters = allCharacters.Where(c => !c.IsAlive()).ToList();

        if (deadCharacters.Any())
        {
            Debug.Log($"Удаление мертвых персонажей: {string.Join(", ", deadCharacters.Select(c => c.Name))}");
        }

        foreach (var deadChar in deadCharacters)
        {
            if (playerCharacters.Contains(deadChar))
                playerCharacters.Remove(deadChar);

            if (enemyCharacters.Contains(deadChar))
                enemyCharacters.Remove(deadChar);

            allCharacters.Remove(deadChar);
            speedCounters.Remove(deadChar); // Удаляем из словаря скорости
        }
    }

    private bool CheckBattleEnd()
    {
        if (!playerCharacters.Any(c => c.IsAlive()))
        {
            Debug.Log("Поражение! Все игроки погибли.");
            return true;
        }

        if (!enemyCharacters.Any(c => c.IsAlive()))
        {
            Debug.Log("Победа! Все враги побеждены.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Определяет команду персонажа.
    /// </summary>
    public Team GetTeam(Character character)
    {
        if (playerCharacters.Contains(character))
            return Team.Player;
        if (enemyCharacters.Contains(character))
            return Team.Enemy;

        // Если персонаж не найден ни в одной команде, это ошибка.
        Debug.LogError($"Персонаж {character.Name} не принадлежит ни одной команде! Возвращаем Enemy по умолчанию.");
        return Team.Enemy; // Возвращаем Enemy по умолчанию, чтобы избежать null-reference
    }

}