# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.6.0] - 2024-04-06

### Added

- Added IMonitorHandle.GetValueAsObject() to get a monitored object without any formattig. As requrest in #26

### Fixed

- Fixed #23 Users can select the update rate of SceneHook in the Settings to Update, FixedUpdate, or LateUpdate
- Fixed #22 Grouped monitored objctes are now unregistered correctly (TextMeshPro)

## [3.5.4] - 2023-06-11

### Fixed

- Fixed #20 WebGL UI is not updating. This was caused because the callback object was created during subsystem registration which seems to not work.
- Fixed #19 Creating a settings file creates an empty resources folder. 
- Fixed a NullReferenceException when starting the game without a settings file as mentioned in #19.

## [3.5.3] - 2023-06-01

### Fixed

- Fixed issue #18 ambiguous reference between 'UnityEditor.Compilation.AssemblyFlags' and '
  System.Reflection.AssemblyFlags'

## [3.5.2] - 2023-05-26

### Fixed

- Fixed an import issue that created multiple Resources folder.

### Added

- Added the option to allow updating monitors even when Time.timeScale is below 0.05

### Changed

- Moved settings access from Window/Runtime Monitoring to Tools/Runtime Monitoring

## [3.5.1] - 2023-04-26

### Fixed

- Fixed a NullReferenceException that occured when exiting playmode.
- Fixed runtime NullReferenceExceptions in MonitoringRegistry caused by race conditions during initialization.

## [3.5.0] - 2023-04-08

### Fixed

- Fixed an invalid cast exception that happened in newer versions of unity.
- Fixed an error that happened when creating an new monitoring settings asset during import.

### Changed

- Removed Thread Dispatcher (com.baracuda.thread-dispatcher) as a dependency.
- Moved settings access from Tools/Runtime Monitoring to Window/Runtime Monitoring

## [3.4.2] - 2023-03-07

### Fixed

- Fixed an exception that occurred in builds when monitoring was set to editor only.

## [3.4.1] - 2022-11-13

### Fixed

- Runtime Monitoring now works properly with Disabled Domain and Scene reloads in Enter Play Mode Options.

### Deprecated (Removing in [4.0.0])

- ISettableValue is now deprecated.

## [3.4.0] - 2022-10-30

### Added

- Added a filtering editor window: (menu: Tools > Runtime Monitoring > Filter Window)
- Added Documentation in multiple file formats to samples.
- MonitoringMetaAttributes can now be applied to interfaces.

### Changed

- Settings are now accessed via: (menu: Tools > Runtime Monitoring > Settings)
- Changed multiple property types in IMonitoringRegistry from returning an IReadOnlyCollection to returning an
  IReadOnlyList.

## [3.3.0] - 2022-10-29

### Known Issues

- Monitoring structs that implement IEnumerable or IEnumerable<T> are not monitored like a collection.

### Added

- Added better error handling for Editor and Debug builds.

### Changed

- Refactored and reworked API to reduce complexity and improve accessibility.
- Removed service locator pattern since it introduced a lot of complexity without offering any benefit.
- Replaced Baracuda.Monitoring.MonitoringSystems with Baracuda.Monitoring.Monitor
- Improved static initialization.
- Updated documentation.

### Fixed

- Fixed an issue that occured when registering a monitor target during static initialization / construction.
- Fixed an issue when monitoring a non generic type with a generic IEnumerable interface.

### Deprecated (Removing in [4.0.0])

