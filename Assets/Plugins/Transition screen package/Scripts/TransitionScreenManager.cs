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
        }


        public void Reveal() //�H�Jtrigger
        {
            _animator.SetTrigger("Reveal");
        }

        public void Hide() //�H�Xtrigger
        {
            _animator.SetTrigger("Hide");
        }

        public void Rule() //�W�h��trigger
        {
            _animator.SetTrigger("Rule");
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

        public void OnFinishedRuleAnimation()
        {
            //Ĳ�o�ƥ�A�q����L�t�γW�h�ʵe�w����
            FinishedRuleEvent?.Invoke();
        }
    }
}
