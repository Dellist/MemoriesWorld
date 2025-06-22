using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Добавляем using System.Linq для .Any() и других LINQ-методов

namespace SkillSystem
{
    // Enums, которые ранее были внутри класса Skill
    public enum TargetType // Это старое enum, вероятно, не используется. Удалить, если TargetRule используется везде.
    {
        SingleEnemy,
        RandomEnemies,
        AllEnemies,
        SingleAlly,
        RandomAllies,
        AllAllies,
        Self,
        AnySingle
    }

    public enum EffectType
    {
        PhysicalDamage,
        MagicalDamage,
        HealthHeal,
        StaminaDamage,
        StaminaRestore,
        ManaDamage,
        ManaRestore,
        StatModify,
        StatusApply,
        StatusRemove
    }

    public enum StatType
    {
        Strength,
        Endurance,
        Intelligence,
        Speed
    }

    public enum DamageType
    {
        Physical,
        Magical,
        Pure
    }

    public enum TargetRule // Это используется в SkillPresets и, вероятно, является правильным
    {
        None,
        SingleEnemy,
        MultipleEnemies,
        AllEnemies,
        SingleAlly,
        MultipleAllies,
        AllAllies,
        AllyOrSelf,
        Self,
        AnySingle,
        RandomEnemies,
        RandomAllies,
    }

    public enum CommandType
    {
        Attack,
        Heal,
        Support
    }

    [Serializable]
    public class EffectData
    {
        public EffectType Type;
        public DamageType DamageType; // Используется для урона
        public StatType StatType; // Используется для изменения характеристик
        public string StatusId; // Используется для наложения/снятия статусов
        public float Power; // Базовое значение эффекта
        public float MainTargetPower; // Сила эффекта для основной цели (для MultipleEnemies/AllEnemies)
        public float[] SecondaryPowers; // Сила эффекта для второстепенных целей (для MultipleEnemies)
        public int Duration; // Длительность статуса или модификатора
    }

    [Serializable]
    public class SkillData
    {
        public string Id;
        public string Name;
        public string Description;
        public CommandType AiCommand;
        public int AiPriority; // Приоритет для AI (чем выше, тем чаще AI будет выбирать этот навык)

        public TargetRule Targeting;
        public int TargetCount; // Количество целей (для Multiple/Random)

        public int ManaCost;
        public int StaminaCost;
        public int HealthCost; // Добавлено HealthCost

        public EffectData[] Effects;

        /// <summary>
        /// Проверяет, может ли персонаж использовать этот навык (достаточно ли ресурсов).
        /// </summary>
        public bool CanUse(Character user)
        {
            return user.CurrentMana >= ManaCost &&
                   user.CurrentStamina >= StaminaCost &&
                   user.CurrentHealth > HealthCost; // Использование HealthCost
        }

        /// <summary>
        /// Возвращает список потенциальных целей для навыка.
        /// </summary>
        public List<Character> GetValidTargets(Character user, List<Character> allCharacters, Func<Character, Team> getTeam)
        {
            List<Character> validTargets = new List<Character>();
            Team userTeam = getTeam(user);
            List<Character> livingCharacters = allCharacters.Where(c => c.IsAlive()).ToList();

            switch (Targeting)
            {
                case TargetRule.Self:
                    validTargets.Add(user);
                    break;
                case TargetRule.SingleEnemy:
                case TargetRule.MultipleEnemies:
                case TargetRule.AllEnemies:
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) != userTeam));
                    break;
                case TargetRule.SingleAlly:
                case TargetRule.MultipleAllies:
                case TargetRule.AllAllies:
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) == userTeam && c != user));
                    break;
                case TargetRule.AllyOrSelf:
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) == userTeam));
                    break;
                case TargetRule.AnySingle:
                    validTargets.AddRange(livingCharacters);
                    break;
            }

            // Для RandomEnemies / RandomAllies / MultipleEnemies / MultipleAllies
            // Мы просто возвращаем всех потенциальных целей, а BattleAIController или игрок
            // выберет конкретные цели. Количество TargetCount будет использоваться на этапе выбора.

            return validTargets;
        }

        /// <summary>
        /// Выполняет эффекты навыка на заданные цели.
        /// Этот метод должен быть в SkillInstance, так как он зависит от user.
        /// Здесь просто заглушка для совместимости, но логика должна быть в SkillInstance.Execute
        /// </summary>
        public void Execute(Character user, List<Character> targets)
        {
            // Здесь должна быть логика применения эффектов.
            // Эта логика была перемещена в SkillInstance.Execute
            Debug.LogWarning($"SkillData.Execute вызван. Эта логика должна быть в SkillInstance.Execute.");
        }
    }
}