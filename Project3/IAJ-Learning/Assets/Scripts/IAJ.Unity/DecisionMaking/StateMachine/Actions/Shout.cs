using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class Shout : IAction
    {
        protected NPC Character { get; set; }


        public Shout(NPC character)
        {
            this.Character = character;
        }

        public void Execute()
        {
            var orcs = GameObject.FindGameObjectsWithTag("Orc");
            foreach(var orc in orcs)
            {
                if(orc.name == Character.name)
                {
                    continue;
                }
                var res = orc.transform.position - Character.transform.position;
                if (Mathf.Sqrt(res.x*res.x  + res.z*res.z) <= 500) // Shouting distance
                {
                    var monster = orc.GetComponent<Monster>();
                    monster.HeardShout = true;
                    monster.ShoutPosition = Character.transform.position;
                }
            }
            Character.GetComponent<Orc>().GetComponent<AudioSource>().Play();
            
            
            // Play audio and visual
        }
    }
}
