namespace RL
{
    public class DeepQLState
    {
        public int HPState { get; set; }
        public int ManaState { get; set; }
        public int LevelState { get; set; }
        public int XPState { get; set; }
        public int GoldState { get; set; }

        public DeepQLState(int hp, int mana, int level,int xp, int gold)
        {
            HPState = hp;
            ManaState = mana;
            LevelState = level;
            XPState = xp;
            GoldState = gold;
        }

        public override string ToString()
        {
            return $"HPState: {HPState}; ManaState: {ManaState}; LevelState: {LevelState}";
        }
    }
}
