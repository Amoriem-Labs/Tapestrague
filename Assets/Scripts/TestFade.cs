using UnityEngine;

public class TestFade : MonoBehaviour
{
    private void Start(){
        Invoke("TestFading", 3f);
    }
    public void TestFading()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToBlack("SampleScene");
        }
        else
        {
            Debug.LogError("SceneFader instance not found in the scene!");
        }
    }
}
