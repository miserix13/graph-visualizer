using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GraphVisualizer
{
    public class GraphVisualizerForm : Form
    {
        private GraphCanvas canvas;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem viewMenu;
        private Panel mainPanel;
        private Splitter splitter;
        private Panel propertiesPanel;
        private PropertyGrid propertyGrid;
        private Label statusLabel;

        public GraphVisualizerForm()
        {
            InitializeComponents();
            CreateSampleGraph();
        }

        private void InitializeComponents()
        {
            // Set up main form
            Text = "Graph Visualizer";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;

            // Create menu strip
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem("&File");
            viewMenu = new ToolStripMenuItem("&View");

            var newMenuItem = new ToolStripMenuItem("&New Graph", null, OnNewGraph);
            newMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            var exitMenuItem = new ToolStripMenuItem("E&xit", null, (s, e) => Close());
            fileMenu.DropDownItems.Add(newMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);

            var showPropertiesMenuItem = new ToolStripMenuItem("&Properties", null, OnToggleProperties);
            showPropertiesMenuItem.Checked = true;
            var resetViewMenuItem = new ToolStripMenuItem("&Reset View", null, OnResetView);
            resetViewMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            viewMenu.DropDownItems.Add(showPropertiesMenuItem);
            viewMenu.DropDownItems.Add(resetViewMenuItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);

            // Create status label
            statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                Text = "Ready | Use mouse wheel to zoom | Middle-click to pan"
            };

            // Create properties panel
            propertiesPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var propertiesLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Properties",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 45),
                ViewForeColor = Color.White,
                ViewBackColor = Color.FromArgb(30, 30, 30),
                LineColor = Color.FromArgb(60, 60, 60),
                CategoryForeColor = Color.LightGray,
                HelpVisible = true
            };
            
            propertiesPanel.Controls.Add(propertyGrid);
            propertiesPanel.Controls.Add(propertiesLabel);

            // Create splitter
            splitter = new Splitter
            {
                Dock = DockStyle.Right,
                Width = 3,
                BackColor = Color.FromArgb(60, 60, 60)
            };

            // Create main panel for graph
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(0)
            };

            // Create graph canvas
            canvas = new GraphCanvas
            {
                Dock = DockStyle.Fill
            };
            canvas.NodeClicked += OnNodeClicked;
            mainPanel.Controls.Add(canvas);

            // Add controls to form
            Controls.Add(mainPanel);
            Controls.Add(splitter);
            Controls.Add(propertiesPanel);
            Controls.Add(statusLabel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private void CreateSampleGraph()
        {
            // Create a sample graph for demonstration
            var graph = new SampleGraph();
            canvas.LoadGraph(graph);
            UpdateStatus("Sample graph loaded");
        }

        private void OnNewGraph(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear current graph and create a new sample?", "New Graph", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CreateSampleGraph();
            }
        }

        private void OnToggleProperties(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                propertiesPanel.Visible = !propertiesPanel.Visible;
                splitter.Visible = propertiesPanel.Visible;
                menuItem.Checked = propertiesPanel.Visible;
            }
        }

        private void OnResetView(object sender, EventArgs e)
        {
            canvas.ResetView();
            UpdateStatus("View reset");
        }

        private void OnNodeClicked(Node node)
        {
            if (node != null)
            {
                propertyGrid.SelectedObject = new NodeProperties(node);
                UpdateStatus($"Selected: {node.GetContentTypeShortName()}");
            }
            else
            {
                propertyGrid.SelectedObject = null;
                UpdateStatus("No node selected");
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message + " | Use mouse wheel to zoom | Middle-click to pan";
        }

        public void LoadGraph(Graph customGraph)
        {
            canvas.LoadGraph(customGraph);
            UpdateStatus($"Graph loaded with {customGraph.nodes.Count} nodes");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show("Are you sure you want to exit?", "Exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        // Helper class to display node properties in PropertyGrid
        private class NodeProperties
        {
            private Node node;

            public NodeProperties(Node node)
            {
                this.node = node;
            }

            [System.ComponentModel.Category("Node")]
            [System.ComponentModel.Description("The type name of the node content")]
            public string TypeName => node.GetContentTypeName();

            [System.ComponentModel.Category("Node")]
            [System.ComponentModel.Description("The short type name")]
            public string ShortName => node.GetContentTypeShortName();

            [System.ComponentModel.Category("Node")]
            [System.ComponentModel.Description("The weight of the node")]
            public float Weight => node.weight;

            [System.ComponentModel.Category("Node")]
            [System.ComponentModel.Description("Whether the node is active")]
            public bool Active => node.active;

            [System.ComponentModel.Category("Hierarchy")]
            [System.ComponentModel.Description("The depth in the hierarchy")]
            public int Depth => node.depth;

            [System.ComponentModel.Category("Hierarchy")]
            [System.ComponentModel.Description("Number of children")]
            public int ChildCount => node.children.Count;

            [System.ComponentModel.Category("Hierarchy")]
            [System.ComponentModel.Description("Has parent node")]
            public bool HasParent => node.parent != null;

            [System.ComponentModel.Category("Visual")]
            [System.ComponentModel.Description("The color of the node")]
            public Color Color => node.GetColor();
        }
    }

    // Sample graph implementation for demonstration
    internal class SampleGraph : Graph
    {
        protected override IEnumerable<Node> GetChildren(Node node)
        {
            return node.children;
        }

        protected override void Populate()
        {
            // This will be populated in the constructor
        }

        public SampleGraph()
        {
            // Create root node
            var root = new Node("Root", 1.5f, true);
            
            // Create child nodes
            var child1 = new Node("DataProcessor", 1.2f, true);
            var child2 = new Node("Renderer", 1.0f, true);
            var child3 = new Node("AudioMixer", 0.8f, false);
            
            // Create grandchildren
            var grandchild1 = new Node("InputHandler", 1.0f, true);
            var grandchild2 = new Node("OutputHandler", 1.0f, true);
            var grandchild3 = new Node("Filter", 0.5f, true);
            
            // Build hierarchy
            root.AddChild(child1);
            root.AddChild(child2);
            root.AddChild(child3);
            
            child1.AddChild(grandchild1);
            child1.AddChild(grandchild2);
            child2.AddChild(grandchild3);
            
            // Add to graph
            AddNodeHierarchy(root);
        }
    }
}
