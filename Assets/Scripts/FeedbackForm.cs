using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FeedbackForm : MonoBehaviour
{
    public static FeedbackForm Instance;

    public TMP_InputField enterID;
    public string Reward;
    public string TimeStamp;

    private string formUrl = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSffFmnIhgfB_xfGhevPa8CM_w4uvnyHX3NZweBSOxY9uIbrZQ/formResponse";

    public void Start()
    {
        Instance = this;
    }
    public void SubmitFeedback()
    {
        TimeStamp = PlayerPrefs.GetString("Timestamp");
        Reward = PlayerPrefs.GetString("FinalReward");
        StartCoroutine(Post(enterID.text, Reward, TimeStamp));
    }

    private IEnumerator Post(string idName,string Reward,string TimeStamp)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.452713622", idName);
        form.AddField("entry.1043978043", Reward);
        form.AddField("entry.1386421183", TimeStamp);

        using (UnityWebRequest www = UnityWebRequest.Post(formUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Feedback submitted successfully.");
            }
            else
            {
                Debug.LogError("Error in feedback submission: " + www.error);
            }
        }
    }
}
