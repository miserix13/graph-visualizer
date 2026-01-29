using System.Collections.Generic;

namespace GraphVisualizer
{
    public interface IGraphLayout
    {
        List<Vertex> vertices { get; }
        void CalculateLayout(Graph graph);
    }

    public class Vertex
    {
        public Node node { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }

        public Vertex(Node node, float x, float y, float width, float height)
        {
            this.node = node;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public class SimpleTreeLayout : IGraphLayout
    {
        public List<Vertex> vertices { get; private set; }

        public SimpleTreeLayout()
        {
            vertices = new List<Vertex>();
        }

        public void CalculateLayout(Graph graph)
        {
            vertices.Clear();

            if (graph == null || graph.nodes.Count == 0)
                return;

            // Simple tree layout algorithm
            var nodesByDepth = new Dictionary<int, List<Node>>();
            
            foreach (var node in graph)
            {
                int depth = node.depth;
                if (!nodesByDepth.ContainsKey(depth))
                {
                    nodesByDepth[depth] = new List<Node>();
                }
                nodesByDepth[depth].Add(node);
            }

            float verticalSpacing = 0.2f;
            float horizontalSpacing = 0.15f;

            foreach (var kvp in nodesByDepth)
            {
                int depth = kvp.Key;
                List<Node> nodesAtDepth = kvp.Value;
                
                float x = depth * horizontalSpacing;
                float startY = (1.0f - (nodesAtDepth.Count * verticalSpacing)) / 2.0f;

                for (int i = 0; i < nodesAtDepth.Count; i++)
                {
                    float y = startY + (i * verticalSpacing);
                    vertices.Add(new Vertex(nodesAtDepth[i], x, y, 0.1f, 0.08f));
                }
            }
        }
    }
}
