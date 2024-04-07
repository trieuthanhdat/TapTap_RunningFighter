using UnityEngine;

namespace Project_RunningFighter.Data
{
    [CreateAssetMenu]
    public class VisualizationConfig : ScriptableObject
    {
        [Header("Animation Boolean")]
        [SerializeField] string m_StaticB = "Static_b";
        [SerializeField] string m_Jump    = "Jump_b";
        [SerializeField] string m_DeathStateTrigger = "Death_b";
        [Header("Animation Float/Int")]
        [SerializeField] string m_SpeedVariable = "Speed_f";
        [SerializeField] string m_DeadType      = "DeathType_int";


        [Header("Animation Triggers")]
        [SerializeField] string m_JumpTrigger       = "Jump_trig";
        [SerializeField] string m_GroundedTrigger   = "Grounded";
        [SerializeField] string m_AliveStateTrigger = "Crouch_up";

        [Header("Other Animation Variables")]

        [Tooltip("Tag that should be on the \"do nothing\" default nodes of each animator layer")]
        [SerializeField] string m_BaseNodeTag = "BaseNode";
        [Header("Animation Speeds")]
        public float SpeedDead = 0;
        public float SpeedIdle = 0;
        public float SpeedNormal = 1;
        public float SpeedUncontrolled = 0; // no leg movement; character appears to be sliding helplessly
        public float SpeedSlowed = 2; // hyper leg movement (character appears to be working very hard to move very little)
        public float SpeedHasted = 1.5f;
        public float SpeedWalking = 0.1f;
        public float SpeedRunning = 1;

        [Header("Associated Resources")]
        [Tooltip("Prefab for the Target Reticule used by this Character")]
        public GameObject TargetReticule;

        [Tooltip("Material to use when displaying a friendly target reticule (e.g. green color)")]
        public Material ReticuleFriendlyMat;

        [Tooltip("Material to use when displaying a hostile target reticule (e.g. red color)")]
        public Material ReticuleHostileMat;


        // These are maintained by our OnValidate(). Code refers to these hashed values, not the string versions!
        [SerializeField][HideInInspector] public int AliveStateTriggerID;
        [SerializeField][HideInInspector] public int DeadStateTriggerID;
        [SerializeField][HideInInspector] public int StaticTypeBooleanID;
        [SerializeField][HideInInspector] public int SpeedVariableID;
        [SerializeField][HideInInspector] public int BaseNodeTagID;

        void OnValidate()
        {
            AliveStateTriggerID = Animator.StringToHash(m_AliveStateTrigger);
            DeadStateTriggerID = Animator.StringToHash(m_DeathStateTrigger);
            StaticTypeBooleanID = Animator.StringToHash(m_StaticB);

            SpeedVariableID = Animator.StringToHash(m_SpeedVariable);
            BaseNodeTagID = Animator.StringToHash(m_BaseNodeTag);
        }
    }
}
