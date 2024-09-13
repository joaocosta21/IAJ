using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

namespace Assets.Scripts.Grid
{
    public interface IConnection
    {
     // The node that this connection came from.
    Node FromNode { get; }

    // The node that this connection leads to.
    Node ToNode { get; }

    // The non-negative cost of this connection.
    float GetCost();
    }
}