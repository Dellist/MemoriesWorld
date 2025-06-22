using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // ���������, ���� ������������ OrderBy ��� ������ LINQ ������

public class CharacterPanelController : MonoBehaviour
{
    public GameObject characterSlotPrefab;
    public Transform content;
    public Button generateButton;
    public Button togglePanelButton;
    public Button playerTeamButton;
    public Button enemyTeamButton;
    public GameObject characterPanel;
    public Button startBattleButton;

    private bool isPanelVisible = false;

    private List<CharacterCellController> playerTeam = new List<CharacterCellController>();
    private List<CharacterCellController> enemyTeam = new List<CharacterCellController>();

    void Start()
    {
        // ��������� ���������� �� CharacterFactory
        CharacterFactory.LoadCharacters();

        // ����������� ������ � �������
        generateButton.onClick.AddListener(GenerateCharacter);
        togglePanelButton.onClick.AddListener(TogglePanel);
        startBattleButton.onClick.AddListener(StartBattleSimulation);

        // ���������� ���������� � ������
        DisplayCharacters();
    }

    void DisplayCharacters()
    {
        // ������� ������ ��������
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // ���������, ��� ������ ����������
        if (characterSlotPrefab == null)
        {
            Debug.LogError("CharacterSlotPrefab is not assigned in the inspector!");
            return;
        }

        // ������ ����� �������� ��� ������� ���������
        foreach (var character in CharacterFactory.CharacterStorage.Values) // *** ��������: ��������� �� Character �������� ***
        {
            var characterSlot = Instantiate(characterSlotPrefab, content);
            var cellController = characterSlot.GetComponent<CharacterCellController>();
            if (cellController != null)
            {
                cellController.SetCharacter(character); // *** ��������: �������� SetCharacter � �������� Character ***
            }
            else
            {
                Debug.LogError("CharacterCellController component not found on the instantiated prefab!");
            }
        }
    }

    void GenerateCharacter()
    {
        // ���������� ������ ���������
        CharacterFactory.CreateRandomCharacter();

        // ��������� ������
        DisplayCharacters();
    }

    void TogglePanel()
    {
        // ����������� ��������� ������
        isPanelVisible = !isPanelVisible;
        characterPanel.SetActive(isPanelVisible);
    }

    private List<CharacterSlot> ConvertToCharacterSlots(List<CharacterCellController> team)
    {
        List<CharacterSlot> slots = new List<CharacterSlot>();

        foreach (var cell in team)
        {
            // �������� CharacterSlot �� CharacterCellController
            var slot = cell.GetComponent<CharacterSlot>();
            if (slot != null)
            {
                slots.Add(slot);
            }
            else
            {
                Debug.LogWarning("CharacterCellController does not have a CharacterSlot component!");
            }
        }

        return slots;
    }
    void StartBattleSimulation()
    {
        // ���������, ��� ��� ������� ���������
        if (playerTeam.Count == 0 || enemyTeam.Count == 0)
        {
            Debug.LogWarning("Both teams must have characters to start the battle!");
            return;
        }

        // ����������� ������� �� CharacterCellController � CharacterSlot
        List<CharacterSlot> playerSlots = ConvertToCharacterSlots(playerTeam);
        List<CharacterSlot> enemySlots = ConvertToCharacterSlots(enemyTeam);

        // ���� BattleSetup
        var battleSetup = Object.FindFirstObjectByType<BattleSetup>();
        if (battleSetup != null)
        {
            battleSetup.SetupBattle(playerSlots, enemySlots);
        }
        else
        {
            Debug.LogError("BattleSetup not found in the scene!");
        }
    }
}