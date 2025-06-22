using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO; // Добавляем для работы с файлами

public static class CharacterFactory
{
    private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "CharacterStorage.json");

    private static readonly string[] NameParts1 = { "Al", "Be", "Ce", "De", "El", "Fa", "Ga", "Ha", "Il", "Jo" };
    private static readonly string[] NameParts2 = { "ron", "bert", "vin", "ton", "rick", "son", "mar", "len", "don", "fred" };

    public static Dictionary<string, Character> CharacterStorage { get; private set; } = new Dictionary<string, Character>();

    static CharacterFactory()
    {
        LoadCharacters();
    }

    public static Character CreateRandomCharacter()
    {
        string name = GenerateRandomName();
        int baseStrength = UnityEngine.Random.Range(5, 11);
        int baseEndurance = UnityEngine.Random.Range(5, 11);
        int baseIntelligence = UnityEngine.Random.Range(5, 11);

        var growthParams = GenerateGrowthParameters();

        var character = new Character(baseStrength, baseEndurance, baseIntelligence, name, growthParams);

        character.InitializeStats();
        character.AddSkill(SkillPresets.BasicAttack());

        var allSkills = new List<SkillSystem.SkillData>
        {
            SkillPresets.Fireball(),
            SkillPresets.Heal()
        };

        var shuffledSkills = allSkills.OrderBy(x => Guid.NewGuid()).ToList();
        int skillsToAdd = UnityEngine.Random.Range(1, Mathf.Min(shuffledSkills.Count, 3) + 1);

        for (int i = 0; i < skillsToAdd; i++)
        {
            character.AddSkill(shuffledSkills[i]);
        }

        CharacterStorage.Add(character.Id, character);

        Debug.Log($"Character created: {character.Name} (ID: {character.Id})");
        SaveCharacters(); // *** КРИТИЧНОЕ ИЗМЕНЕНИЕ: Сохраняем после создания персонажа ***
        return character;
    }

    private static string GenerateRandomName()
    {
        string part1 = NameParts1[UnityEngine.Random.Range(0, NameParts1.Length)];
        string part2 = NameParts2[UnityEngine.Random.Range(0, NameParts2.Length)];
        return part1 + part2;
    }

    private static Character.GrowthParameters GenerateGrowthParameters()
    {
        int totalGrowthPoints = 13;
        int strengthGrowth = UnityEngine.Random.Range(1, totalGrowthPoints - 2);
        int enduranceGrowth = UnityEngine.Random.Range(1, totalGrowthPoints - strengthGrowth - 1);
        int intelligenceGrowth = totalGrowthPoints - strengthGrowth - enduranceGrowth;

        return new Character.GrowthParameters(strengthGrowth, enduranceGrowth, intelligenceGrowth);
    }

    public static void SaveCharacters()
    {
        var wrapper = new SerializationWrapper();

        foreach (var character in CharacterStorage.Values)
        {
            wrapper.Entries.Add(new KeyValue
            {
                Key = character.Id,
                Value = character.ToJson()
            });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(SaveFilePath, json);
    }

    public static void LoadCharacters()
    {
        if (!File.Exists(SaveFilePath))
        {
            CharacterStorage.Clear();
            Debug.LogWarning("No character save file found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

            CharacterStorage.Clear();

            if (wrapper != null && wrapper.Entries != null)
            {
                foreach (var kv in wrapper.Entries)
                {
                    if (!string.IsNullOrEmpty(kv.Value))
                    {
                        var character = Character.FromJson(kv.Value);
                        if (character != null)
                            CharacterStorage[kv.Key] = character;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Save file structure is invalid.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}. Deleting file.");
            File.Delete(SaveFilePath);
            CharacterStorage.Clear();
        }
    }

    public static void ClearCharacterStorage()
    {
        CharacterStorage.Clear();

        if (File.Exists(SaveFilePath))
        {
            try
            {
                File.Delete(SaveFilePath);
                Debug.Log($"Character save file deleted: {SaveFilePath}.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete character save file at {SaveFilePath}: {e.Message}");
            }
        }
        Debug.Log("Character storage cleared.");
    }
}

// Убедитесь, что этот класс находится вне CharacterFactory (или является его public nested class)
// и помечен [Serializable]
[Serializable]
public class KeyValue
{
    public string Key;
    public string Value;
}
public class SerializationWrapper
{
    public List<KeyValue> Entries = new List<KeyValue>();
}