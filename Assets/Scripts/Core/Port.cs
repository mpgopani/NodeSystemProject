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

        // Runtime connection - which node's port is this connected to?
        [NonSerialized] public Port connectedPort;

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
        /// Connects two ports together.
        /// </summary>
        public void Connect(Port otherPort)
        {
            if (otherPort == null) return;
            if (portType == otherPort.portType)
            {
                Debug.LogError($"Cannot connect {portType} to {otherPort.portType}");
                return;
            }
            connectedPort = otherPort;
            otherPort.connectedPort = this;
        }

        public void Disconnect()
        {
            if (connectedPort != null)
            {
                connectedPort.connectedPort = null;
            }
            connectedPort = null;
        }
    }
}
