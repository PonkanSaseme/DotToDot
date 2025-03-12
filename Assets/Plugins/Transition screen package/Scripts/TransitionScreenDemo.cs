using System.Collections.Generic;
using TransitionScreenPackage.Data;
using UnityEngine;

namespace TransitionScreenPackage.Demo
{
    public class TransitionScreenDemo : MonoBehaviour
    {
        [SerializeField] private TransitionScreenType _selectedType;
        [SerializeField] private TransitionScreenVersion _selectedVersion;

        [Space]
        [Header("DO NOT CHANGE THESE!")]
        [SerializeField] private List<TransitionScreenObject> _transitionScreens;

        private TransitionScreenManager _currentTransitionScreen;

        //�s�WIsTransitioning �ݩʡA�w�]�� false
        public bool IsTransitioning { get; private set; } = false;

        private void SpawnSelectedTransitionScreen()
        {
            foreach (TransitionScreenObject transition in _transitionScreens)
            {
                if (transition.Type.Equals(_selectedType))
                {
                    // �M���ª� TransitionScreen
                    if (_currentTransitionScreen != null)
                    {
                        _currentTransitionScreen.FinishedRevealEvent -= OnTransitionScreenRevealed;
                        _currentTransitionScreen.FinishedHideEvent -= OnTransitionScreenHidden;
                        _currentTransitionScreen.FinishedRuleEvent -= OnTransitionScreenRule;
                        Destroy(_currentTransitionScreen.gameObject);
                    }

                    //����}�l�A�]�� true
                    IsTransitioning = true;

                    // ���ͷs�� TransitionScreen
                    GameObject instantiatedTransitionScreen = Instantiate(transition.GetVersion(_selectedVersion).PrefabObject, transform);
                    _currentTransitionScreen = instantiatedTransitionScreen.GetComponent<TransitionScreenManager>();

                    // �q�\�ƥ�
                    _currentTransitionScreen.FinishedRevealEvent += OnTransitionScreenRevealed;
                    _currentTransitionScreen.FinishedHideEvent += OnTransitionScreenHidden;
                    _currentTransitionScreen.FinishedHideEvent += OnTransitionScreenRule;

                    //�Ұ� Reveal (����ʵe�}�l)
                    _currentTransitionScreen.Reveal();
                    break;
                }
            }
        }

        private void OnEnable()
        {
            SpawnSelectedTransitionScreen();
        }

        private void OnTransitionScreenRevealed()
        {
            //���?Reveal �ʵe�����A�}�l Hide (�B�n����)
            _currentTransitionScreen.Hide();
        }

        private void OnTransitionScreenHidden()
        {
            //���?Hide �ʵe�����A�аO��false�A�ʵe�w����
            _currentTransitionScreen.Rule();
        }
        private void OnTransitionScreenRule()
        {
            IsTransitioning = false;
        }
    }
}
