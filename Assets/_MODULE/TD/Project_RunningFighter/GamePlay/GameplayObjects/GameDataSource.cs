using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Data;
using Project_RunningFighter.Gameplay.Action;
using UnityEngine;

public class GameDataSource : MonoSingleton<GameDataSource>
{
    [Header("Character classes")]
    [Tooltip("All CharacterClass data should be slotted in here")]
    [SerializeField]
    private CharacterClass[] m_CharacterData;

    Dictionary<CharacterTypeEnum, CharacterClass> m_CharacterDataMap;

    //Actions that are directly listed here will get automatically assigned ActionIDs and they don't need to be a part of m_ActionPrototypes array
    [Header("Common action prototypes")]
    [SerializeField]
    GameAction m_GeneralChaseActionPrototype;

    [SerializeField]
    GameAction m_GeneralTargetActionPrototype;

    [SerializeField]
    GameAction m_Emote1ActionPrototype;

    [SerializeField]
    GameAction m_Emote2ActionPrototype;

    [SerializeField]
    GameAction m_Emote3ActionPrototype;

    [SerializeField]
    GameAction m_Emote4ActionPrototype;

    [SerializeField]
    GameAction m_ReviveActionPrototype;

    [SerializeField]
    GameAction m_StunnedActionPrototype;

    [SerializeField]
    GameAction m_DropActionPrototype;

    [SerializeField]
    GameAction m_PickUpActionPrototype;

    [Tooltip("All Action prototype scriptable objects should be slotted in here")]
    [SerializeField]
    private GameAction[] m_ActionPrototypes;

    public GameAction GeneralChaseActionPrototype => m_GeneralChaseActionPrototype;

    public GameAction GeneralTargetActionPrototype => m_GeneralTargetActionPrototype;

    public GameAction Emote1ActionPrototype => m_Emote1ActionPrototype;

    public GameAction Emote2ActionPrototype => m_Emote2ActionPrototype;

    public GameAction Emote3ActionPrototype => m_Emote3ActionPrototype;

    public GameAction Emote4ActionPrototype => m_Emote4ActionPrototype;

    public GameAction ReviveActionPrototype => m_ReviveActionPrototype;

    public GameAction StunnedActionPrototype => m_StunnedActionPrototype;

    public GameAction DropActionPrototype => m_DropActionPrototype;
    public GameAction PickUpActionPrototype => m_PickUpActionPrototype;

    List<GameAction> m_AllActions;

    public GameAction GetActionPrototypeByID(GameActionID index)
    {
        return m_AllActions[index.ID];
    }

    public bool TryGetActionPrototypeByID(GameActionID index, out GameAction action)
    {
        for (int i = 0; i < m_AllActions.Count; i++)
        {
            if (m_AllActions[i].ActionID == index)
            {
                action = m_AllActions[i];
                return true;
            }
        }

        action = null;
        return false;
    }

    //Contents of the CharacterData list, indexed by CharacterType for convenience.
    public Dictionary<CharacterTypeEnum, CharacterClass> CharacterDataByType
    {
        get
        {
            if (m_CharacterDataMap == null)
            {
                m_CharacterDataMap = new Dictionary<CharacterTypeEnum, CharacterClass>();
                foreach (CharacterClass data in m_CharacterData)
                {
                    if (m_CharacterDataMap.ContainsKey(data.CharacterType))
                    {
                        throw new System.Exception($"Duplicate character definition detected: {data.CharacterType}");
                    }
                    m_CharacterDataMap[data.CharacterType] = data;
                }
            }
            return m_CharacterDataMap;
        }
    }

    private void Awake()
    {
        BuildActionIDs();
    }

    void BuildActionIDs()
    {
        var uniqueActions = new HashSet<GameAction>(m_ActionPrototypes);
        uniqueActions.Add(GeneralChaseActionPrototype);
        uniqueActions.Add(GeneralTargetActionPrototype);
        uniqueActions.Add(Emote1ActionPrototype);
        uniqueActions.Add(Emote2ActionPrototype);
        uniqueActions.Add(Emote3ActionPrototype);
        uniqueActions.Add(Emote4ActionPrototype);
        uniqueActions.Add(ReviveActionPrototype);
        uniqueActions.Add(StunnedActionPrototype);
        uniqueActions.Add(DropActionPrototype);
        uniqueActions.Add(PickUpActionPrototype);

        m_AllActions = new List<GameAction>(uniqueActions.Count);

        int i = 0;
        foreach (var uniqueAction in uniqueActions)
        {
            uniqueAction.ActionID = new GameActionID { ID = i };
            m_AllActions.Add(uniqueAction);
            i++;
        }
    }
}