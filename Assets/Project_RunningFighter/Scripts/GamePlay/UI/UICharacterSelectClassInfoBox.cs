using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UICharacterSelectClassInfoBox : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_WelcomeBanner;
        [SerializeField]
        private TextMeshProUGUI m_ClassLabel;
        [SerializeField]
        private GameObject m_HideWhenNoClassSelected;
        [SerializeField]
        private Image m_ClassBanner;
        [SerializeField]
        private Image m_Skill1;
        [SerializeField]
        private Image m_Skill2;
        [SerializeField]
        private Image m_Skill3;
        [SerializeField]
        private Image m_ReadyButtonImage;
        [SerializeField]
        private GameObject m_Checkmark;
        [SerializeField]
        [Tooltip("Message shown in the char-select screen. {0} will be replaced with the player's seat number")]
        [Multiline]
        private string m_WelcomeMsg = "Welcome, P{0}!";
        [SerializeField]
        [Tooltip("Format of tooltips. {0} is skill name, {1} is skill description. Html-esque tags allowed!")]
        [Multiline]
        private string m_TooltipFormat = "<b>{0}</b>\n\n{1}";

        private bool m_IsLockedIn = false;

        public void OnSetPlayerNumber(int playerNumber)
        {
            m_WelcomeBanner.text = string.Format(m_WelcomeMsg, (playerNumber + 1));
        }

        public void ConfigureForNoSelection()
        {
            m_HideWhenNoClassSelected.SetActive(false);
            SetLockedIn(false);
        }

        public void SetLockedIn(bool lockedIn)
        {
            m_ReadyButtonImage.color = lockedIn ? Color.green : Color.white;
            m_IsLockedIn = lockedIn;
            m_Checkmark.SetActive(lockedIn);
        }

        public void ConfigureForClass(Data.CharacterClass characterClass)
        {
            m_HideWhenNoClassSelected.SetActive(true);

            m_Checkmark.SetActive(m_IsLockedIn);

            m_ClassLabel.text = characterClass.DisplayedName;
            m_ClassBanner.sprite = characterClass.ClassBannerSprite;

            ConfigureSkillIcon(m_Skill1, characterClass.Skill1);
            ConfigureSkillIcon(m_Skill2, characterClass.Skill2);
            ConfigureSkillIcon(m_Skill3, characterClass.Skill3);
        }

        private void ConfigureSkillIcon(Image iconSlot, Action.GameAction action)
        {
            if (action == null)
            {
                iconSlot.gameObject.SetActive(false);
            }
            else
            {
                iconSlot.gameObject.SetActive(true);
                iconSlot.sprite = action.Config.Icon;
                UITooltipDetector tooltipDetector = iconSlot.GetComponent<UITooltipDetector>();
                if (tooltipDetector)
                {
                    tooltipDetector.SetText(string.Format(m_TooltipFormat, action.Config.DisplayedName, action.Config.Description));
                }
            }
        }
    }
}
