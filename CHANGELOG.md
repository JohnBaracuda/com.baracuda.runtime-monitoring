# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Looking into solutions to circumvent the whole IL2CPP type def generation requirement because it is making things very complicated and error prone.
- Thread-Dispatcher will be removed as a dependency in a future release.

## [3.2.0] - 2022-10-14

### Changed
- Updated thread dispatcher version dependecy.

## [3.1.6] - 2022-10-13

### Removed
- Removed deprecated code that occasionally caused the settings file to be marked as dirty.


## [3.1.5] - 2022-10-12

### Fixed
- Fixed an exception that was caused by monitoring a struct with an IEnumerable interface.

### Changed
- IL2CPP type def class is now generated with a random name to prevent potential duplicates.

### Removed
- Removed a warning that was displayed when calling this.UnregisterMonitor() on an object that was not already registered. 


## [3.1.4] - 2022-10-11

### Fixed
- Fixed an issue that prevented the UIs visibility from being toggled.

### Deprecated
- MShowIndexerAttribute is now obsolete. Use MShowIndexAttribute instead. Removing in [4.0.0]


## [3.1.1] - 2022-10-09

### Fixed
- Fixed UI not updating when using the default IMGUI UI. (UI started to update after its visibility was changed manually)


## [3.1.0] - 2022-10-08

### Added
- Added multiple font assets to "Resources/Monitoring" that are used by the default MonitoringUI. Additional fonts at this path will be available when using the default MonitoringUI.

### Changed
- Reworked multiple aspects of the UI system. MonitoringUI instances can now be instantiated and destroyed at will during runtime.
- Selecting a MonitoringUI prefab in the settings window is no longer required. Instead you can now select an override prefab to replace the default IMGUI based UI.
- The default IMGUI based UI is no longer a readonly prefab but now created dynamically during runtime. You can still import an IMGUI based prefab / MonitoringUI from the samples.
- The 'Enable Monitoring' toggle in the settings window is now a selection with an additional editor only option.
- Updated samples for IMGUI, TextMeshPro and UIToolkit.
- Updated example scene.

### Deprecated
- IMonitoringUI.IsVisible() is now obsolete. Use IMonitoringUI.Visible instead. Removing in [4.0.0]
- IMonitoringUI.Show() is now obsolete. Use IMonitoringUI.Visible instead. Removing in [4.0.0]
- IMonitoringUI.Hide() is now obsolete. Use IMonitoringUI.Visible instead. Removing in [4.0.0]
- IMonitoringUI.ToggleDisplay() is now obsoleteUse IMonitoringUI.Visible instead. Removing in [4.0.0]
- IMonitoringUI.GetActiveUIController() is now obsolete. Use IMonitoringUI.GetCurrent<T> instead. Removing in [4.0.0]
- IMonitoringUI.GetActiveUIController<T>() is now obsolete. Use IMonitoringUI.GetCurrent<T> instead. Removing in [4.0.0]
- IMonitoringUI.CreateMonitoringUI() is now obsolete. Use IMonitoringUI.Visible instead. Removing in [4.0.0]
- IMonitoringSettings.UIController is now obsolete. Use different approach instead. Removing in [4.0.0]
- IMonitoringSettings.EnableMonitoring is now obsolete. Use IMonitoringSettings.IsMonitoringEnabled instead. Removing in [4.0.0]
- MonitoringUIController is now obsolete. Use MonitoringUI instead. Removing in [4.0.0]
- MonitoringUIController.IsVisible() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.ShowMonitoringUI() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.HideMonitoringUI() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.OnUnitDisposed() is now obsolete. Use MonitoringUI.OnMonitorUnitDisposed instead. Removing in [4.0.0]
- MonitoringUIController.OnUnitCreated() is now obsolete. Use MonitoringUI.OnMonitorUnitCreated instead. Removing in [4.0.0]

### Removed
- Removed com.baracuda.ui assembly.

### Fixed
- Fixed NullReferenceExceptions caused by missing dummy systems when disabling runtime monitoring.
- Fixed an issue with missing namespaces in newer Unity versions.


## [3.0.1] - 2022-10-04

### Fixed
- Fixed a versioning issue with Open UPM


## [3.0.0] - 2022-10-03

### Changed
- Changed the project structure and layout to make it comply with UPM package conventions.
- Restructured the repository to make it compatible with UPM.
- Reworked IL2CPP type definition generation.

### Added
- Static struct member can now be monitored.
- Added multiple assemblies to be ignored during profiling by default. (List is part of the settings file)
- Added package manager dependencies for com.baracuda.thread-dispatcher

### Fixed
- Fixed an IL2CPP type definition issue caused by missing support for generic monitored types. 

### Removed
- Removed obsolete scripts, API and assemblies. 
- Removed .unitpackages for UI assets. The same assets can now be imported via the samples section in the package manager window.


## [2.1.5] - 2022-09-24

### Fixed
- Fixed some IL2CPP type generation issues caused by potentially inaccessible type generations being generated.
- Fixed some potential runtime exceptions caused by missing generic type checks.


## [2.1.4] - 2022-08-20

### Fixed
- Fixed ExecutionEngineException (missing AOT code) that occurred in IL2CPP builds for custom IList{T} value processors.
- Improved value processor warning messages.


## [2.1.3] - 2022-08-20

### Fixed
- Fixed an IL2CPP build issue caused by a missing namespace.


## [2.1.2] - 2022-08-16

### Changed
- Refactored internal classes and removed unused code.


## [2.1.1] - 2022-08-14

### Changed
- Deprecated Assembly-Baracuda-Pooling.
- Deprecated Assembly-Baracuda-Reflection.
- Scripts from obsolete assemblies are now contained in Assembly-Baracuda-Utilities.
- Note that obsolete assemblies and their contents can be removed and only exist because of update compatibility.
- The first MonitoringUIController instance in a scene will be used as the active MonitoringUIController. Only if no instance is located in a scene the selected prefab from the settings window is instantiated and used.

### Added
- Interface monitoring. Interface member can now be used as a target for any monitor attribute.

### Fixed
- Fixed the scale of some TMP UI elements
- Fixed an issue caused by unsafe code usage.


## [2.1.0] - 2022-08-05

### Changed
- Reworked the systems core structure. Systems are now managed and communicate using a Service Locator Pattern.
- Old static API is now obsolete.
- Refactored and moved internal classes and namespaces.
- Reworked how events are monitored. (more options, coloring & subscriber display)
- Reworked AOT file generation.
- Reworked dynamic filtering of monitored member.
- Reworked and optimized TMP & UIToolkit controller.

### Added
- Methods can now be monitored with support for out parameter.
- Added filtering options to the settings window. The example scene showcases new capabilities of the filtering system.
- Global value processor methods can be declared by adding the [GlobalValueProcessorAttribute] to a static method with a valid signature.
- Added validation logic to dynamically enable / disable monitored member during runtime.
- Added new MetaAttributes & Formatting options.
- Added attributes for custom text, background & group coloring.
- Added MFontAttribute to set the font asset for a monitored unit.
- Added multiple smaller options like text align etc.
- Added ConsoleMonitor Prefab as a preset to easily monitor console logs during runtime.
- Added SystemMonitor Prefab as a preset to easily visualize information about the current system during runtime.
- Added MColorAttribute to set the color of a monitored unit.
- Added profiling data to AOT type generation. AOT file now contains additional information about monitored types, member etc.
- Added multiple font assets.

### Fixed
- Fixed an issue that was caused when monitoring constant fields.
- Fixed conditional compilation using DISABLE_MONITORING
- Fixed WebGL runtime issues caused by WebGL initialization order.