- Baracuda.Monitoring.MonitoringSystems is now obsolete. Use Baracuda.Monitoring.Monitor instead!
- MonitoringSystems.Initialized is now obsolete. Use Monitor.Initialized instead!
- MonitoringSystems.Manager is now obsolete. Use Monitor, MonitoringEvents and MonitoringRegistry instead!
- MonitoringSystems.Settings is now obsolete. Use Monitor.Settings instead!
- MonitoringSystems.Utility is now obsolete. Use Baracuda.Monitor.Registry instead!
- MonitoringSystems.UI is now obsolete. Use Monitor.UI instead!
- MonitoringSystems.Resolve{T} is now obsolete. Use Baracuda.Monitoring.Monitor to access API instead!
- MonitoringSystems.Register{T} is now obsolete. Use Baracuda.Monitoring.Monitor to access API instead!
- MonitoringSystems.SystemNotRegisteredException is now obsolete.
- MonitoringExtensions.RegisterMonitor{T}(T target) is now obsolete. Use MonitoringExtensions.BeginMonitoring{T}(T
  target) instead!
- MonitoringExtensions.UnregisterMonitor{T}(T target) is now obsolete. Use MonitoringExtensions.EndMonitoring{T}(T
  target) instead!
- IMonitoringManager is now obsolete.
- IMonitoringManager.IsInitialized is now obsolete. Use Monitor.Initialized instead!
- IMonitoringManager.ProfilingCompleted is now obsolete. Use Monitor.Events.ProfilingCompleted instead!
- IMonitoringManager.UnitCreated is now obsolete. Use Monitor.Events.MonitorHandleCreated instead!
- IMonitoringManager.UnitDisposed is now obsolete. Use Monitor.Events.MonitorHandleDisposed!
- IMonitoringManager.RegisterTarget{T}(T target) is now obsolete. Use Monitor.BeginMonitoring instead!
- IMonitoringManager.UnregisterTarget{T}(T target) is now obsolete. Use Monitor.EndMonitoring!
- IMonitoringManager.GetStaticUnits() is now obsolete. Use Monitor.Registry.GetMonitorHandles(HandleTypes.Static)
  instead!
- IMonitoringManager.GetInstanceUnits() is now obsolete. Use Monitor.Registry.GetMonitorHandles(HandleTypes.Instance)
  instead!
- IMonitoringManager.GetAllMonitoringUnits() is now obsolete. Use Monitor.Registry.GetMonitorHandles() instead!
- IMonitoringUtility is now obsolete.
- IMonitoringUtility.IsFontHashUsed(int fontHash) is now obsolete. Use Baracuda.Monitor.Registry.UsedFonts instead!
- IMonitoringUtility.GetMonitorUnitsForTarget() is now obsolete. Use
  Baracuda.Monitor.Registry.GetMonitorHandlesForTarget instead!
- IMonitoringUtility.GetAllTags() is now obsolete. Use Baracuda.Monitor.Registry.UsedTags instead!
- IMonitoringUtility.GetAllTypeStrings() is now obsolete. Use Baracuda.Monitor.Registry.UsedTypes instead!
- IMonitoringManager.IsFontHashUsed(int fontHash) is now obsolete. Use Baracuda.Monitor.Registry.UsedFonts instead!
- IMonitoringManager.GetMonitorUnitsForTarget(object target) is now obsolete. Use Baracuda.Monitor.Registry.UsedFonts
  instead!
- IMonitoringManager.GetAllTags() is now obsolete. Use Baracuda.Monitor.Registry.UsedTags instead!
- IMonitoringManager.GetAllTypeStrings() is now obsolete. Use Baracuda.Monitor.Registry.UsedTypes instead!
- IMonitorUnit is now obsolete. Use IMonitorHandle instead!
- IMonitoringSubsystem{T} is now obsolete.
- IMonitorHandle.TargetName is now obsolete. Use IMonitorHandle.DisplayName instead!
- IMonitorSettings.UIController is now obsolete. Use MonitoringUIOverride instead!
- IMonitorSettings.EnableMonitoring is now obsolete. Use IMonitoringSettings.IsMonitoringEnabled instead!
- IMonitorSettings.AutoInstantiateUI is now obsolete.
- ProfilingCompletedListener is now obsolete. Use ProfilingCompletedDelegate instead!

## [3.2.2] - 2022-10-21

### Fixed

- Added a temporary fix to prevent an exception during initialization.

## [3.2.1] - 2022-10-20

