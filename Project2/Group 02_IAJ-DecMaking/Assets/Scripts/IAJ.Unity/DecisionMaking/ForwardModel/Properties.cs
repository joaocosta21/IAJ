using System.Linq.Expressions;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class Properties
    {
        public object Mana { get; set; }
        public object MaxMana { get; set; }
        public object HP { get; set; }
        public object ShieldHP { get; set; }
        public object MaxShieldHP { get; set; }
        public object MaxHP { get; set; }
        public object XP { get; set; }
        public object Time { get; set; }
        public object Duration { get; set; }
        public object Money { get; set; }
        public object PreviousMoney { get; set; }
        public object Level { get; set; }
        public object PreviousLevel { get; set; }
        public object Position { get; set; }
    
        public Properties(AutonomousCharacter Character){
            this.Mana = Character.baseStats.Mana;
            this.MaxMana = Character.baseStats.MaxMana;
            this.XP = Character.baseStats.XP;
            this.MaxHP = Character.baseStats.MaxHP;
            this.HP = Character.baseStats.HP;
            this.ShieldHP = Character.baseStats.ShieldHP;
            this.MaxShieldHP = Character.baseStats.MaxShieldHp;
            this.Money = Character.baseStats.Money;
            this.Time = Character.baseStats.Time;
            this.Level = Character.baseStats.Level;
            this.Position = Character.gameObject.transform.position;
            this.Duration = 0.0f;
            this.PreviousLevel = 1;
            this.PreviousMoney = 0;
        }

        public Properties(Properties propertie)
        {
            this.Mana = propertie.Mana;
            this.MaxMana = propertie.MaxMana;
            this.XP = propertie.XP;
            this.MaxHP = propertie.MaxHP;
            this.HP = propertie.HP;
            this.ShieldHP = propertie.ShieldHP;
            this.MaxShieldHP = propertie.MaxShieldHP;
            this.Money = propertie.Money;
            this.Time = propertie.Time;
            this.Level = propertie.Level;
            this.Position = propertie.Position;
            this.Duration = propertie.Duration;
            this.PreviousLevel = propertie.PreviousLevel;
            this.PreviousMoney = propertie.PreviousMoney;
        }

        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    return this.Mana;
                case PropertiesName.MAXMANA:
                    return this.MaxMana;
                case PropertiesName.XP:
                    return this.XP;
                case PropertiesName.MAXHP: 
                    return this.MaxHP;
                case PropertiesName.HP:
                    return this.HP;
                case PropertiesName.ShieldHP:
                    return this.ShieldHP;
                case PropertiesName.MaxShieldHP:
                    return this.MaxShieldHP;
                case PropertiesName.MONEY:
                    return this.Money;
                case PropertiesName.TIME:
                    return this.Time;
                case PropertiesName.LEVEL:
                    return this.Level;
                case PropertiesName.POSITION:
                    return this.Position;
                case PropertiesName.DURATION:
                    return this.Duration;
                case PropertiesName.PreviousLEVEL:
                    return this.PreviousLevel;
                case PropertiesName.PreviousMONEY:
                    return this.PreviousMoney;
                default:
                    return null;
            }
        }

        public void SetProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    this.Mana = value;
                    break;
                case PropertiesName.MAXMANA:
                    this.MaxMana = value;
                    break;
                case PropertiesName.XP:
                    this.XP = value;
                    break;
                case PropertiesName.MAXHP:
                    this.MaxHP = value;
                    break;
                case PropertiesName.HP:
                    this.HP = value;
                    break;
                case PropertiesName.ShieldHP:
                    this.ShieldHP = value;
                    break;
                case PropertiesName.MaxShieldHP:
                    this.MaxShieldHP = value;
                    break;
                case PropertiesName.MONEY:
                    this.Money = value;
                    break;
                case PropertiesName.TIME:
                    this.Time = value;
                    break;
                case PropertiesName.LEVEL:
                    this.Level = value;
                    break;
                case PropertiesName.POSITION:
                    this.Position = value;
                    break;
                case PropertiesName.DURATION:
                    this.Duration = value;
                    break;
                case PropertiesName.PreviousLEVEL:
                    this.PreviousLevel = value;
                    break;
                case PropertiesName.PreviousMONEY:
                    this.PreviousMoney = value;
                    break;
            }
        }
    }
}
