using UnityEngine;

namespace NodeSystem.Nodes
{
    /// <summary>
    /// Compares two values and determines flow based on result.
    /// </summary>
    public class CompareNode : Node
    {
        public enum ComparisonType { Equal, NotEqual, Greater, Less, GreaterOrEqual, LessOrEqual }

        [SerializeField] private ComparisonType comparisonType = ComparisonType.Equal;
        [SerializeField] private float valueA = 0f;
        [SerializeField] private float valueB = 0f;
        [SerializeField] private string trueNodeId = "";
        [SerializeField] private string falseNodeId = "";

        private bool lastComparisonResult = false;

        public override void Execute()
        {
            lastComparisonResult = PerformComparison();
            Debug.Log($"[CompareNode] {valueA} {comparisonType} {valueB} = {lastComparisonResult}");
        }

        private bool PerformComparison()
        {
            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    return Mathf.Approximately(valueA, valueB);
                case ComparisonType.NotEqual:
                    return !Mathf.Approximately(valueA, valueB);
                case ComparisonType.Greater:
                    return valueA > valueB;
                case ComparisonType.Less:
                    return valueA < valueB;
                case ComparisonType.GreaterOrEqual:
                    return valueA >= valueB;
                case ComparisonType.LessOrEqual:
                    return valueA <= valueB;
                default:
                    return false;
            }
        }

        public override Node GetNextNode()
        {
            if (parentGraph == null) return null;
            string targetId = lastComparisonResult ? trueNodeId : falseNodeId;
            foreach (var n in parentGraph.GetAllNodes())
            {
                if (n.nodeId == targetId) return n;
            }
            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            nodeName = "Compare Values";

            if (outputPorts.Count == 0)
            {
                AddOutputPort("Result", typeof(bool));
            }
        }

        public void SetComparison(float a, float b, ComparisonType type)
        {
            valueA = a;
            valueB = b;
            comparisonType = type;
        }
    }
}
