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
            //����FindWithTag()�M��parent
            _parent = GameObject.FindWithTag("parent");

            // �p�GFindWithTag()�䤣��A���FindObjectsOfType<Transform>()
            if (_parent == null)
            {
                Transform[] allObjects = FindObjectsOfType<Transform>(true); //true�i������ê���
                foreach (Transform obj in allObjects)
                {
                    if (obj.CompareTag("parent"))
                    {
                        _parent = obj.gameObject;
                        break;
                    }
                }
            }

            //Debug�ˬd�O�_���\���parent
            if (_parent != null)
            {
                _parent.SetActive(false); //�T�O����}�l��parent�O������
            }
            else
            {
                Debug.LogError("�䤣��parent�A�нT�OTag�]��parent");
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
            //�T�Oparent����w����~����
            if (_parent != null)
            {
                _parent.SetActive(true);
            }
            else
            {
                Debug.LogError("�L�k�}��parent�A�]��_parent�� null");
            }

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
