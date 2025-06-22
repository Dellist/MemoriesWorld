// Character.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using SkillSystem; // ��������� using

[Serializable]
public class Character
{
    public string Id { get; private set; }
    public string Name;

    public int Strength;
    public int Endurance;
    public int Intelligence;

    public int CurrentHealth { get; private set; } //
    public int CurrentStamina { get; private set; } //
    public int CurrentMana { get; private set; } //

    public List<SkillInstance> SkillInstances { get; private set; } = new List<SkillInstance>(); //

    public float Speed => 80 + (Endurance * 0.25f); //
    public int MaxHealth => Strength * 100; //
    public int MaxStamina => Endurance * 5; //
    public int MaxMana => Intelligence * 7; //

    public bool HasSelectedAction { get; set; } //

    public event Action OnStatsChanged; //

    // ����� ����������
    public int Level { get; private set; } = 1; // ������� ���������
    public int Experience { get; private set; } = 0; // ������� ����
    public int ExperienceToNextLevel => Level * 100; // ���� ��� ���������� ������
    public GrowthParameters GrowthParams { get; private set; } // ��������� �����

    public Character(int strength, int endurance, int intelligence, string name, GrowthParameters growthParams) //
    {
        Id = Guid.NewGuid().ToString(); //
        Name = name; //
        Strength = strength; //
        Endurance = endurance; //
        Intelligence = intelligence; //
        GrowthParams = growthParams; //

        InitializeStats(); //
    }

    private Character() { } //

    public void InitializeStats() //
    {
        CurrentHealth = MaxHealth; //
        CurrentStamina = MaxStamina; //
        CurrentMana = MaxMana; //
        OnStatsChanged?.Invoke(); //
    }

    public bool IsAlive() //
    {
        return CurrentHealth > 0; //
    }
    public void AddExperience(int amount) //
    {
        Experience += amount; //
        Debug.Log($"Character {Name} gained {amount} experience. Total: {Experience}/{ExperienceToNextLevel}"); //

        while (Experience >= ExperienceToNextLevel) //
        {
            Experience -= ExperienceToNextLevel; //
            LevelUp(); //
        }
    }

    public void LevelUp() //
    {
        Level++; //
        ApplyGrowth(); //
        InitializeStats(); //
        Debug.Log($"Character {Name} leveled up to {Level}!"); //
    }

    private void ApplyGrowth() //
    {
        Strength += GrowthParams.StrengthGrowth; //
        Endurance += GrowthParams.EnduranceGrowth; //
        Intelligence += GrowthParams.IntelligenceGrowth; //
        OnStatsChanged?.Invoke(); //
    }

    public void AddSkill(SkillSystem.SkillData skillData) //
    {
        if (skillData != null) //
        {
            SkillInstances.Add(new SkillInstance(skillData, this)); // ��������: �������� 'this'
        }
    }

    [Serializable]
    public class GrowthParameters //
    {
        public int StrengthGrowth; //
        public int EnduranceGrowth; //
        public int IntelligenceGrowth; //

        public GrowthParameters(int strengthGrowth, int enduranceGrowth, int intelligenceGrowth) //
        {
            StrengthGrowth = strengthGrowth; //
            EnduranceGrowth = enduranceGrowth; //
            IntelligenceGrowth = intelligenceGrowth; //
        }
    }

