using System.Collections.Generic;
using UnityEngine;

public static class SkillPresets
{
    private static Dictionary<string, SkillSystem.SkillData> skillLibrary;

    static SkillPresets()
    {
        skillLibrary = new Dictionary<string, SkillSystem.SkillData>();

        Register(BasicAttack());
        Register(Fireball());
        Register(Heal());
    }

    private static void Register(SkillSystem.SkillData skill)
    {
        if (skill == null || string.IsNullOrEmpty(skill.Id))
        {
            Debug.LogWarning("Попытка зарегистрировать некорректный навык (SkillSystem.SkillData)");
            return;
        }

        if (!skillLibrary.ContainsKey(skill.Id))
        {
            skillLibrary[skill.Id] = skill;
        }
        else
        {
            Debug.LogWarning($"Навык с ID {skill.Id} уже зарегистрирован.");
        }
    }

    public static SkillSystem.SkillData GetSkillById(string id)
    {
        if (skillLibrary.TryGetValue(id, out var skill))
        {
            return skill;
        }

        Debug.LogError($"[SkillPresets] Навык с ID '{id}' не найден.");
        return null;
    }

    // 🔽 Навыки: генерация и регистрация
    public static SkillSystem.SkillData BasicAttack()
    {
        return new SkillSystem.SkillData
        {
            Id = "basicAttack",
            Name = "Basic Attack",
            Description = "A simple physical attack.",
            AiCommand = SkillSystem.CommandType.Attack,
            AiPriority = 50,
            Targeting = SkillSystem.TargetRule.SingleEnemy,
            TargetCount = 1,
            ManaCost = 0,
            StaminaCost = 2, // Добавлено
            HealthCost = 0,  // Добавлено
            Effects = new[]
            {
                new SkillSystem.EffectData
                {
                    Type = SkillSystem.EffectType.PhysicalDamage,
                    DamageType = SkillSystem.DamageType.Physical,
                    Power = 100 // Базовая сила для расчета урона
                }
            }
        };
    }

    public static SkillSystem.SkillData Fireball()
    {
        return new SkillSystem.SkillData
        {
            Id = "fireball",
            Name = "Fireball",
            Description = "Deals magic damage to main target and reduced damage to others",
            AiCommand = SkillSystem.CommandType.Attack,
            AiPriority = 70,
            Targeting = SkillSystem.TargetRule.MultipleEnemies, // Изменено на MultipleEnemies
            TargetCount = 3, // Максимум 3 цели, AI будет выбирать до 3х ближайших
            ManaCost = 3,
            StaminaCost = 0, // Добавлено
            HealthCost = 0,  // Добавлено
            Effects = new[]
            {
                new SkillSystem.EffectData
                {
                    Type = SkillSystem.EffectType.MagicalDamage,
                    DamageType = SkillSystem.DamageType.Magical,
                    Power = 85, // Базовая сила, если нет Main/Secondary, или как fallback
                    MainTargetPower = 100, // Сильнее по главной цели
                    SecondaryPowers = new[] { 70f, 50f } // Для второй и третьей цели
                }
            }
        };
    }

    public static SkillSystem.SkillData Heal()
    {
        return new SkillSystem.SkillData
        {
            Id = "heal",
            Name = "Heal",
            Description = "Restores HP to an ally",
            AiCommand = SkillSystem.CommandType.Heal,
            AiPriority = 80,
            Targeting = SkillSystem.TargetRule.AllyOrSelf,
            TargetCount = 1,
            ManaCost = 3,
            StaminaCost = 0, // Добавлено
            HealthCost = 0,  // Добавлено
            Effects = new[]
            {
                new SkillSystem.EffectData
                {
                    Type = SkillSystem.EffectType.HealthHeal,
                    Power = 70 // Базовое количество исцеления
                }
            }
        };
    }
}