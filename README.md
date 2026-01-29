# Graph Visualizer - Windows Forms Edition

A Windows Forms application for visualizing hierarchical graph structures with a custom canvas renderer.

## Overview

This project has been converted from a Unity Editor extension to a standalone Windows Forms application. It provides an interactive graph visualization tool with support for:

- **Hierarchical Node Structures**: Display parent-child relationships in a tree layout
- **Interactive Canvas**: Pan and zoom with mouse controls
- **Node Selection**: Click nodes to view detailed properties
- **Custom Rendering**: High-performance double-buffered rendering with GDI+

## Features

### Visual Features
- Color-coded nodes based on content type
- Connection lines showing parent-child relationships
- Grid background for spatial reference
- Node selection highlighting
- Weight indicators on nodes
- Active/inactive node states

### Interaction Features
- **Mouse Wheel**: Zoom in/out
- **Middle Mouse Button**: Pan the canvas
- **Left Click**: Select nodes
- **Ctrl+N**: Create new graph
- **Ctrl+R**: Reset view

### UI Components
- **Main Canvas**: Graph visualization area
- **Properties Panel**: Detailed node information display
- **Menu Bar**: File and View operations
- **Status Bar**: User guidance and feedback

## Project Structure

### Core Classes
- **Graph.cs**: Abstract base class for graph data structures
- **Node.cs**: Node representation with hierarchy support
- **GraphCanvas.cs**: Custom Windows Forms control for rendering graphs
- **GraphVisualizerForm.cs**: Main application window

### Layout System
- **IGraphLayout.cs**: Interface for layout algorithms
- **SimpleTreeLayout**: Basic tree layout implementation
- **Vertex**: Position and dimension data for nodes

### Renderer Interface
- **IGraphRenderer.cs**: Abstraction for graph rendering
- **GraphSettings**: Configuration for visual appearance

## Building and Running

### Requirements
- .NET 8.0 SDK or later
- Windows OS (Windows Forms dependency)

### Build
```powershell
dotnet build
```

### Run
```powershell
dotnet run
```

## Sample Usage

The application includes a sample graph that loads automatically on startup. The sample demonstrates:
- Root node with multiple children
- Multi-level hierarchy (grandchildren)
- Different node weights
- Active and inactive nodes

## Customization

### Creating Custom Graphs

Extend the `Graph` base class and implement:
```csharp
protected abstract IEnumerable<Node> GetChildren(Node node);
protected abstract void Populate();
```

### Custom Layout Algorithms

Implement `IGraphLayout` interface:
```csharp
public interface IGraphLayout
{
    List<Vertex> vertices { get; }
    void CalculateLayout(Graph graph);
}
```

## Changes from Unity Version

### Removed Dependencies
- UnityEngine
- UnityEditor
- Unity PlayableGraph APIs

### Replaced Components
- Unity Editor Window → Windows Forms Form
- Unity GUI → GDI+ Graphics
- UnityEngine.Color → System.Drawing.Color
- Unity Rect → System.Drawing.Rectangle
- Unity Vector2/Vector3 → System.Drawing.PointF

### New Features
- Standalone executable (no Unity required)
- Enhanced mouse controls (pan/zoom)
- PropertyGrid integration for node inspection
- Modern dark theme UI

## Known Limitations

- No LiteGraph external library (using custom renderer instead)
- Some nullability warnings (benign, related to nullable reference types)
- Windows-only (Windows Forms dependency)

## Future Enhancements

Potential improvements:
- Save/Load graph data
- Export to image formats
- Custom node styling
- Animation support
- Multi-graph comparison
- Search and filter capabilities

## License

Same as original project.

## Contributing

Contributions welcome! Areas for improvement:
- Additional layout algorithms
- Performance optimizations
- Cross-platform support (consider Avalonia UI)
- More sample graphs