    // ����������� ��� ���������� ������ ��� �������������� � ���������
    public void TakeDamage(int damage) //
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage); //
        OnStatsChanged?.Invoke(); //
        if (CurrentHealth <= 0)
        {
            Debug.Log($"{Name} �����!");
        }
    }

    public void RestoreHealth(int amount) // ���������� �������� RestoreHealth
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount); //
        OnStatsChanged?.Invoke(); //
    }

    public void ConsumeMana(int amount) //
    {
        CurrentMana = Mathf.Max(0, CurrentMana - amount); //
        OnStatsChanged?.Invoke(); //
    }

    public void RestoreMana(int amount) //
    {
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount); //
        OnStatsChanged?.Invoke(); //
    }

    public void ConsumeStamina(int amount) //
    {
        CurrentStamina = Mathf.Max(0, CurrentStamina - amount); //
        OnStatsChanged?.Invoke(); //
    }

    public void RestoreStamina(int amount) //
    {
        CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + amount); //
        OnStatsChanged?.Invoke(); //
    }

    // ��� ������ �� ���� � ���������� Character.cs, �� ���� � ���� ������� ��������.
    // ���� ��� ����� ��� ����� �� �������/����, �� �� ����� ��������.
    public void TakeStaminaDamage(int amount)
    {
        CurrentStamina = Mathf.Max(0, CurrentStamina - amount);
        OnStatsChanged?.Invoke();
    }

    public void TakeManaDamage(int amount)
    {
        CurrentMana = Mathf.Max(0, CurrentMana - amount);
        OnStatsChanged?.Invoke();
    }


    /// <summary>
    /// ���������� ����� ��� ������������/�������������� Character � JSON.
    /// ������������ ��� ���������� � �������� ������ ����������� �����.
    /// </summary>
    [Serializable]
    private class CharacterDataForJson //
    {
        public string Id; //
        public string Name; //
        public int Strength; //
        public int Endurance; //
        public int Intelligence; //
        public int CurrentHealth; //
        public int CurrentStamina; //
        public int CurrentMana; //
        public List<string> SkillIds; // ������ ID ������� ��� ����������
        public int Level; // ��������� ��� ������������
        public int Experience; // ��������� ��� ������������
        public GrowthParameters GrowthParams; // ��������� ��� ������������
    }

    /// <summary>
    /// ����������� ������ Character � JSON-������.
    /// </summary>
    public string ToJson() //
    {
        CharacterDataForJson data = new CharacterDataForJson //
        {
            Id = Id, //
            Name = Name, //
            Strength = Strength, //
            Endurance = Endurance, //
            Intelligence = Intelligence, //
            CurrentHealth = CurrentHealth, //
            CurrentStamina = CurrentStamina, //
            CurrentMana = CurrentMana, //
            Level = Level, // ���������
            Experience = Experience, // ���������
            GrowthParams = GrowthParams, // ���������
            SkillIds = new List<string>() //
        };

        foreach (var skillInstance in SkillInstances) //
        {
            if (skillInstance?.Data != null) //
            {
                data.SkillIds.Add(skillInstance.Data.Id); //
            }
        }

        return JsonUtility.ToJson(data); //
    }

    /// <summary>
    /// ������������� ������ Character �� JSON-������.
    /// ��������������� SkillInstance, ��������� SkillPresets.
    /// </summary>
    public static Character FromJson(string json) //
    {
        CharacterDataForJson data = JsonUtility.FromJson<CharacterDataForJson>(json); //

        // ���������� ��������� �����������
        Character character = new Character //
        {
            Id = data.Id, //
            Name = data.Name, //
            Strength = data.Strength, //
            Endurance = data.Endurance, //
            Intelligence = data.Intelligence, //
            CurrentHealth = data.CurrentHealth, //
            CurrentStamina = data.CurrentStamina, //
            CurrentMana = data.CurrentMana, //
            Level = data.Level, // ���������
            Experience = data.Experience, // ���������
            GrowthParams = data.GrowthParams, // ���������
            SkillInstances = new List<SkillInstance>() // �������������� ����� ������
        };

        // ��������������� SkillInstance �� SkillIds
        foreach (var skillId in data.SkillIds) //
        {
            SkillSystem.SkillData skillData = SkillPresets.GetSkillById(skillId); // ������������, ��� SkillPresets.GetSkillById ����������
            if (skillData != null) //
            {
                character.AddSkill(skillData); // ���������� AddSkill ��� ���������� SkillInstance (������ � `this`)
            }
            else
            {
                Debug.LogWarning($"[Character.FromJson] ����� � ID '{skillId}' �� ������ � SkillPresets. ��������."); //
            }
        }

        return character; //
    }
}