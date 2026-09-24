using Osiedle.Player;
using UnityEngine;

namespace Osiedle.UI
{
    /// <summary>
    /// Tymczasowy podgląd dla testów (lewy górny róg): HP, Złom, Moc, dash i numer ciosu.
    /// Prawdziwy HUD (butelki oranżady, wskaźnik Unitrąby) przyjdzie w osobnym etapie, z tekstami z Texts_PL.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        const int Width = 260;
        const int LineHeight = 22;
        const int Margin = 10;

        [SerializeField] PlayerResources resources;
        [SerializeField] PlayerDash dash;
        [SerializeField] PlayerMelee melee;

        GUIStyle style;

        void OnGUI()
        {
            if (resources == null) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 16 };

            var health = resources.Health;
            int line = 0;
            Label(ref line, $"HP: {health.Current:0} / {health.Max:0}");
            Label(ref line, $"Złom: {resources.Scrap} / {resources.MaxScrap}");
            Label(ref line, $"Moc: {resources.Power:0} / {resources.MaxPower:0}");
            if (dash != null && dash.Charges != null)
                Label(ref line, $"Dash: {dash.Charges.Charges} / {dash.Charges.MaxCharges}");
            if (melee != null)
                Label(ref line, melee.IsAttacking ? $"Cios: {melee.CurrentStep + 1}" : "Cios: —");
        }

        void Label(ref int line, string text)
        {
            GUI.Label(new Rect(Margin, Margin + line * LineHeight, Width, LineHeight), text, style);
            line++;
        }
    }
}
