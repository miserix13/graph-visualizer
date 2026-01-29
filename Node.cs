using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GraphVisualizer
{
    public class Node
    {
        public object content { get; private set; }
        public float weight { get; set; }
        public bool active { get; private set; }
        public Node parent { get; private set; }
        public IList<Node> children { get; private set; }

        public Node(object content, float weight = 1.0f, bool active = false)
        {
            this.content = content;
            this.weight = weight;
            this.active = active;
            children = new List<Node>();
        }

        public void AddChild(Node child)
        {
            if (child == this) throw new Exception("Circular graphs not supported.");
            if (child.parent == this) return;

            children.Add(child);
            child.parent = this;
        }

        public int depth
        {
            get { return GetDepthRecursive(this); }
        }

        private static int GetDepthRecursive(Node node)
        {
            if (node.parent == null) return 1;
            return 1 + GetDepthRecursive(node.parent);
        }

        public virtual Type GetContentType()
        {
            return content == null ? null : content.GetType();
        }

        public virtual string GetContentTypeName()
        {
            Type type = GetContentType();
            return type == null ? "Null" : type.ToString();
        }

        public virtual string GetContentTypeShortName()
        {
            return GetContentTypeName().Split('.').Last();
        }

        public override string ToString()
        {
            return "Node content: " + GetContentTypeName();
        }

        public virtual Color GetColor()
        {
            Type type = GetContentType();
            if (type == null)
                return Color.Red;

            string shortName = type.ToString().Split('.').Last();
            float h = (float)Math.Abs(shortName.GetHashCode()) / int.MaxValue;
            return ColorFromHSV(h, 0.6f, 1.0f);
        }

        // Helper method to convert HSV to RGB for System.Drawing.Color
        private static Color ColorFromHSV(float h, float s, float v)
        {
            int hi = Convert.ToInt32(Math.Floor(h * 6)) % 6;
            float f = h * 6 - (float)Math.Floor(h * 6);
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float r = 0, g = 0, b = 0;
            switch (hi)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            return Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
