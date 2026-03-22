using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Branches execution based on an input boolean condition.
    /// </summary>
    public class BranchNode : Node
    {
        [SerializeField] private Node ifTrueNode;
        [SerializeField] private Node ifFalseNode;
        [SerializeField] private bool condition = false;

        public override void Execute()
        {
            var inputVal = GetInputValue("Condition");
            if (inputVal is bool b)
            {
                condition = b;
            }

            Debug.Log($"[BranchNode] Condition = {condition}, routing to {(condition ? ifTrueNode?.nodeName : ifFalseNode?.nodeName)}");
        }

        public override Node GetNextNode()
        {
            return condition ? ifTrueNode : ifFalseNode;
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
