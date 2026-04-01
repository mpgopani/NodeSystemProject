using UnityEngine;
using System;
using System.Collections.Generic;

namespace NodeSystem
{
    /// <summary>
    /// Represents a connection point on a node.
    /// Ports carry data between nodes in the graph.
    /// </summary>
    [System.Serializable]
    public class Port
    {
        public string portId = "";
        public string portName = "";
        public PortType portType = PortType.Output;
        public System.Type dataType = typeof(object);

        // Runtime connections - this allows 1-to-Many output routing!
        [NonSerialized] public List<Port> connections = new List<Port>();

        public enum PortType { Input, Output }

        public Port() { }

        public Port(string name, PortType type, System.Type dataType)
        {
            portId = System.Guid.NewGuid().ToString();
            portName = name;
            portType = type;
            this.dataType = dataType;
        }

        /// <summary>
        /// Connects two ports together (supports 1-to-many outputs).
        /// </summary>
        public void Connect(Port otherPort)
        {
            if (otherPort == null) return;
            if (portType == otherPort.portType)
            {
                Debug.LogError($"Cannot connect {portType} to {otherPort.portType}");
                return;
            }
            
            if (!connections.Contains(otherPort))
                connections.Add(otherPort);
                
            if (!otherPort.connections.Contains(this))
                otherPort.connections.Add(this);
        }

        public void Disconnect(Port specificPort = null)
        {
            if (specificPort == null)
            {
                // Disconnect from ALL
                foreach (var p in new List<Port>(connections))
                {
                    p.connections.Remove(this);
                }
                connections.Clear();
            }
            else
            {
                connections.Remove(specificPort);
                specificPort.connections.Remove(this);
            }
        }
    }
}
