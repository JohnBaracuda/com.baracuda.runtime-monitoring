Monitoring
===

Runtime Monitoring is an open source tool providing an easy way to monitor the value or state of C# objects and members. Just add the 'Monitor' attribute to a field, property, event, method or class and get its value or state displayed during runtime.


&nbsp;
## Table of Contents


- [Getting started](#getting-started)
- [Import / Installation](#import)
- [Value Processor](#value-processor)
- [Monitoring API](#api)
- [Runtime (Mono & IL2CPP)](#runtime)
- [Display / UI](#display-ui)
  - [UI Toolkit](#ui-toolkit)
  - [Unity UI](#ui-ugui)
  - [GUI](#ui-gui)
  - [Custom UI / Display Implimentation](#custom-ui-implimentation)
- [Assemblies / Modules](#assemblies)
  - [Monitoring Core](#monitoring-core)
  - [Monitoring Editor](#monitoring-editor)
  - [Monitoring UI](#monitoring-ui)
  - [Monitoring Example](#monitoring-example)
  - [Threading / Dispatcher](#thread-dispatcher)
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

// When monitoring non static member of an object, 
// instances of the object must be registered.

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

// Monitord member are evaluated in an update loop.
// You can provide an event that will tell monitoring that a 
// value has changed to remove it from the update loop. 

private int healthPoints;
public event Action<int> OnHealthChanged;

[Monitor(UpdateEvent = nameof(OnHealthChanged))]
public int HealthPoints 
{
    get
    {
        return healthPoints;
    }
    private set
    {
        healthPoints = value;
        OnHealthChanged?.Invoke(healthPoints);
    }
}
```


&nbsp;
## Import

Import this asset into your project as a .unitypackage available at [Runtime-Monitoring/releases](https://github.com/JohnBaracuda/Runtime-Monitoring/releases) page. 

Depending on your needs you may select or deselect individual modules when importing. [Monitoring](#monitoring-core "Monitoring Assembly"), [Monitoring Editor](#monitoring-editor "Monitoring Editor Assembly") & [Threading](#thread-dispatcher "Threading / Dispatcher") are essensial modules for this asset. [Monitoring Example](#monitoring-example "Monitoring Example Assembly") contains an optional example scene and [Monitoring UI](#monitoring-ui "Monitoring Preset UI Assemblies") contains UI / Display preset that should work out of the box with different Unity UI Systems.

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
&nbsp;

Static ValueProcessor Methods can have certain overloads for objects that impliment generic collection interfaces.


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
&nbsp;
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
&nbsp;
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
## Runtime

The true purpose of this tool is to provide an easy way to debug and monitor build games. Both Mono & IL2CPP runtimes are supported. Mono runtime works without any limitations.

&nbsp;
### IL2CPP Runtime

Monitoring is making extensive use of dynamic type & method creation during its initialization process. This means that the IL2CPP runtime has a hard time because it requires AOT compilation (Ahead of time compilation)

In order to use IL2CPP as a runtime some features are disabled or reduced and some types must be generated during a build process, that can then be used by the IL2CPP runtime as templates. You can configure the IL2CPP AOT type generation from the monitoring settings.



&nbsp;
## Display UI

The monitoring system does not controll any UI elements. It is almost compleatly separated from UI but provides an easy way to either chose one of the prefabricated UI modules or to create a custom UI Solution based on individual preferences. 

Because Unity has multiple UI systems every prefabricated UI Module is based on one of them. At the moment only UI Toolkit is implimented but Unity UI and event GUI are planned as upcoming features.


&nbsp;
### UI Toolkit

Currently the only implimented UI Solution. UI Toolkit is only available when using Unity 2020.1 or newer. 



&nbsp;
## Licence

[MIT License](https://github.com/JohnBaracuda/Runtime-Monitoring/blob/main/LICENSE) so do what you want but consider giving a star ⭐ or a donation to support me ❤️

❤️❤️❤️ [Donations | PayPal.me](https://www.paypal.com/paypalme/johnbaracuda) ❤️❤️❤️