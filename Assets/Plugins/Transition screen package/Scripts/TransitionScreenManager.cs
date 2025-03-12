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
        public event FinishedHide FinishedHideEvent;

        public delegate void FinishedRule();
        public event FinishedRule FinishedRuleEvent;

        private void Awake()
        {
            // 如果需要初始化
        }

        public void Reveal() //淡入trigger
        {
            _animator.SetTrigger("Reveal");
        }

        public void Hide() //淡出trigger
        {
            _animator.SetTrigger("Hide");
        }

        public void Rule() //規則頁trigger
        {
            _animator.SetTrigger("Rule");
        }

        public void OnFinishedHideAnimation()
        {
            Debug.Log("Finished Hide Animation");
            // 確保 Animator 不影響後續操作
            _animator.enabled = false;

            // 觸發事件，通知其他系統轉場已完成
            FinishedHideEvent?.Invoke();
        }

        public void OnFinishedRevealAnimation()
        {
            Debug.Log("Finished Reveal Animation");
            // 觸發事件，通知其他系統轉場已完成
            FinishedRevealEvent?.Invoke();
        }

        public void OnFinishedRuleAnimation()
        {
            Debug.Log("Finished Rule Animation");
            // 觸發事件，通知其他系統規則動畫已完成
            FinishedRuleEvent?.Invoke();
        }
    }
}