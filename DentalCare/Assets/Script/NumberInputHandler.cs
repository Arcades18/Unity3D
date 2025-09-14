using UnityEngine;
using TMPro;
public class NumberInputHandler : MonoBehaviour
{
    public TMP_InputField numberInputField;
    public float size;
    public GameObject enterSizePanel;

    public GameObject smallPrefab;
    public GameObject mediumPrefab;
    public GameObject largePrefab;

    public GameObject detailButton;

    public void OnSubmit()
    {
        if (float.TryParse(numberInputField.text, out float result))
        {
            size = result;
            Debug.Log("User entered number: " + size);
            enterSizePanel.gameObject.SetActive(false);
            SpawnPrefab();

        }
        else
        {
            Debug.LogWarning("Invalid number input!");
        }
    }

    private void SpawnPrefab()
    {
        if (size != 0)
        {

            float userSize = size;

            if (userSize < 7)
            {
                smallPrefab.gameObject.SetActive(true);
            }
            else if (userSize < 9)
            {
                mediumPrefab.gameObject.SetActive(true);
            }
            else
            {
                largePrefab.gameObject.SetActive(true);
            }
            detailButton.SetActive(true);
        }
    }
}

