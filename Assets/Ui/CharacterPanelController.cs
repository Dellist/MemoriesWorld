using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Добавляем, если используется OrderBy или другие LINQ методы

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
        // Загружаем персонажей из CharacterFactory
        CharacterFactory.LoadCharacters();

        // Привязываем кнопки к методам
        generateButton.onClick.AddListener(GenerateCharacter);
        togglePanelButton.onClick.AddListener(TogglePanel);
        startBattleButton.onClick.AddListener(StartBattleSimulation);

        // Отображаем персонажей в панели
        DisplayCharacters();
    }

    void DisplayCharacters()
    {
        // Удаляем старые элементы
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Проверяем, что префаб установлен
        if (characterSlotPrefab == null)
        {
            Debug.LogError("CharacterSlotPrefab is not assigned in the inspector!");
            return;
        }

        // Создаём новые элементы для каждого персонажа
        foreach (var character in CharacterFactory.CharacterStorage.Values) // *** ИЗМЕНЕНО: Итерируем по Character объектам ***
        {
            var characterSlot = Instantiate(characterSlotPrefab, content);
            var cellController = characterSlot.GetComponent<CharacterCellController>();
            if (cellController != null)
            {
                cellController.SetCharacter(character); // *** ИЗМЕНЕНО: Вызываем SetCharacter с объектом Character ***
            }
            else
            {
                Debug.LogError("CharacterCellController component not found on the instantiated prefab!");
            }
        }
    }

    void GenerateCharacter()
    {
        // Генерируем нового персонажа
        CharacterFactory.CreateRandomCharacter();

        // Обновляем панель
        DisplayCharacters();
    }

    void TogglePanel()
    {
        // Переключаем видимость панели
        isPanelVisible = !isPanelVisible;
        characterPanel.SetActive(isPanelVisible);
    }

    private List<CharacterSlot> ConvertToCharacterSlots(List<CharacterCellController> team)
    {
        List<CharacterSlot> slots = new List<CharacterSlot>();

        foreach (var cell in team)
        {
            // Получаем CharacterSlot из CharacterCellController
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
        // Проверяем, что обе команды заполнены
        if (playerTeam.Count == 0 || enemyTeam.Count == 0)
        {
            Debug.LogWarning("Both teams must have characters to start the battle!");
            return;
        }

        // Преобразуем команды из CharacterCellController в CharacterSlot
        List<CharacterSlot> playerSlots = ConvertToCharacterSlots(playerTeam);
        List<CharacterSlot> enemySlots = ConvertToCharacterSlots(enemyTeam);

        // Ищем BattleSetup
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