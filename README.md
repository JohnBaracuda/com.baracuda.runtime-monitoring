Monitoring
===

Runtime Monitoring is an open source tool providing an easy way to monitor the value or state of C# objects and members. Just add the 'Monitor' attribute to a field, property, event, method or class and get its value or state displayed during runtime.

Note that this documentation is currently work in progress.


&nbsp;
## Table of Contents


- [Getting started](#getting-started)
- [Import / Installation](#import)
- [Settings](#settings)
- [Value Processor](#value-processor)
- [Update Loop](#update-loop)
- [Update Event](#update-event)
- [Runtime (Mono & IL2CPP)](#runtime)
- [Display / UI](#display-ui)
  - [UI Toolkit](#ui-toolkit)
  - [Unity UI](#unity-ui)
  - [Custom UI Implementation](#custom-ui-implementation)
- [Assemblies / Modules](#assemblies-and-modules)
- [Planned Features](#planned-features)
- [Licence](#licence)

&nbsp;
## Getting Started

```c#
// Place the MonitorAttribute on any field, property or event.

[Monitor]
private int healthPoints;

[Monitor]
public int HealthPoints { get; private set; }

[Monitor]
public event OnHealthChanged;
```

```c#
// When monitoring instances, these objects must be registered.

public class Player : MonoBehaviour
{
    [Monitor]
    private int healthPoints;

    private void Awake()
    {
        MonitoringUnitManager.RegisterTarget(this);
    }

    private void OnDestroy()
    {
        MonitoringUnitManager.UnregisterTarget(this);
    }
}

// You can simplify this process by using on of the provided basetypes.
// In this case MonitoredBehaviour

public class Player : MonitoredBehaviour
{
    [Monitor]
    private int healthPoints;
}
```


&nbsp;
## Import

Import this asset into your project as a .unitypackage available at [Runtime-Monitoring/releases](https://github.com/JohnBaracuda/Runtime-Monitoring/releases) page. 

Depending on your needs you may select or deselect individual modules when importing. [Monitoring](#monitoring-core "Monitoring Assembly"), [Monitoring Editor](#monitoring-editor "Monitoring Editor Assembly") & [Threading](#thread-dispatcher "Threading / Dispatcher") are essensial modules for this asset. [Monitoring Example](#monitoring-example "Monitoring Example Assembly") contains an optional example scene and [Monitoring UI](#monitoring-ui "Monitoring Preset UI Assemblies") contains UI / Display preset that should work out of the box with different Unity UI Systems.

 Assembly                    | Path                             | Editor           | Core  
:-                           |:-                                |:----------------:|:----------------:         
Assembly-Baracuda-Monitoring | Baracuda/Monitoring              |                  |:heavy_check_mark:
Assembly-Baracuda-Editor     | Baracuda/Monitoring.Editor       |:heavy_check_mark:|:heavy_check_mark:
Assembly-Baracuda-Threading  | Baracuda/Threading               |                  |:heavy_check_mark:
Assembly-Baracuda-Example    | Baracuda/Monitoring.Example      |                  |
Assembly-Baracuda-UITookit   | Baracuda/Monitoring.UI/UIToolkit |                  |


&nbsp;
## Settings

Open the settings by navigating to (menu: Tools > Monitoring > Settings) 


&nbsp;
## Value Processor

You can add the ValueProcessorAttribute to a monitored field or porperty to gain more controll of its string representation. Use the attibute to pass the name of a method that will be used to parse the current value to a string.

The value processor method must accept a value of the monitored members type, can be both static and non static (when monitoring a non non static member) and must return a string.

```c#
[ValueProcessor(nameof(IsAliveProcessor))]
[Monitor] 
private bool isAlive;

private string IsAliveProcessor(bool isAliveValue)
{
    return isAliveValue ? "Player is Alive" : "Player is Dead!";
}
```
```c#
[ValueProcessor(nameof(IListProcessor))] 
[Monitor] private IList<string> names = new string[] {"Gordon", "Alyx", "Barney"};

private string IListProcessor(IList<string> elements)
{
    var str = string.Empty;
    foreach (var name in elements)
    {
        str += name;
        str += "\n";
    }
    return str;
}
```

Static processor methods can have certain overloads for objects that impliment generic collection interfaces. Those overload allow you to process the value of individual elements of the collection instead of the whole collection all at once. 

```c#
//IList<T> ValueProcessor

[ValueProcessor(nameof(IListProcessor))] 
[Monitor] private IList<string> names = new string[] {"Gordon", "Alyx", "Barney"};

private static string IListProcessor(string element)
{
    return $"The name is {element}";
}


[ValueProcessor(nameof(IListProcessorWithIndex))] 
[Monitor] private IList<string> Names => names;

private static string IListProcessorWithIndex(string element, int index)
{
    return $"The name at index {index} is {element}";
}
```
```c#
//IDictionary<TKey, TValue> ValueProcessor

[ValueProcessor(nameof(IDictionaryProcessor))]
[Monitor] private IDictionary<string, bool> isAliveDictionary = new Dictionary<string, bool>
{
    {"Bondrewd", true}, 
    {"Lyza", false}
};

private static string IDictionaryProcessor(string name, bool isAlive)
{
    return $"{name} is {(isAlive ? "alive" : "dead")}";
}
```
```c#
//IEnumerable<T> ValueProcessor

[ValueProcessor(nameof(IEnumerableValueProcessor))]
[Monitor] 
private IEnumerable<int> randomNumbers = new List<int>
{
    1, 43, 14, 65, 23, 174, 16, 2, 786, 4, 89
};

private static string IEnumerableValueProcessor(int number)
{
    return $"{number} is {((number & 1) == 0? "Even" : "Odd")}";
}
```

&nbsp;
## Update Loop

Monitord member are evaluated in an update loop. You can provide an event that will tell monitoring that a value has changed to remove it from the update loop. 


```c#
public enum UpdateOptions
{
    Auto = 0,
    DontUpdate = 1,
    FrameUpdate = 2,
    TickUpdate = 4,
}
```

+ ```UpdateOtions.Auto```: If an update event is set, the state of the members  will only be evaluated when the event is invoked. Otherwise Tick is the preferred update interval. 

+ ```UpdateOtions.DontUpdate```: The members will not be evaluated except once on load. Use this option for constant values.

+ ```UpdateOtions.FrameUpdate```: The member will be evaluated on every LateUpdate.

+ ```UpdateOtions.TickUpdate```: The member will be evaluated on every Tick. Tick is a custom update cycle that is roughly called 30 times per second.


&nbsp;

## Update Event

When monitoring a field or a property (Value units) you can provide an 'OnValueChanged' event that will tell the monitored unit that the state of the member has changed.

This event can either be an ```Action``` or an ```Action<T>```, with T being the type of the monitored field or property. Note that once a valid update event was provided the unit will not be evaluated during an update cycle anymore, unless  ```UpdateOptions``` are explicitly set to ```UpdateOptions.Auto``` or ```UpdateOptions.FrameUpdate```. 

Passing an event will slightly reduce performance overhead for values or member that you know will update rarely. It is however not required.

```c#
private int healthPoints;
public event Action<int> OnHealthChanged;

[Monitor(UpdateEvent = nameof(OnHealthChanged))]
public int HealthPoints 
{
    get => healthPoints;
    private set
    {
        healthPoints = value;
        OnHealthChanged?.Invoke(healthPoints);
    }
}
```

```c#
[Monitor(UpdateEvent = nameof(OnGameStateChanged))]
private bool isGamePaused;

public event Action OnGameStateChanged;

public void PauseGame()
{
    isGamePaused = true;
    OnGameStateChanged?.Invoke();
}

public void ContinueGame()
{
    isGamePaused = false;
    OnGameStateChanged?.Invoke();
}
```



&nbsp;
## Runtime

The true purpose of this tool is to provide an easy way to debug and monitor build games. Both Mono & IL2CPP runtimes are supported. Mono runtime works without any limitations.

### IL2CPP

Monitoring is making extensive use of dynamic type & method creation during its initialization process. This means that the IL2CPP runtime has a hard time because it requires AOT compilation (Ahead of time compilation)

In order to use IL2CPP as a runtime some features are disabled or reduced and some types must be generated during a build process, that can then be used by the IL2CPP runtime as templates. You can configure the IL2CPP AOT type generation from the monitoring settings.


&nbsp;
## Display UI

The monitoring system does not controll any UI elements. It is almost compleatly separated from UI but provides an easy way to either chose one of the prefabricated UI modules or to create a custom UI Solution based on individual preferences. 

Because Unity has multiple UI systems every prefabricated UI Module is based on one of them. At the moment only UI Toolkit is implimented but Unity UI and event GUI are planned as upcoming features. Use the ```MonitoringDisplay``` API to control the current Monitoring UI, independent of its implementation.

```c#
using Baracuda.Monitoring.API;

// Show the monitoring UI overlay.
MonitoringDisplay.Show();

// Hide the monitoring UI overlay.
MonitoringDisplay.Hide();

// Toggle the visibility of the active monitoring display.
// This method returns a bool indicating the new visibility state.
MonitoringDisplay.ToggleDisplay();

// Returns true if the there is an active monitoring display that is also visible.
MonitoringDisplay.IsVisible();
```


### UI Toolkit

Currently the only implimented UI Solution. UI Toolkit is only available when using Unity 2020.1 or newer. 

### Unity UI

Unity UI is not yet implimented. 

### Custom UI Implementation

(WIP) You can create a custom UI implimentation...

+ Create a new class and inherit from ```MonitoringDisplayController```
+ Add the script to a new GameObject and create a prefab of it.
+ Make sure to delete the GameObject from your scene.
+ Select the monitoring settings and serialize the prefab at General/Monitoring Display Controller.

&nbsp;
## Assemblies and Modules

Runtime Monitoring is separated into multiple assemblies / modules. Some of those modules are essential while others are not.

 Assembly                    | Path                             | Editor           | Core             | Note  
:-                           |:-                                |:----------------:|:----------------:|:- 
Assembly-Baracuda-Monitoring | Baracuda/Monitoring              |                  |:heavy_check_mark:|
Assembly-Baracuda-Editor     | Baracuda/Monitoring.Editor       |:heavy_check_mark:|:heavy_check_mark:| [Thread Dispatcher](#https://github.com/JohnBaracuda/Thread-Dispatcher)
Assembly-Baracuda-Threading  | Baracuda/Threading               |                  |:heavy_check_mark:|
Assembly-Baracuda-Example    | Baracuda/Monitoring.Example      |                  |                  |
Assembly-Baracuda-UITookit   | Baracuda/Monitoring.UI/UIToolkit |                  |                  | Unity 2020.1 or newer



&nbsp;
## Planned Features

+ Unity UI implementation
+ GUI implementation
+ Method monitoring
+ Class scoped monitoring
+ Improved IL2CPP support / AOT generation
+ Compatibility with 2019.4


&nbsp;
## Licence

[MIT License](https://github.com/JohnBaracuda/Runtime-Monitoring/blob/main/LICENSE) so do what you want but consider giving a star ⭐ or a donation to support me ❤️

❤️❤️❤️ [Donations | PayPal.me](https://www.paypal.com/paypalme/johnbaracuda) ❤️❤️❤️