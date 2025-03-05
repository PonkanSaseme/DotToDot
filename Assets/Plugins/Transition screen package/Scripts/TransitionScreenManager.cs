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
            //�T�O Animator ���v�T����ާ@
            _animator.enabled = false;

            //Ĳ�o�ƥ�A�q����L�t������w����
            FinishedHideEvent?.Invoke();
        }

        public void OnFinishedRevealAnimation()
        {
            //Ĳ�o�ƥ�A�q����L�t������w����
            FinishedRevealEvent?.Invoke();
        }
    }
}
