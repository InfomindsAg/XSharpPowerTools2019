# X# PowerTools

---------------------------------------

A set of tools for [X#](https://www.xsharp.eu) developers

## Getting started

Get the extension on the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=InfomindsAG.XSharpPowerTools).
Installation requires a minimum Visual Studio Version of 16.10.4 and a minimum X# Version of 2.7.

## Features

- Code Browser
- Find and insert Namespaces

The tools can be found under **Tools>X# Power Tools** or in the editors context menu.

![X# Power Tools Commands](Images/ToolsMenu.PNG)
![X# Power Tools Commands](Images/ContextMenu.PNG)

There are no default shortcuts assigned, but you can easily assign shortcuts  yourself by following this [tutorial](https://docs.microsoft.com/en-us/visualstudio/ide/identifying-and-customizing-keyboard-shortcuts-in-visual-studio?view=vs-2019)

### Code Browser

The Code Browser allows to easily navigate within a solution containing X#-projects.

![X# Code Browser](Images/CodeBrowser.PNG)

It offers various options to filter the search results:

- example		- searches for classes with names similar to 'example'
- ex1.ex2		- searches for members similar to 'ex2' within classes similar to 'ex1' ('.' equal to ':')
- .example	    - searches for members 'example' within all classes
- ..example	    - searches for members 'example' within current document
- "example"     - matches whole word only
- ex*Model      - * is a placeholder for multiple characters
- p example     - searches for procedures/functions similar to 'example'
- d example     - searches for defines similar to 'example'

By pressing Return or double-clicking the implementation of the selected element will be opened in the editor.

By pressing Ctrl + Return or clicking the top-right button the found results are exported into a dockable Visual Studio Toolwindow where results are grouped by the containing project.

![X# Code Browser ToolWindow](Images/CodeBrowserToolWindow.PNG)

### Find and insert Namespaces

Find and insert Namespaces makes it easy to find the containing Namespace of a type and insert the corresponding using at top of the current document if necessary.

![X# Find Namespace](Images/FindNameSpace.PNG)