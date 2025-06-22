using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] private Text currentTurnText; // Текст для текущего хода
    [SerializeField] private Transform futureTurnsContainer; // Контейнер для будущих ходов
    [SerializeField] private GameObject turnEntryPrefab; // Префаб для отображения персонажа в списке
    [SerializeField] private Color playerColor = Color.green; // Цвет для игроков
    [SerializeField] private Color enemyColor = Color.red; // Цвет для врагов

    private Dictionary<Character, Color> characterColors = new();

    public void Initialize(List<Character> allCharacters, Dictionary<Character, Team> characterTeams)
    {
        // Устанавливаем цвета для персонажей
        foreach (var character in allCharacters)
        {
            characterColors[character] = characterTeams[character] == Team.Player ? playerColor : enemyColor;
        }
    }

    public void UpdateTurnOrder(Character currentCharacter, List<Character> futureTurns)
    {
        // Обновляем текущий ход
        currentTurnText.text = $"Текущий ход: {currentCharacter.Name}";
        currentTurnText.color = characterColors[currentCharacter];

        // Очищаем список будущих ходов
        foreach (Transform child in futureTurnsContainer)
        {
            Destroy(child.gameObject);
        }

        // Добавляем будущие ходы
        foreach (var character in futureTurns)
        {
            var entry = Instantiate(turnEntryPrefab, futureTurnsContainer);
            var text = entry.GetComponent<Text>();
            text.text = character.Name;
            text.color = characterColors[character];
        }
    }
}