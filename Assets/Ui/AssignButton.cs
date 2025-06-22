using UnityEngine;
using UnityEngine.UI;

public class AssignButton : MonoBehaviour
{
    public CharacterSlot targetSlot; 

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnAssignButtonClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnAssignButtonClicked);
    }

    private void OnAssignButtonClicked()
    {
        if (targetSlot == null)
        {
            Debug.LogError("Target slot is not assigned in AssignButton.");
            return;
        }

        if (AssignmentManager.Instance == null)
        {
            Debug.LogError("AssignmentManager.Instance is null. Make sure AssignmentManager is in the scene.");
            return;
        }

        AssignmentManager.Instance.StartAssignment(targetSlot);
    }
}