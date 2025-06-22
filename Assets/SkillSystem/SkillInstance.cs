
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillSystem 
{
    public class SkillInstance
    {
        public SkillData Data { get; private set; }
        private Character _user; 


        private const float PHYSICAL_ATTACK_STAT_MULTIPLIER = 0.7f; 
        private const float PHYSICAL_DEFENSE_STAT_MULTIPLIER = 0.5f; 
        private const float MAGICAL_ATTACK_STAT_MULTIPLIER = 0.9f; 
        private const float MAGICAL_DEFENSE_STAT_MULTIPLIER = 0.3f; 
        private const float HEAL_STAT_MULTIPLIER = 1.0f;

        private const float PERCENT_PER_DEFENSE_STAT_POINT = 0.01f; 
        private const float MAX_DEFENSE_REDUCTION = 0.9f;          
        private const float MIN_DAMAGE_MULTIPLIER = 0.1f;         
        private const float MIN_HEAL_MULTIPLIER = 0.1f;           


        public SkillInstance(SkillData skillData, Character user) 
        {
            Data = skillData;
            _user = user;
        }

        public bool CanUse(Character user)
        {
            return Data.CanUse(user); //
        }

  
        public List<Character> GetValidTargets(Character user, List<Character> allCharacters, Func<Character, Team> getTeam)
        {
            List<Character> validTargets = new List<Character>();
            Team userTeam = getTeam(user);
            List<Character> livingCharacters = allCharacters.Where(c => c.IsAlive()).ToList();

            switch (Data.Targeting) //
            {
                case TargetRule.Self: //
                    validTargets.Add(user); //
                    break;
                case TargetRule.SingleEnemy: //
                case TargetRule.MultipleEnemies: //
                case TargetRule.AllEnemies: //
                case TargetRule.RandomEnemies: 
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) != userTeam)); //
                    break;
                case TargetRule.SingleAlly: //
                case TargetRule.MultipleAllies: //
                case TargetRule.AllAllies: //
                case TargetRule.RandomAllies: 
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) == userTeam && c != user)); //
                    break;
                case TargetRule.AllyOrSelf: //
                    validTargets.AddRange(livingCharacters.Where(c => getTeam(c) == userTeam)); //
                    break;
                case TargetRule.AnySingle: //
                    validTargets.AddRange(livingCharacters); //
                    break;
                default:
                    Debug.LogWarning($"[SkillInstance.GetValidTargets] Неизвестное правило таргетинга: {Data.Targeting}");
                    break;
            }
            return validTargets; //
        }

        public void Execute(List<Character> targets) 
        {
          
            if (!CanUse(_user)) 
            {
                Debug.LogWarning($"{_user.Name} не может использовать {Data.Name}: недостаточно ресурсов.");
                return;
            }

            _user.ConsumeMana(Data.ManaCost); 
            _user.ConsumeStamina(Data.StaminaCost); 
            _user.TakeDamage(Data.HealthCost);

            foreach (var effect in Data.Effects)
            {
                ApplyEffect(_user, targets, effect);
            }
        }

        private void ApplyEffect(Character user, List<Character> targets, SkillSystem.EffectData effect) 

        {
            for (int i = 0; i < targets.Count; i++) //
            {
                Character target = targets[i]; //
                float power = effect.Power; //


                if (Data.Targeting == TargetRule.MultipleEnemies || Data.Targeting == TargetRule.MultipleAllies ||
                    Data.Targeting == TargetRule.AllEnemies || Data.Targeting == TargetRule.AllAllies ||
                    Data.Targeting == TargetRule.RandomEnemies || Data.Targeting == TargetRule.RandomAllies)
                {
                    if (i == 0 && effect.MainTargetPower != 0) //
                    {
                        power = effect.MainTargetPower; //
                    }
                    else if (i > 0 && effect.SecondaryPowers != null && effect.SecondaryPowers.Length >= i) //
                    {

                        power = effect.SecondaryPowers[i - 1]; //
                    }
                }


                if (!target.IsAlive()) continue; //

                switch (effect.Type) //
                {
                    case EffectType.PhysicalDamage: //
                    case EffectType.MagicalDamage: //
                        int damage = CalculateDamage(user, target, effect.DamageType, power); //
                        target.TakeDamage(damage); //
                        Debug.Log($"{user.Name} наносит {damage} {effect.DamageType} урона {target.Name} с помощью {Data.Name}.");
                        break;

                    case EffectType.HealthHeal: //
                        int heal = CalculateHeal(user, power); //
                        target.RestoreHealth(heal); //
                        Debug.Log($"{user.Name} лечит {heal} HP {target.Name} с помощью {Data.Name}.");
                        break;

                    case EffectType.StaminaDamage: //
                        int staminaDamage = (int)GetExpectedStaminaDamage(user, target, effect); // 
                        target.ConsumeStamina(staminaDamage); //
                        Debug.Log($"{user.Name} наносит {staminaDamage} урона выносливости {target.Name} с помощью {Data.Name}.");
                        break;

                    case EffectType.StaminaRestore: //
                        int staminaRestore = (int)GetExpectedStaminaRestore(user, target, effect); //
                        target.RestoreStamina(staminaRestore); //
                        Debug.Log($"{user.Name} восстанавливает {staminaRestore} выносливости {target.Name} с помощью {Data.Name}.");
                        break;

                    case EffectType.ManaDamage: //
                        int manaDamage = (int)GetExpectedManaDamage(user, target, effect); // 
                        target.ConsumeMana(manaDamage); //
                        Debug.Log($"{user.Name} наносит {manaDamage} урона мане {target.Name} с помощью {Data.Name}.");
                        break;

                    case EffectType.ManaRestore: //
                        int manaRestore = (int)GetExpectedManaRestore(user, target, effect); // 
                        target.RestoreMana(manaRestore); //
                        Debug.Log($"{user.Name} восстанавливает {manaRestore} маны {target.Name} с помощью {Data.Name}.");
                        break;
                    case EffectType.StatModify: //
                        Debug.LogWarning($"[SkillInstance.ApplyEffect] EffectType {effect.Type} не реализован. (StatType: {effect.StatType}, Power: {effect.Power}, Duration: {effect.Duration})"); //
                        break;
                    case EffectType.StatusApply: //
                        Debug.LogWarning($"[SkillInstance.ApplyEffect] EffectType {effect.Type} не реализован. (StatusId: {effect.StatusId}, Duration: {effect.Duration})"); //
                        break;
                    case EffectType.StatusRemove: //
                        Debug.LogWarning($"[SkillInstance.ApplyEffect] EffectType {effect.Type} не реализован. (StatusId: {effect.StatusId})"); //
                        break;
                }
            }
        }

        private int CalculateDamage(Character user, Character target, DamageType damageType, float power)
        {
            float relevantOffenseStat = 0;
            float defenseReduction = 0.0f;

            switch (damageType) //
            {
                case DamageType.Physical: //
                    relevantOffenseStat = user.Strength * PHYSICAL_ATTACK_STAT_MULTIPLIER;
                    defenseReduction = target.Endurance * PHYSICAL_DEFENSE_STAT_MULTIPLIER;
                    break;
                case DamageType.Magical: //
                    relevantOffenseStat = user.Intelligence * MAGICAL_ATTACK_STAT_MULTIPLIER;
                    defenseReduction = target.Intelligence * MAGICAL_DEFENSE_STAT_MULTIPLIER;
                    break;
                case DamageType.Pure: //
                    relevantOffenseStat = 1.0f; 
                    defenseReduction = 0.0f;
                    break;
            }

     
            float baseDamage = power + relevantOffenseStat;

            float finalDamage = Mathf.Max(0, baseDamage - defenseReduction);


            finalDamage = Mathf.Max(finalDamage, power * MIN_DAMAGE_MULTIPLIER); 

            int calculatedDamage = Mathf.RoundToInt(finalDamage);

            return Mathf.Max(1, calculatedDamage); 
        }

        private int CalculateHeal(Character user, float power)
        {
            // Для лечения, Интеллект - ключевой стат
            float relevantHealStat = user.Intelligence * HEAL_STAT_MULTIPLIER;

            // Базовое лечение, масштабированное от Power навыка
            float baseHeal = power + relevantHealStat;

            // Учет минимального лечения
            baseHeal = Mathf.Max(baseHeal, power * MIN_HEAL_MULTIPLIER); //

            int calculatedHeal = Mathf.RoundToInt(baseHeal);

            return Mathf.Max(1, calculatedHeal); // Минимум 1 лечения
        }


        public float GetExpectedDamage(Character user, Character target) //
        {
            float totalExpectedDamage = 0; //
            foreach (var effect in Data.Effects) //
            {
                if (effect.Type == EffectType.PhysicalDamage || effect.Type == EffectType.MagicalDamage) //
                {
                    float effectPower = effect.Power; //

                    if (Data.Targeting == TargetRule.MultipleEnemies || Data.Targeting == TargetRule.AllEnemies ||
                        Data.Targeting == TargetRule.RandomEnemies)
                    {
                        if (effect.MainTargetPower != 0) // 
                        {
                            effectPower = effect.MainTargetPower; //
                        }
                    }
                    totalExpectedDamage += CalculateDamage(user, target, effect.DamageType, effectPower); //
                }
            }
            return totalExpectedDamage; //
        }

        public float GetExpectedHeal(Character user, Character target) //
        {
            float totalExpectedHeal = 0; //
            foreach (var effect in Data.Effects) //
            {
                if (effect.Type == EffectType.HealthHeal) //
                {
                    float effectPower = effect.Power;
                    if (Data.Targeting == TargetRule.MultipleAllies || Data.Targeting == TargetRule.AllAllies ||
                        Data.Targeting == TargetRule.RandomAllies)
                    {
                        if (effect.MainTargetPower != 0) // 
                        {
                            effectPower = effect.MainTargetPower;
                        }
                    }
                    totalExpectedHeal += CalculateHeal(user, effectPower); //
                }
            }
            return totalExpectedHeal; //
        }

        public float GetExpectedStaminaDamage(Character user, Character target, EffectData effect = null) //
        {
            float power = (effect != null) ? effect.Power : Data.Effects.Where(e => e.Type == EffectType.StaminaDamage).Sum(e => e.Power);
            return power; //
        }

        public float GetExpectedStaminaRestore(Character user, Character target, EffectData effect = null) //
        {
            float power = (effect != null) ? effect.Power : Data.Effects.Where(e => e.Type == EffectType.StaminaRestore).Sum(e => e.Power);
            return power; //
        }

        public float GetExpectedManaDamage(Character user, Character target, EffectData effect = null) //
        {
            float power = (effect != null) ? effect.Power : Data.Effects.Where(e => e.Type == EffectType.ManaDamage).Sum(e => e.Power);
            return power; //
        }

        public float GetExpectedManaRestore(Character user, Character target, EffectData effect = null) //
        {
            float power = (effect != null) ? effect.Power : Data.Effects.Where(e => e.Type == EffectType.ManaRestore).Sum(e => e.Power);
            return power; //
        }

        public float GetExpectedDamageForIncomingAttack(Character target) //
        {
            return 0; //
        }
    }
}