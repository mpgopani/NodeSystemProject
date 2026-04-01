using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Branches execution based on an input boolean condition.
    /// </summary>
    public class BranchNode : Node
    {
        [SerializeField] private string ifTrueNodeId = "";
        [SerializeField] private string ifFalseNodeId = "";
        [SerializeField] private bool condition = false;

        public override void Execute()
        {
            var inputVal = GetInputValue("Condition");
            if (inputVal is bool b)
            {
                condition = b;
            }

            Debug.Log($"[BranchNode] Condition = {condition}, routing to {(condition ? ifTrueNodeId : ifFalseNodeId)}");
        }

        public override Node GetNextNode()
        {
            if (parentGraph == null) return null;
            string targetId = condition ? ifTrueNodeId : ifFalseNodeId;
            foreach (var n in parentGraph.GetAllNodes())
            {
                if (n.nodeId == targetId) return n;
            }
            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Branch";

            if (inputPorts.Count == 0)
            {
                AddInputPort("Condition", typeof(bool));
            }
        }
    }
}
