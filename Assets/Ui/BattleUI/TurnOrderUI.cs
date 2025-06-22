using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderUI : MonoBehaviour
{
    [SerializeField] private Text currentTurnText; // ����� ��� �������� ����
    [SerializeField] private Transform futureTurnsContainer; // ��������� ��� ������� �����
    [SerializeField] private GameObject turnEntryPrefab; // ������ ��� ����������� ��������� � ������
    [SerializeField] private Color playerColor = Color.green; // ���� ��� �������
    [SerializeField] private Color enemyColor = Color.red; // ���� ��� ������

    private Dictionary<Character, Color> characterColors = new();

    public void Initialize(List<Character> allCharacters, Dictionary<Character, Team> characterTeams)
    {
        // ������������� ����� ��� ����������
        foreach (var character in allCharacters)
        {
            characterColors[character] = characterTeams[character] == Team.Player ? playerColor : enemyColor;
        }
    }

    public void UpdateTurnOrder(Character currentCharacter, List<Character> futureTurns)
    {
        // ��������� ������� ���
        currentTurnText.text = $"������� ���: {currentCharacter.Name}";
        currentTurnText.color = characterColors[currentCharacter];

        // ������� ������ ������� �����
        foreach (Transform child in futureTurnsContainer)
        {
            Destroy(child.gameObject);
        }

        // ��������� ������� ����
        foreach (var character in futureTurns)
        {
            var entry = Instantiate(turnEntryPrefab, futureTurnsContainer);
            var text = entry.GetComponent<Text>();
            text.text = character.Name;
            text.color = characterColors[character];
        }
    }
}