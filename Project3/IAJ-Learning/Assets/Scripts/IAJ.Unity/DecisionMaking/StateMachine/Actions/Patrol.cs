using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class Patrol : IAction
    {
        protected NPC Character { get; set; }
        public Vector3 Point1 { get; set; }
        public Vector3 Point2 { get; set; }

        public Patrol(NPC character, Vector3 point1, Vector3 point2)
        {
            this.Character = character;
            this.Point1 = point1;
            this.Point2 = point2;
        }


        public void Execute()
        {
            if (Character.isStopped())
            {
                Character.StartPathfinding(Point1);
            }
            Vector3 dist1 = Character.gameObject.transform.position - Point1;
            Vector3 dist2 = Character.gameObject.transform.position - Point2;
            if(Math.Abs(dist1.x) <= 0.1 && Math.Abs(dist1.z) <= 0.1)
            {
                Character.StartPathfinding(Point2);
            }else if (Math.Abs(dist2.x) <= 0.1 && Math.Abs(dist2.z) <= 0.1)
            {
                Character.StartPathfinding(Point1);
            }
        }
    }
}
