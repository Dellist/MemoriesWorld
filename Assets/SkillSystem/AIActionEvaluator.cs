
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SkillSystem; // 

public class AIActionEvaluator
{
    private Character _self; //
    private List<Character> _allCharacters; //
    private Func<Character, Team> _getTeam; //
    private List<Character> _allies; //
    private List<Character> _enemies; //

    private const float HEAL_LOW_HP_BONUS_FACTOR = 3.0f; // 
    private const float HEAL_MID_HP_BONUS_FACTOR = 1.5f; // 
    private const float OVERHEAL_PENALTY = 5000f; // 

    private const float DAMAGE_LOW_HP_BONUS_FACTOR = 1.5f; // 
    private const float RESOURCE_LOW_PENALTY_FACTOR = 1.5f; // 
    private const float FINISHING_BLOW_BONUS = 1000f; // 


    private const int NUM_RANDOM_TARGET_GROUPS = 5; 

    public AIActionEvaluator(Character self, List<Character> allCharacters, Func<Character, Team> getTeam) //
    {
        _self = self; //
        _allCharacters = allCharacters; //
        _getTeam = getTeam; //

        Team selfTeam = getTeam(_self); //
        _allies = allCharacters.Where(c => getTeam(c) == selfTeam && c.IsAlive()).ToList(); //
        _enemies = allCharacters.Where(c => getTeam(c) != selfTeam && c.IsAlive()).ToList(); //
    }

    public (SkillInstance skill, List<Character> targets) GetBestAction() //
    {
        var possibleActions = GetPossibleActions(); //
        if (!possibleActions.Any()) //
        {
            return (null, null); // 
        }

        var bestAction = possibleActions.OrderByDescending(action => action.score).FirstOrDefault(); //
        return (bestAction.skill, bestAction.targets); //
    }


    public List<(SkillInstance skill, List<Character> targets, float score)> GetPossibleActions() //
    {
        var possibleActions = new List<(SkillInstance, List<Character>, float)>(); //

        foreach (var skill in _self.SkillInstances) //
        {


            var validPotentialTargets = skill.GetValidTargets(_self, _allCharacters, _getTeam); //


            if (skill.Data.Targeting == TargetRule.Self || //
                skill.Data.Targeting == TargetRule.AllEnemies || //
                skill.Data.Targeting == TargetRule.AllAllies) //
            {
                if (validPotentialTargets.Any()) //
                {
                    float score = EvaluateSkill(skill, validPotentialTargets); //
                    possibleActions.Add((skill, validPotentialTargets, score)); //
                }
            }
            // Для SingleEnemy/SingleAlly/AnySingle/AllyOrSelf
            else if (skill.Data.Targeting == TargetRule.SingleEnemy || //
                     skill.Data.Targeting == TargetRule.SingleAlly || //
                     skill.Data.Targeting == TargetRule.AnySingle || //
                     skill.Data.Targeting == TargetRule.AllyOrSelf) //
            {
                foreach (var target in validPotentialTargets) //
                {
                    float score = EvaluateSkill(skill, new List<Character> { target }); //
                    possibleActions.Add((skill, new List<Character> { target }, score)); //
                }
            }

            else if (skill.Data.Targeting == TargetRule.MultipleEnemies || //
                     skill.Data.Targeting == TargetRule.MultipleAllies || //
                     skill.Data.Targeting == TargetRule.RandomEnemies || //
                     skill.Data.Targeting == TargetRule.RandomAllies) //
            {
                var targetGroups = GetTargetGroups(validPotentialTargets, skill.Data.TargetCount, skill.Data.Targeting); // 
                foreach (var group in targetGroups) //
                {
                    if (group.Any()) //
                    {
                        float score = EvaluateSkill(skill, group); //
                        possibleActions.Add((skill, group, score)); //
                    }
                }
            }
        }
        return possibleActions; //
    }

