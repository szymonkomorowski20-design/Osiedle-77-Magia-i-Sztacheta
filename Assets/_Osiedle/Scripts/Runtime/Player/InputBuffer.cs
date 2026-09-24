namespace Osiedle.Player
{
    /// <summary>
    /// Bufor wejścia: zapamiętuje naciśnięcie przycisku na krótki czas (<see cref="Window"/>),
    /// żeby akcja wykonała się, nawet jeśli gracz wcisnął ją odrobinę za wcześnie.
    /// Czysta logika bez sceny, dzięki czemu ma testy.
    /// </summary>
    public class InputBuffer
    {
        float pressedAt = float.NegativeInfinity;

        public float Window { get; set; }

        public InputBuffer(float window)
        {
            Window = window;
        }

        public void Press(float time) => pressedAt = time;

        public bool IsPending(float time) => time - pressedAt <= Window;

        /// <summary>Zwraca true i czyści bufor, jeśli naciśnięcie jest jeszcze ważne.</summary>
        public bool TryConsume(float time)
        {
            if (!IsPending(time)) return false;
            Clear();
            return true;
        }

        public void Clear() => pressedAt = float.NegativeInfinity;
    }
}
