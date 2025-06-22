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
        // ���������� AIActionEvaluator ��� ������ ������� ��������
        var evaluator = new AIActionEvaluator(ai, allCharacters, getTeam);
        return evaluator.GetBestAction();
    }

    // ������ EvaluateSkill, EvaluateResourceCost � GetTargetGroups
    // ���� ���������� ��� �������� � AIActionEvaluator � ������ �� ����� �����.
}