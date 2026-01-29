using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GraphVisualizer
{
    public class GraphCanvas : Panel
    {
        private Graph graph;
        private IGraphLayout layout;
        private Node selectedNode;
        private Dictionary<Node, RectangleF> nodeBounds;
        private Point panOffset;
        private Point lastMousePosition;
        private bool isPanning;
        private float zoom = 1.0f;

        public event Action<Node> NodeClicked;

        public GraphCanvas()
        {
            nodeBounds = new Dictionary<Node, RectangleF>();
            panOffset = new Point(0, 0);
            
            // Enable double buffering for smooth rendering
            DoubleBuffered = true;
            ResizeRedraw = true;
            
            BackColor = Color.FromArgb(30, 30, 30);

            // Mouse events
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;
        }

        public void LoadGraph(Graph customGraph)
        {
            graph = customGraph;
            layout = new SimpleTreeLayout();
            layout.CalculateLayout(graph);
            selectedNode = null;
            nodeBounds.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Apply transformations
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(zoom, zoom);

            if (graph == null || layout == null || layout.vertices == null)
            {
                DrawNoGraphMessage(g);
                return;
            }

            // Draw grid
            DrawGrid(g);

            // Draw connections first (so they appear behind nodes)
            DrawConnections(g);

            // Draw nodes
            DrawNodes(g);
        }

        private void DrawNoGraphMessage(Graphics g)
        {
            string message = "No graph loaded. Use File > New Graph to create one.";
            using (Font font = new Font("Segoe UI", 14))
            using (SolidBrush brush = new SolidBrush(Color.Gray))
            {
                SizeF size = g.MeasureString(message, font);
                PointF position = new PointF(
                    (Width - size.Width) / 2,
                    (Height - size.Height) / 2
                );
                g.DrawString(message, font, brush, position);
            }
        }

        private void DrawGrid(Graphics g)
        {
            int gridSize = 50;
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 50)))
            {
                // Vertical lines
                for (int x = -panOffset.X % gridSize; x < Width; x += gridSize)
                {
                    g.DrawLine(gridPen, x / zoom, 0, x / zoom, Height / zoom);
                }
                
                // Horizontal lines
                for (int y = -panOffset.Y % gridSize; y < Height; y += gridSize)
                {
                    g.DrawLine(gridPen, 0, y / zoom, Width / zoom, y / zoom);
                }
            }
        }

        private void DrawConnections(Graphics g)
        {
            if (layout.vertices == null)
                return;

            using (Pen connectionPen = new Pen(Color.FromArgb(100, 100, 100), 2))
            {
                connectionPen.EndCap = LineCap.ArrowAnchor;

                foreach (var vertex in layout.vertices)
                {
                    Node node = vertex.node;
                    RectangleF fromRect = GetNodeBounds(vertex);
                    PointF fromCenter = new PointF(
                        fromRect.X + fromRect.Width / 2,
                        fromRect.Y + fromRect.Height / 2
                    );

                    foreach (var child in node.children)
                    {
                        var childVertex = layout.vertices.FirstOrDefault(v => v.node == child);
                        if (childVertex != null)
                        {
                            RectangleF toRect = GetNodeBounds(childVertex);
                            PointF toCenter = new PointF(
                                toRect.X + toRect.Width / 2,
                                toRect.Y + toRect.Height / 2
                            );

                            // Draw connection line
                            g.DrawLine(connectionPen, fromCenter, toCenter);
                        }
                    }
                }
            }
        }

        private void DrawNodes(Graphics g)
        {
            if (layout.vertices == null)
                return;

            nodeBounds.Clear();

            foreach (var vertex in layout.vertices)
            {
                Node node = vertex.node;
                RectangleF bounds = GetNodeBounds(vertex);
                nodeBounds[node] = bounds;

                // Determine node color
                Color nodeColor = node.GetColor();
                
                // Draw node background
                using (SolidBrush brush = new SolidBrush(nodeColor))
                {
                    g.FillRectangle(brush, bounds);
                }

                // Draw node border
                Color borderColor = node == selectedNode ? Color.Yellow : 
                                   node.active ? Color.White : Color.Gray;
                float borderWidth = node == selectedNode ? 3f : node.active ? 2f : 1f;
                
                using (Pen borderPen = new Pen(borderColor, borderWidth))
                {
                    g.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }

                // Draw node label
                string label = node.GetContentTypeShortName();
                using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(label, font, textBrush, bounds, format);
                }

                // Draw weight indicator if significant
                if (node.weight > 1.0f)
                {
                    string weightText = $"x{node.weight:F1}";
                    using (Font smallFont = new Font("Segoe UI", 8))
                    using (SolidBrush weightBrush = new SolidBrush(Color.LightGray))
                    {
                        g.DrawString(weightText, smallFont, weightBrush, 
                            new PointF(bounds.Right - 25, bounds.Top + 2));
                    }
                }
            }
        }

        private RectangleF GetNodeBounds(Vertex vertex)
        {
            float x = vertex.x * Width;
            float y = vertex.y * Height;
            float width = Math.Max(vertex.width * Width, 120);
            float height = Math.Max(vertex.height * Height, 60);
            
            return new RectangleF(x, y, width, height);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true;
                lastMousePosition = e.Location;
                Cursor = Cursors.Hand;
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Check if clicked on a node
                Point adjustedPoint = new Point(
                    (int)((e.X - panOffset.X) / zoom),
                    (int)((e.Y - panOffset.Y) / zoom)
                );

                Node clickedNode = null;
                foreach (var kvp in nodeBounds)
                {
                    if (kvp.Value.Contains(adjustedPoint))
                    {
                        clickedNode = kvp.Key;
                        break;
                    }
                }

                if (clickedNode != selectedNode)
                {
                    selectedNode = clickedNode;
                    NodeClicked?.Invoke(selectedNode);
                    Invalidate();
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;
                
                panOffset.X += deltaX;
                panOffset.Y += deltaY;
                
                lastMousePosition = e.Location;
                Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = false;
                Cursor = Cursors.Default;
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoom;
            zoom += e.Delta > 0 ? 0.1f : -0.1f;
            zoom = Math.Max(0.1f, Math.Min(zoom, 3.0f));
            
            // Adjust pan offset to zoom towards mouse position
            float zoomFactor = zoom / oldZoom;
            panOffset.X = (int)(e.X - (e.X - panOffset.X) * zoomFactor);
            panOffset.Y = (int)(e.Y - (e.Y - panOffset.Y) * zoomFactor);
            
            Invalidate();
        }

        public void ResetView()
        {
            zoom = 1.0f;
            panOffset = new Point(0, 0);
            Invalidate();
        }

        public Node GetSelectedNode()
        {
            return selectedNode;
        }
    }
}
