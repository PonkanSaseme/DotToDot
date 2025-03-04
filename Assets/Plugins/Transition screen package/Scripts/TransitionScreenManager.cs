using UnityEngine;

namespace TransitionScreenPackage
{
    public class TransitionScreenManager : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        private GameObject _parent;

        public delegate void FinishedReveal();
        public FinishedReveal FinishedRevealEvent;

        public delegate void FinishedHide();
        public FinishedHide FinishedHideEvent;

        private void Awake()
        {
            //先用FindWithTag()尋找parent
            _parent = GameObject.FindWithTag("parent");

            // 如果FindWithTag()找不到，改用FindObjectsOfType<Transform>()
            if (_parent == null)
            {
                Transform[] allObjects = FindObjectsOfType<Transform>(true); //true可找到隱藏物件
                foreach (Transform obj in allObjects)
                {
                    if (obj.CompareTag("parent"))
                    {
                        _parent = obj.gameObject;
                        break;
                    }
                }
            }

            //Debug檢查是否成功找到parent
            if (_parent != null)
            {
                _parent.SetActive(false); //確保轉場開始時parent是關閉的
            }
            else
            {
                Debug.LogError("找不到parent，請確保Tag設為parent");
            }
        }


        public void Reveal()
        {
            _animator.SetTrigger("Reveal");
        }

        public void Hide()
        {
            _animator.SetTrigger("Hide");
        }

        public void OnFinishedHideAnimation()
        {
            //確保parent物件已找到後才執行
            if (_parent != null)
            {
                _parent.SetActive(true);
            }
            else
            {
                Debug.LogError("無法開啟parent，因為_parent為 null");
            }

            //確保 Animator 不影響後續操作
            _animator.enabled = false;

            //觸發事件，通知其他系統轉場已完成
            FinishedHideEvent?.Invoke();
        }

        public void OnFinishedRevealAnimation()
        {
            //觸發事件，通知其他系統轉場已完成
            FinishedRevealEvent?.Invoke();
        }
    }
}