    private float EvaluateSkill(SkillInstance skill, List<Character> targets) //
    {
        float score = 0; //


        score += skill.Data.AiPriority; //


        score += EvaluateHealthImpact(skill, targets); //


        score -= EvaluateResourceCost(skill); //


        score += EvaluateFinishingBlow(skill, targets); //

        return score; //
    }

    private float EvaluateHealthImpact(SkillInstance skill, List<Character> targets) //
    {
        float totalImpact = 0; //
        foreach (var effect in skill.Data.Effects) //
        {
            foreach (var target in targets) //
            {
                if (!target.IsAlive()) continue; //

                switch (effect.Type) //
                {
                    case EffectType.PhysicalDamage: //
                    case EffectType.MagicalDamage: //
                        float expectedDamage = skill.GetExpectedDamage(_self, target); //

                        if (target.CurrentHealth <= 0) continue; 

               
                        float damageUtility = expectedDamage;
                        float targetHealthPercentage = (float)target.CurrentHealth / target.MaxHealth;

                        if (targetHealthPercentage < 0.2f) //
                        {
                            damageUtility *= DAMAGE_LOW_HP_BONUS_FACTOR;
                        }
                        else if (targetHealthPercentage < 0.5f) //
                        {
                            damageUtility *= 1.2f;
                        }
                        totalImpact += damageUtility; //
                        break;

                    case EffectType.HealthHeal: //
                        float expectedHeal = skill.GetExpectedHeal(_self, target); //
                        float healthMissing = target.MaxHealth - target.CurrentHealth; //

                        if (healthMissing <= 0) //
                        {
                            totalImpact -= OVERHEAL_PENALTY; 
                        }
                        else
                        {
                            float actualHealBenefit = Mathf.Min(expectedHeal, healthMissing); //
                            float healUtility = actualHealBenefit; //

                            
                            float currentHealthPercentage = (float)target.CurrentHealth / target.MaxHealth;
                            if (currentHealthPercentage < 0.1f) // 
                            {
                                healUtility *= HEAL_LOW_HP_BONUS_FACTOR;
                            }
                            else if (currentHealthPercentage < 0.4f) // Низкое HP
                            {
                                healUtility *= HEAL_MID_HP_BONUS_FACTOR;
                            }
                            else
                            {
                                healUtility *= 0.5f; // 
                            }
                            totalImpact += healUtility; //

                      
                            float expectedIncomingDamage = skill.GetExpectedDamageForIncomingAttack(target); //
                            if (target.CurrentHealth <= expectedIncomingDamage && (target.CurrentHealth + expectedHeal) > expectedIncomingDamage) //
                            {
                                totalImpact += 1000f * HEAL_MID_HP_BONUS_FACTOR; 
                            }
                   
                            else if (currentHealthPercentage < 0.2f && (target.CurrentHealth + expectedHeal) / target.MaxHealth >= 0.2f) //
                            {
                                totalImpact += 200f; //
                            }
                        }
                        break;

                    case EffectType.StaminaDamage: //
                        totalImpact += skill.GetExpectedStaminaDamage(_self, target) * 0.7f; 
                        break;
                    case EffectType.StaminaRestore: //
                        totalImpact += skill.GetExpectedStaminaRestore(_self, target) * 0.5f; 
                        break;
                    case EffectType.ManaDamage: //
                        totalImpact += skill.GetExpectedManaDamage(_self, target) * 0.8f; 
                        break;
                    case EffectType.ManaRestore: //
                        totalImpact += skill.GetExpectedManaRestore(_self, target) * 0.6f; 
                        break;
                    case EffectType.StatModify: //
                    case EffectType.StatusApply: //
                    case EffectType.StatusRemove: //
                        break;
                }
            }
        }
        return totalImpact; //
    }

