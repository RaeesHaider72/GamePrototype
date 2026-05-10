using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    public Image loadingImage;

    public float maxTime = 5f;

    private float activeTime;

    void Update()
    {
        if (activeTime < maxTime)
        {
            activeTime += Time.deltaTime;

            loadingImage.fillAmount = activeTime / maxTime;

            if(activeTime >= maxTime)
            {
                loadingImage.fillAmount = 1f;
                activeTime = maxTime;
                SceneManager.LoadScene("GamePlay");
            }
        }
    }
}