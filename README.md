# Notepad

This is a multi-tabbed Notepad application built with C# and WPF (Windows Presentation Foundation). The project follows the MVVM (Model-View-ViewModel) architectural pattern.

## Features
* **Tabbed Interface:** You can open and edit multiple text documents at the same time using tabs. Unsaved changes are indicated by an asterisk (*) next to the file name.
* **Folder Explorer:** The app includes a built-in file browser that allows you to navigate your drives, open files directly from the tree view, create new files, and copy/paste folders. You can toggle this view from the menu.
* **Standard File Operations:** Create, open, save, save as, and close files. You also have an option to close all open tabs at once. The app will warn you if you try to close files with unsaved changes.
* **Search & Replace:** A comprehensive search tool that lets you find text, replace specific occurrences, or replace all occurrences. You can choose to search only within the currently active tab or across all open tabs.
* **Keyboard Shortcuts:** Supports standard shortcuts like `Ctrl+N` (New), `Ctrl+O` (Open), `Ctrl+S` (Save), `Ctrl+Shift+S` (Save As), `Ctrl+W` (Close Tab), and `Ctrl+F` (Find).

## Technologies Used
* C#
* .NET 8.0 (Windows)
* WPF (Windows Presentation Foundation)
* MVVM (Model-View-ViewModel) Architecture

## How to Run the Project
1. Make sure you have **Visual Studio** installed (2022 recommended) with the ".NET desktop development" workload.
2. Clone the repository or download the source code archive.
3. Open the `Notepad.sln` file in Visual Studio.
4. Build the solution and press `F5` (or the Start button) to launch the application.
