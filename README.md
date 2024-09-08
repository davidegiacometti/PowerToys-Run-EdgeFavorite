# PowerToys Run: Edge Favorite plugin

Simple [PowerToys Run](https://learn.microsoft.com/windows/powertoys/run) experimental plugin for search Microsoft Edge favorites.

## Important

- Not planning to support other browsers, so please do not ask
- The plugin is developed for the new Microsoft Edge based on Chromium
- The plugin is built on top of the `%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Bookmarks` and may stop working in case of changes to Microsoft Edge

## Requirements

- Microsoft Edge
- PowerToys minimum version 0.81.0

## Installation

- Download the [latest release](https://github.com/davidegiacometti/PowerToys-Run-EdgeFavorite/releases/) by selecting the architecture that matches your machine: `x64` (more common) or `ARM64`
- Close PowerToys
- Extract the archive to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`
- Open PowerToys

## Localization

The plugin has limited user-facing strings, but due to feedback and downloads, version 0.7.0 and onward supports localization.  
The goal is to cover the same languages as PowerToys.  
Pull requests for new languages or improvements to existing translations are welcome.

- Fork and clone this repository
- Open `Community.PowerToys.Run.Plugin.EdgeFavorite.sln` in Visual Studio
- In Solution Explorer, navigate to `Community.PowerToys.Run.Plugin.EdgeFavorite\Properties`
- To enhance existing translations, open and update the relevant `.resx` file
- To add a new language, create a new resource file named `Resources.xx-XX.resx` (e.g. `Resources.it-IT.resx` for Italian)
- Once you're done, commit your changes, push to GitHub, and make a pull request

## Screenshots

![Search](./images/Search.png)

![Plugin Manager](./images/PluginManager.png)
