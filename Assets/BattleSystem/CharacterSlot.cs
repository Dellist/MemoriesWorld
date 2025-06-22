using UnityEngine;
using UnityEngine.UI;
using System; // ��������� ��� Action

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text manaText;
    [SerializeField] private Text staminaText;
    [SerializeField] private Button assignButton;
    [SerializeField] private Image characterImage;

    private Character assignedCharacter;

    // �������, ������� ����� ����������, ����� �������� � ����� ����� ������� (��������/������)
    public event Action<Character> OnCharacterAssignedOrCleared;

    private void Awake()
    {
        // �������� �� ������� ������ �� UI-��������
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

        // ������������ �� ������� ��� ����������� �����
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
    /// ��������� ��������� �� ���� ����.
    /// </summary>
    /// <param name="character">�������� ��� ����������.</param>
    public void AssignCharacter(Character character)
    {
        if (assignedCharacter != null)
        {
            // ������������ �� ������� ���������, ���� �� ���
            assignedCharacter.OnStatsChanged -= UpdateUI;
        }

        assignedCharacter = character;

        if (assignedCharacter != null)
        {
            // ������������� �� ������� ��������� ������������� ���������
            assignedCharacter.OnStatsChanged += UpdateUI;
        }

        UpdateUI(); // ��������� UI ����� ����� ����������
        Debug.Log($"Character {character.Name} assigned to the slot.");
        OnCharacterAssignedOrCleared?.Invoke(assignedCharacter); // ��������� �� ���������
    }

    /// <summary>
    /// ������� ���� �� ���������.
    /// </summary>
    public void ClearSlot()
    {
        if (assignedCharacter != null)
        {
            assignedCharacter.OnStatsChanged -= UpdateUI; // ������������ �� �������
            assignedCharacter = null;
        }
        UpdateUI(); // ��������� UI ����� �������
        Debug.Log("Slot cleared.");
        OnCharacterAssignedOrCleared?.Invoke(assignedCharacter); // ��������� �� ���������
    }

    public Character GetAssignedCharacter()
    {
        return assignedCharacter;
    }

    // ������� ����� GetTeamManager(), ��� ��� ��� ������ ������ ���� � AssignmentManager
    // ��� � BattleSetup, ������� ���������� �������������� ����� � �������.

    /// <summary>
    /// ��������� ����������� UI � ������������ � ������� ���������.
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
                characterImage.color = Color.white; // ���������� �����������
                // TODO: ���� � ��������� ���� Sprite, ���������� ��� �����:
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
                characterImage.color = Color.clear; // ������� �����������
            }
        }
    }
}