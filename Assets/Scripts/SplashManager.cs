using System.Collections;
using UnityEngine;

public class SplashManager : MonoBehaviour
{
    public LoadingPanel loadingPanel;

    private IEnumerator Start()
    {
        loadingPanel.gameObject.SetActive(true);
        yield return null;
    }
}
