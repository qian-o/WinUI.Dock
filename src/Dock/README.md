This is an experimental project aimed at implementing a docking feature similar to Visual Studio. The design is based on AvalonDock but simplifies the concepts of Anchor and Document.

## Project Structure
- DockingManager: The manager of the entire dock, responsible for the layout and sidebar management of the dock.
- LayoutContainer: Similar to the LayoutPanel concept in AvalonDock.
- DocumentContainer: Similar to the LayoutDocumentPaneGroup concept in AvalonDock.
- Document: The content that is ultimately displayed.

## Existing Features
- Dock layout.
- Dock dragging.
- Dock closing.
- Dock pinning and floating.

## Features to be Implemented
- Creating new windows.

## Notes
This project is experimental, and the final code will be implemented in WinUI.Dock with some structural adjustments.