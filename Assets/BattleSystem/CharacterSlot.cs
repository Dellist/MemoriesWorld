using UnityEngine;
using UnityEngine.UI;
using System; // Добавлено для Action

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Button assignButton;
    [SerializeField] private Image characterImage;

    private Character assignedCharacter;

    // Событие, которое будет вызываться, когда персонаж в слоте будет изменен (назначен/очищен)
    public event Action<Character> OnCharacterAssignedOrCleared;

    private void Awake()
    {
        // Проверка на наличие ссылок на UI-элементы
        if (nameText == null || healthText == null || manaText == null || staminaText == null || characterImage == null)
        {
            Debug.LogError("UI references in CharacterSlot are not assigned! Please assign all fields in the inspector.");
        }

        if (assignButton != null)
        {
            assignButton.onClick.AddListener(OnAssignButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (assignButton != null)
        {
            assignButton.onClick.RemoveListener(OnAssignButtonClicked);
        }

        // Отписываемся от события при уничтожении слота
        if (assignedCharacter != null)
        {
            assignedCharacter.OnStatsChanged -= UpdateUI;
        }
    }

    private void OnAssignButtonClicked()
    {
        if (AssignmentManager.Instance != null)
        {
            AssignmentManager.Instance.StartAssignment(this);
        }
    }

    /// <summary>
    /// Назначает персонажа на этот слот.
    /// </summary>
    /// <param name="character">Персонаж для назначения.</param>
    public void AssignCharacter(Character character)
    {
        if (assignedCharacter != null)
        {
            // Отписываемся от старого персонажа, если он был
            assignedCharacter.OnStatsChanged -= UpdateUI;
        }

        assignedCharacter = character;

        if (assignedCharacter != null)
        {
            // Подписываемся на событие изменения характеристик персонажа
            assignedCharacter.OnStatsChanged += UpdateUI;
        }

        UpdateUI(); // Обновляем UI сразу после назначения
        Debug.Log($"Character {character.Name} assigned to the slot.");
        OnCharacterAssignedOrCleared?.Invoke(assignedCharacter); // Оповещаем об изменении
    }

    /// <summary>
    /// Очищает слот от персонажа.
    /// </summary>
    public void ClearSlot()
    {
        if (assignedCharacter != null)
        {
            assignedCharacter.OnStatsChanged -= UpdateUI; // Отписываемся от событий
            assignedCharacter = null;
        }
        UpdateUI(); // Обновляем UI после очистки
        Debug.Log("Slot cleared.");
        OnCharacterAssignedOrCleared?.Invoke(assignedCharacter); // Оповещаем об изменении
    }

    public Character GetAssignedCharacter()
    {
        return assignedCharacter;
    }

    // Удаляем метод GetTeamManager(), так как его логика должна быть в AssignmentManager
    // или в BattleSetup, которые определяют принадлежность слота к команде.

    /// <summary>
    /// Обновляет отображение UI в соответствии с данными персонажа.
    /// </summary>
    private void UpdateUI()
    {
        if (assignedCharacter != null)
        {
            nameText.text = assignedCharacter.Name;
            healthText.text = $"HP: {assignedCharacter.CurrentHealth}/{assignedCharacter.MaxHealth}";
            manaText.text = $"MP: {assignedCharacter.CurrentMana}/{assignedCharacter.MaxMana}";
            staminaText.text = $"SP: {assignedCharacter.CurrentStamina}/{assignedCharacter.MaxStamina}";

            if (characterImage != null)
            {
                characterImage.color = Color.white; // Показываем изображение
                // TODO: Если у персонажа есть Sprite, установите его здесь:
                // characterImage.sprite = assignedCharacter.CharacterSprite; 
            }
        }
        else
        {
            nameText.text = "Empty";
            healthText.text = "HP: -/-";
            manaText.text = "MP: -/-";
            staminaText.text = "SP: -/-";

            if (characterImage != null)
            {
                characterImage.color = Color.clear; // Убираем изображение
            }
        }
    }
}