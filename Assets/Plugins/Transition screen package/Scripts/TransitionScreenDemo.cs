using System.Collections.Generic;
using TransitionScreenPackage.Data;
using UnityEngine;
using UnityEngine.UI;

namespace TransitionScreenPackage.Demo
{
    public class TransitionScreenDemo : MonoBehaviour
    {
        [SerializeField] private TransitionScreenType _selectedType;
        [SerializeField] private TransitionScreenVersion _selectedVersion;
        [SerializeField] public GameObject _RuleScene;

        [Space]
        [Header("DO NOT CHANGE THESE!")]
        [SerializeField] private List<TransitionScreenObject> _transitionScreens;

        private TransitionScreenManager _currentTransitionScreen;

        //新增IsTransitioning 屬性，預設為 false
        public bool IsTransitioning { get; private set; } = false;

        private void SpawnSelectedTransitionScreen()
        {
            foreach (TransitionScreenObject transition in _transitionScreens)
            {
                if (transition.Type.Equals(_selectedType))
                {
                    // 清除舊的 TransitionScreen
                    if (_currentTransitionScreen != null)
                    {
                        _currentTransitionScreen.FinishedRevealEvent -= OnTransitionScreenRevealed;
                        _currentTransitionScreen.FinishedHideEvent -= OnTransitionScreenHidden;
                        _currentTransitionScreen.FinishedRuleEvent -= OnTransitionScreenRule;
                        Destroy(_currentTransitionScreen.gameObject);
                    }

                    //轉場開始，設為 true
                    IsTransitioning = true;

                    // 產生新的 TransitionScreen
                    GameObject instantiatedTransitionScreen = Instantiate(transition.GetVersion(_selectedVersion).PrefabObject, transform);
                    _currentTransitionScreen = instantiatedTransitionScreen.GetComponent<TransitionScreenManager>();

                    // 訂閱事件
                    _currentTransitionScreen.FinishedRevealEvent += OnTransitionScreenRevealed;
                    _currentTransitionScreen.FinishedHideEvent += OnTransitionScreenHidden;
                    _currentTransitionScreen.FinishedRuleEvent += OnTransitionScreenRule;

                    //啟動 Reveal (轉場動畫開始)
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
            //轉場 Reveal 動畫結束，開始 Hide (遮罩隱藏)
            _currentTransitionScreen.Hide();
        }

        private void OnTransitionScreenHidden()
        {
            //轉場 Hide 動畫結束，開始 Rule(播放規則動畫)
            _currentTransitionScreen.Rule();
        }
        private void OnTransitionScreenRule()
        {
            //Rule 動畫結束，標記為false，動畫已完成
            IsTransitioning = false;
        }
    }
}