    private float EvaluateResourceCost(SkillInstance skill) //
    {
        float penalty = 0; //
        var skillData = skill.Data; //

   
        if (!skill.CanUse(_self)) 
        {
            return 1000000f; // 
        }

     
        float manaPercentage = (float)_self.CurrentMana / _self.MaxMana;
        penalty += skillData.ManaCost * (1 + (1 - manaPercentage) * RESOURCE_LOW_PENALTY_FACTOR); // 

 
        float staminaPercentage = (float)_self.CurrentStamina / _self.MaxStamina;
        penalty += skillData.StaminaCost * (1 + (1 - staminaPercentage) * RESOURCE_LOW_PENALTY_FACTOR * 0.5f); //


        float healthPercentage = (float)_self.CurrentHealth / _self.MaxHealth;
        penalty += skillData.HealthCost * (1 + (1 - healthPercentage) * RESOURCE_LOW_PENALTY_FACTOR * 2.0f); // 

        return penalty;
    }

    private float EvaluateFinishingBlow(SkillInstance skill, List<Character> targets) //
    {
        float bonus = 0; //

        foreach (var effect in skill.Data.Effects) //
        {
            if (effect.Type == EffectType.PhysicalDamage || effect.Type == EffectType.MagicalDamage) //
            {
                foreach (var target in targets) //
                {

                    if (target.IsAlive() && skill.GetExpectedDamage(_self, target) >= target.CurrentHealth && target.CurrentHealth > 0) //
                    {
                        bonus += FINISHING_BLOW_BONUS; //
                    }
                }
            }
        }
        return bonus; //
    }


    private List<List<Character>> GetTargetGroups(List<Character> potentialTargets, int targetCount, TargetRule targetRule) 
    {
        var groups = new List<List<Character>>(); //


        if (potentialTargets.Count <= targetCount) //
        {
            if (potentialTargets.Any()) //
            {
                groups.Add(potentialTargets.ToList());
            }
            return groups; //
        }

    
        if (targetCount <= 1) //
        {
            foreach (var t in potentialTargets) //
            {
                groups.Add(new List<Character> { t }); //
            }
            return groups.Distinct(new CharacterListComparer()).ToList(); //
        }

        if (targetRule == TargetRule.MultipleEnemies || targetRule == TargetRule.AllEnemies || targetRule == TargetRule.RandomEnemies)
        {

            var sortedByHealth = potentialTargets.OrderBy(c => c.CurrentHealth).ToList();
            groups.Add(sortedByHealth.Take(targetCount).ToList());
        }
        else if (targetRule == TargetRule.MultipleAllies || targetRule == TargetRule.AllAllies || targetRule == TargetRule.RandomAllies)
        {

            var sortedByHealthPercentage = potentialTargets.OrderBy(c => (float)c.CurrentHealth / c.MaxHealth).ToList();
            groups.Add(sortedByHealthPercentage.Take(targetCount).ToList());
        }



        for (int i = 0; i < NUM_RANDOM_TARGET_GROUPS; i++) //
        {
            List<Character> shuffledTargets = potentialTargets.OrderBy(x => Guid.NewGuid()).ToList(); //
            List<Character> randomGroup = shuffledTargets.Take(targetCount).ToList(); //

            if (randomGroup.Any()) //
            {
                groups.Add(randomGroup); //
            }
        }
        return groups.Distinct(new CharacterListComparer()).ToList(); //
    }

    private class CharacterListComparer : IEqualityComparer<List<Character>> //
    {
        public bool Equals(List<Character> x, List<Character> y) //
        {
            if (x == null || y == null) return false; //
            if (x.Count != y.Count) return false; //

            var sortedX = x.OrderBy(c => c.Id).ToList(); //
            var sortedY = y.OrderBy(c => c.Id).ToList(); //

            for (int i = 0; i < sortedX.Count; i++) //
            {
                if (sortedX[i].Id != sortedY[i].Id) return false; //
            }
            return true; //
        }

        public int GetHashCode(List<Character> obj) //
        {
            if (obj == null) return 0; //
            unchecked //
            {
                int hash = 17; //
                foreach (var character in obj.OrderBy(c => c.Id)) //
                {
                    hash = hash * 23 + character.Id.GetHashCode(); //
                }
                return hash; //
            }
        }
    }
}