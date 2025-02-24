namespace RL
{
    public class TQLState
    {
        public int HPState { get; set; }
        public int ManaState { get; set; }
        public int LevelState { get; set; }

        public TQLState(int hp, int mana, int level)
        {
            HPState = hp;
            ManaState = mana;
            LevelState = level;
        }

        public override string ToString()
        {
            return $"HPState: {HPState}; ManaState: {ManaState}; LevelState: {LevelState}";
        }

        // Static method to parse a TQLState from a string
        public static TQLState Parse(string stateString)
        {
            // Example format: "HPState: 100; ManaState: 50; LevelState: 3"
            var parts = stateString.Split(';');

            // Extract and parse each part
            int hp = int.Parse(parts[0].Split(':')[1].Trim());
            int mana = int.Parse(parts[1].Split(':')[1].Trim());
            int level = int.Parse(parts[2].Split(':')[1].Trim());

            // Create and return a new TQLState object
            return new TQLState(hp, mana, level);
        }

        // Equality and HashCode to use this as a Dictionary key
        public override bool Equals(object obj)
        {
            if (obj is TQLState other)
            {
                return HPState == other.HPState && ManaState == other.ManaState && LevelState == other.LevelState;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (HPState, ManaState, LevelState).GetHashCode();
        }
    }
}
