using System;
using System.Collections.Generic;
using UnityEngine;



public class TeamManager : MonoBehaviour
{
    private Dictionary<Character, Team> characterTeams = new Dictionary<Character, Team>();

    public List<CharacterSlot> characterSlots = new List<CharacterSlot>();

    private void Awake()
    {
        // Инициализация списка слотов
        characterSlots = new List<CharacterSlot>(GetComponentsInChildren<CharacterSlot>());
        if (characterSlots.Count == 0)
        {
            Debug.LogError($"No CharacterSlot objects found under {gameObject.name}!");
        }
    }

    public void ClearTeam()
    {
        foreach (var slot in characterSlots)
        {
            slot.ClearSlot();
        }

        // Очистка персонажей и команд
        characterTeams.Clear();
    }

    public void AssignCharacterToSlot(int index, Character character, Team team)
    {
        if (index >= 0 && index < characterSlots.Count)
        {
            if (IsCharacterAlreadyAssigned(character))
            {
                Debug.LogWarning($"Character {character.Name} is already assigned to another slot in {gameObject.name}!");
                return;
            }

            characterSlots[index].AssignCharacter(character);

            // Привязка персонажа к команде
            characterTeams[character] = team;
        }
        else
        {
            Debug.LogWarning($"Invalid slot index: {index} in {gameObject.name}");
        }
    }

    public List<CharacterSlot> GetCharacterSlots()
    {
        return characterSlots;
    }

    public bool IsCharacterAlreadyAssigned(Character character)
    {
        foreach (var slot in characterSlots)
        {
            if (slot.GetAssignedCharacter() == character)
            {
                return true;
            }
        }
        return false;
    }

    public Team GetTeam(Character character)
    {
        // Возвращаем команду персонажа, если она назначена
        return characterTeams.TryGetValue(character, out var team) ? team : Team.Player; // По умолчанию Player
    }

    public Func<Character, Team> GetTeamDelegate()
    {
        // Возвращаем делегат для определения команды
        return GetTeam;
    }
}