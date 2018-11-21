namespace VRTK.Examples
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_Keyboard : MonoBehaviour
    {
        private InputField input;

        public void ClickKey(string character)
        {
            input.text += character;
        }

        public void Backspace()
        {
            if (input.text.Length > 0)
            {
                input.text = input.text.Substring(0, input.text.Length - 1);
            }
        }

        //按下回车键
        public void Enter()
        {
            GameObject.Find("UI_Interactions").GetComponent<UIControl>().showRankAfterInput(input.text,Constant.SCORE);//显示排名
            input.text = "";
        }

        private void Start()
        {
            input = GetComponentInChildren<InputField>();
        }
    }
}