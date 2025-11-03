using UnityEngine;

namespace CasualBonusSlot.Views
{
    public class ChanceView : MonoBehaviour
    {
        [SerializeField] private Animator chanceAnimator;

        /// <summary>
        /// 激熱カットイン演出を再生
        /// </summary>
        public void PlayCutIn()
        {
            chanceAnimator.SetTrigger("Trigger");
        }
    }
}
