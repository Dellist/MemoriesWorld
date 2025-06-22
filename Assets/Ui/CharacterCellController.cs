using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCellController : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text strengthText;
    public Text enduranceText;
    public Text intelligenceText;
    public Text experienceText;
    public Text teamPositionText;
    public Text skillsText;

    private static CharacterCellController selectedCell;
    private Image backgroundImage;

    private Character assignedCharacter; // *** ИЗМЕНЕНО: Храним ссылку на объект Character ***

    public static event Action<CharacterCellController> OnCharacterSelected;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }

    /// <summary>
    /// Устанавливает данные персонажа напрямую из объекта Character.
    /// </summary>
    /// <param name="character">Объект Character с данными.</param>
    public void SetCharacter(Character character) // *** ИЗМЕНЕНО: Принимаем объект Character ***
    {
        if (character == null)
        {
            Debug.LogError("Character object is null!");
            return;
        }

        assignedCharacter = character; // Сохраняем ссылку на персонажа
        nameText.text = $"Name: {character.Name}";
        levelText.text = $"Level: {character.Level}";
        strengthText.text = $"STR: {character.Strength}";
        enduranceText.text = $"END: {character.Endurance}";
        intelligenceText.text = $"INT: {character.Intelligence}";
        experienceText.text = $"EXP: {character.Experience}/{character.ExperienceToNextLevel}";

        if (character.SkillInstances == null || character.SkillInstances.Count == 0)
        {
            skillsText.text = "No skills available.";
        }
        else
        {
            StringBuilder skillsBuilder = new StringBuilder();
            foreach (var skillInstance in character.SkillInstances)
            {
                if (skillInstance?.Data != null)
                {
                    skillsBuilder.AppendLine(skillInstance.Data.Name);
                }
            }

            skillsText.text = skillsBuilder.ToString();
        }
    }

    public string GetCharacterId()
    {
        return assignedCharacter?.Id; // Возвращаем ID из сохраненного объекта Character
    }

    public Character GetCharacter() // *** ДОБАВЛЕНО: Метод для получения объекта Character ***
    {
        return assignedCharacter;
    }

    public void OnClick()
    {
        Debug.Log($"Character {assignedCharacter?.Name} (ID: {assignedCharacter?.Id}) selected.");
        if (selectedCell != null)
        {
            selectedCell.Deselect();
        }
        selectedCell = this;
        selectedCell.Select();

        OnCharacterSelected?.Invoke(this);
        AssignmentManager.Instance.AssignSelectedCharacter(this);
    }

    private void Select()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.yellow;
        }
    }

    private void Deselect()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }

    public static CharacterCellController GetSelectedCell()
    {
        return selectedCell;
    }

    public void SetTeamPosition(int position, string teamName)
    {
        if (teamPositionText != null)
        {
            teamPositionText.text = $"{teamName} Pos: {position}";
        }
    }
}