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

        private void Awake()
        {
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
