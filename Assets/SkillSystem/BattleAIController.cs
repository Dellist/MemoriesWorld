using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SkillSystem;

public class BattleAIController
{
    private Func<Character, Team> getTeam;

    public BattleAIController(Func<Character, Team> getTeamFunc)
    {
        getTeam = getTeamFunc;
    }

    public (SkillInstance, List<Character>) DecideAction(Character ai, List<Character> allCharacters)
    {
        // Используем AIActionEvaluator для выбора лучшего действия
        var evaluator = new AIActionEvaluator(ai, allCharacters, getTeam);
        return evaluator.GetBestAction();
    }

    // Методы EvaluateSkill, EvaluateResourceCost и GetTargetGroups
    // были перемещены или изменены в AIActionEvaluator и больше не нужны здесь.
}