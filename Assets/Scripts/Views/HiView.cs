using UnityEngine;

namespace CasualBonusSlot.Views
{
    public class HiView : MonoBehaviour
    {
        [SerializeField] private Animator hiAnimator;

        /// <summary>
        /// 通常カットイン演出を再生
        /// </summary>
        public void PlayCutIn()
        {
            hiAnimator.SetTrigger("Trigger");
        }
    }
}
