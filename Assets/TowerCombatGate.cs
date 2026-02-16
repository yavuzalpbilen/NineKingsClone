using System.Collections.Generic;
using UnityEngine;

public class TowerCombatGate : MonoBehaviour
{
    [Header("What to disable until combat")]
    [Tooltip("Ateş eden script class isimlerini yaz. Örn: TowerShooter, Shooter, AutoShooter vs.")]
    [SerializeField]
    private string[] shooterTypeNames =
    {
        "TowerShooter",
        "Shooter",
        "AutoShooter",
        "AutoFire",
        "ShootAtEnemy"
    };

    [Tooltip("Fight başlayana kadar kapatılacak componentleri otomatik bulur (bu objede ve child'larda).")]
    [SerializeField] private bool includeChildren = true;

    private BattleManager battle;
    private readonly List<Behaviour> gated = new List<Behaviour>();
    private bool lastState = false;

    private void Awake()
    {
        battle = FindObjectOfType<BattleManager>();
        CacheGatedComponents();
        Apply(battle != null && battle.CombatActive);
    }

    private void CacheGatedComponents()
    {
        gated.Clear();

        Behaviour[] comps = includeChildren
            ? GetComponentsInChildren<Behaviour>(true)
            : GetComponents<Behaviour>();

        for (int i = 0; i < comps.Length; i++)
        {
            Behaviour b = comps[i];
            if (b == null) continue;
            if (b == this) continue;

            string typeName = b.GetType().Name;

            for (int k = 0; k < shooterTypeNames.Length; k++)
            {
                if (typeName == shooterTypeNames[k])
                {
                    gated.Add(b);
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (battle == null)
            battle = FindObjectOfType<BattleManager>();

        bool combat = battle != null && battle.CombatActive;

        if (combat != lastState)
            Apply(combat);
    }

    private void Apply(bool combat)
    {
        lastState = combat;

        // combat başlamadan: disable
        // combat başlayınca: enable
        for (int i = 0; i < gated.Count; i++)
        {
            if (gated[i] != null)
                gated[i].enabled = combat;
        }
    }
}
