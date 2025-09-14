using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject startScene;
    public GameObject enterSizeScene;

    public GameObject smallPanel;
    public GameObject mediumPanel;
    public GameObject largePanel;

    public NumberInputHandler NumberInputHandler;

    public GameObject detailButton;
    public GameObject backButton;

    public Button scanButton;
    public RawImage photoPreview;


    public void enterSize()
    {
        startScene.gameObject.SetActive(false);
        enterSizeScene.gameObject.SetActive(true);
    }

    public void DetailButton()
    {
        float userSize = NumberInputHandler.size;

        if (userSize > 0)
        {
            if (userSize < 7)
            {
                smallPanel.gameObject.SetActive(true);
            }
            else if (userSize < 9)
            {
                mediumPanel.gameObject.SetActive(true);
            }
            else
            {
                largePanel.gameObject.SetActive(true);
            }
            detailButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);
        }
    }
    public void BackButton()
    {
        if(smallPanel.gameObject.activeSelf)
        {
            smallPanel.gameObject.SetActive(false);
        }
        else if (mediumPanel.gameObject.activeSelf)
        {
            mediumPanel.gameObject.SetActive(false);
        }
        else if (largePanel.gameObject.activeSelf)
        {
            largePanel.gameObject.SetActive(false);
        }
        backButton.SetActive(false);
        detailButton.SetActive(true);
    }
}
