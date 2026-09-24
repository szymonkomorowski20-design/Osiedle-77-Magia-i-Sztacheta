using Osiedle.Player;
using UnityEngine;

namespace Osiedle.UI
{
    /// <summary>
    /// Tymczasowy podgląd dla testów (lewy górny róg): HP, Złom, Moc, dash, numer ciosu, komunikat o śmierci.
    /// Prawdziwy HUD (butelki oranżady, wskaźnik Unitrąby) przyjdzie w osobnym etapie, z tekstami z Texts_PL.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        const int Width = 420;
        const int LineHeight = 22;
        const int Margin = 10;

        [SerializeField] PlayerResources resources;
        [SerializeField] PlayerDash dash;
        [SerializeField] PlayerMelee melee;
        [SerializeField] PlayerDeath death;

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
            Label(ref line, death != null && death.IsDead
                ? "KUBA WRACA NA TRZEPAK — wciśnij R, żeby spróbować jeszcze raz"
                : "R — restart walki");
        }

        void Label(ref int line, string text)
        {
            GUI.Label(new Rect(Margin, Margin + line * LineHeight, Width, LineHeight), text, style);
            line++;
        }
    }
}
