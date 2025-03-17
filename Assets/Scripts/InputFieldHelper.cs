using UnityEngine;

public class InputFieldHelper : MonoBehaviour
{
    public void ShowKeyboard()
    {
        // 呼叫JavaScript程式碼來顯示鍵盤
        Application.ExternalCall("ShowKeyboard");
    }
}