### Changed

- Updated system installation process and removed initialization from static constructor.
- Added a the new property 'Initialized' to MonitoringSystems.cs that returns true once systems are installed properly.
- Added experimental API to register targets before systems are installed.

### Fixed

- Fixed an exception that happened when disabling async profiling.

## [3.2.0] - 2022-10-14

### Changed

- Updated thread dispatcher version dependency.

## [3.1.6] - 2022-10-13

### Removed

- Removed deprecated code that occasionally caused the settings file to be marked as dirty.

## [3.1.5] - 2022-10-12

### Fixed

- Fixed an exception that was caused by monitoring a struct with an IEnumerable interface.

### Changed

- IL2CPP type def class is now generated with a random name to prevent potential duplicates.

### Removed

- Removed a warning that was displayed when calling this.UnregisterMonitor() on an object that was not already
  registered.

## [3.1.4] - 2022-10-11

### Fixed

- Fixed an issue that prevented the UIs visibility from being toggled.

### Deprecated

- MShowIndexerAttribute is now obsolete. Use MShowIndexAttribute instead. Removing in [4.0.0]

## [3.1.1] - 2022-10-09

### Fixed

- Fixed UI not updating when using the default IMGUI UI. (UI started to update after its visibility was changed
  manually)

## [3.1.0] - 2022-10-08

### Added

- Added multiple font assets to "Resources/Monitoring" that are used by the default MonitoringUI. Additional fonts at
  this path will be available when using the default MonitoringUI.

### Changed

- Reworked multiple aspects of the UI system. MonitoringUI instances can now be instantiated and destroyed at will
  during runtime.
- Selecting a MonitoringUI prefab in the settings window is no longer required. Instead you can now select an override
  prefab to replace the default IMGUI based UI.
- The default IMGUI based UI is no longer a readonly prefab but now created dynamically during runtime. You can still
  import an IMGUI based prefab / MonitoringUI from the samples.
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
- IMonitoringSettings.EnableMonitoring is now obsolete. Use IMonitoringSettings.IsMonitoringEnabled instead. Removing
  in [4.0.0]
- MonitoringUIController is now obsolete. Use MonitoringUI instead. Removing in [4.0.0]
- MonitoringUIController.IsVisible() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.ShowMonitoringUI() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.HideMonitoringUI() is now obsolete. Use MonitoringUI.Visible instead. Removing in [4.0.0]
- MonitoringUIController.OnUnitDisposed() is now obsolete. Use MonitoringUI.OnMonitorUnitDisposed instead. Removing
  in [4.0.0]
- MonitoringUIController.OnUnitCreated() is now obsolete. Use MonitoringUI.OnMonitorUnitCreated instead. Removing
  in [4.0.0]

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
- Removed .unitpackages for UI assets. The same assets can now be imported via the samples section in the package
  manager window.

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
- The first MonitoringUIController instance in a scene will be used as the active MonitoringUIController. Only if no
  instance is located in a scene the selected prefab from the settings window is instantiated and used.

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
- Global value processor methods can be declared by adding the [GlobalValueProcessorAttribute] to a static method with a
  valid signature.
- Added validation logic to dynamically enable / disable monitored member during runtime.
- Added new MetaAttributes & Formatting options.
- Added attributes for custom text, background & group coloring.
- Added MFontAttribute to set the font asset for a monitored unit.
- Added multiple smaller options like text align etc.
- Added ConsoleMonitor Prefab as a preset to easily monitor console logs during runtime.
- Added SystemMonitor Prefab as a preset to easily visualize information about the current system during runtime.
- Added MColorAttribute to set the color of a monitored unit.
- Added profiling data to AOT type generation. AOT file now contains additional information about monitored types,
  member etc.
- Added multiple font assets.

### Fixed

- Fixed an issue that was caused when monitoring constant fields.
- Fixed conditional compilation using DISABLE_MONITORING
- Fixed WebGL runtime issues caused by WebGL initialization order.