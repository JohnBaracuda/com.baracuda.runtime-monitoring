Runtime Monitoring
===

Runtime Monitoring is an easy way for you to monitor the state of your C# classes and objects during runtime. Just add the 'Monitor' attribute to a field, property, event or method and get its value or state displayed automatically in a customizable and extendable UI.

&nbsp;

### Also available on the [Asset Store!](https://u3d.as/2QxJ)

&nbsp;

[![YouTube Preview](https://johnBaracuda.com/media/img/monitoring/Thumbnail.png)](https://www.youtube.com/watch?v=Ir4KPjykYUM)

&nbsp;
## Table of Contents

- [Getting started](#getting-started)
	- [Setup](#setup)
	- [Technical Information](#technical-information)
	- [Feature List](#features)
	- [Import](#import)
- [Monitoring Member](#instanced-and-static-member)
	- [Instanced & Static Member](#instanced-and-static-member)
	- [Monitoring Fields](#monitoring-fields)
	- [Monitoring Properties](#monitoring-properties)
	- [Monitoring Events](#monitoring-events)
	- [Monitoring Methods](#monitoring-methods)
- [Attributes](#attributes)
	- [Value Processor](#value-processor)
	- [Value Processor (Global)](#global-value-processor)
	- [Conditional Display](#conditional-display)
	- [Update Event](#update-event)
- [Monitoring API](#public-api)
- [Runtime](#runtime)
- [Runtime Compatibility](#runtime-compatibility)
- [UI Formatting](#ui-formatting)
- [UI Controller](#ui-controller)
- [Custom UI Controller](#custom-ui-controller)
- [Troubleshooting](#troubleshooting)
- [Assemblies / Modules](#assemblies-and-modules)
- [Planned Features](#planned-features)
- [Support Me ❤️](#support-me)
- [License](#licence)




&nbsp;
## Getting Started

```c#

// Monitor any field, property, event or method during runtime!

[Monitor]
private int healthPoints;

[Monitor]
public int HealthPoints { get; private set; }

[Monitor]
public int GetHealthPoints() => healthPoints;

[Monitor]
public event Action OnHealthChanged;


// Monitor static member as well as instance member

[Monitor]
public static string playerName;

[Monitor]
protected static bool IsPlayerAlive { get; set; }

[Monitor]
internal static event Action<int> OnScoreChanged;


// Use conditions to determine if a member is displayed or not.

[Monitor]
[MShowIf(Condition.CollectionNotEmpty)]
private Stack<string> errorMessages { get; }


// Reduce update overhead by providing an update event.

[Monitor]
[MUpdateEvent(nameof(OnPlayerSpawn))]
public Vector3 LastSpawnPosition { get; set; }

public static event Action<Vector3> OnPlayerSpawn;


// Use processor methods to customize how the value is displayed.

[Monitor]
[MValueProcessor(nameof(IsAliveProcessor))]
public bool IsAlive { get; private set; }

private string IsAliveProcessor(bool value) => value? "Alive" : "Dead";


// Monitor out parameter value.

[MonitorMethod]
public bool TryGetPlayer(int playerId, out var player)
{
    // ...
}


// Register & unregister objects with members you want to monitor.
// This process can be simplified / automated (Take a look at Monitoring Objects)

public class Player : MonoBehaviour
{
    [Monitor]
    private int healthPoints;

    private void Awake()
    {
        MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(this);
    }

    private void OnDestroy()
    {
        MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(this);
    }
}
```
![basic example](https://johnbaracuda.com/media/img/monitoring/Example_03.png)




&nbsp;
## Setup
+ Download and import Runtime Monitoring.
+ Open the settings by navigating to (menu: Tools > RuntimeMonitoring > Settings).
+ Depending on the Unity version and your preferences, import and optional UIController package.
+ Use the `Monitoring UI Controller` field in the UI Controller foldout or use the `Set Active UIController` button on a listed element to set the active UI Controller.
+ The inspector of the set UI Controller object will be inlined and can be edited from the settings window.
![basic example](https://johnbaracuda.com/media/img/monitoring/Example_06.png)




&nbsp;
## Technical Information
+ Unity Version: <b>2019.4</b> (for UIToolkit <b>2020.1</b>) <br/> 
+ Scripting Backend: <b>Mono & IL2CPP</b>
+ API Compatibility: <b>.NET Standard 2.0 or .NET 4.xP</b>
+ Asset Version: <b>2.0.0</b>




&nbsp;
## Features
+ Monitor the value of a Field.
+ Monitor the return value of a Property.
+ Monitor the state of an Event.
+ Monitor the return value & out parameter of a Method.
+ Monitor static and instance member.
+ Display Collections in a readable way. (Not just ToString)
+ Chose one of three available UI solution presets.
+ IMGUI support (default).
+ TextMeshPro based uGUI support.
+ UIToolkit support.
+ Detached UI Interface for custom UI solutions.
+ Custom control of how monitored members are displayed.
+ Works both asynchronous and synchronous (WebGL supported).
+ Mono & IL2CPP support.
+ Clean open source code faithful to C# conventions & best practices.
+ Global value processor. (like property drawer)
+ Drag & drop example modules. (FPSMonitor, ConsoleMonitor etc.)
+ Draw conditions. (only show value if true, not null etc.)
+ Many more features. 




&nbsp;
## Import

Import this asset into your project as a .unitypackage available at [Runtime-Monitoring/releases](https://github.com/JohnBaracuda/Runtime-Monitoring/releases) or clone this repository and use it directly. You can also download this asset from the [Asset Store!](https://u3d.as/2QxJ). Take a look at [Setup](#setup) instructions for more information how to import optional packages. (spoiler: via the settings window)




&nbsp;
## Instanced and Static Member

When monitoring non static member of a class, instances of these classes must be registered when they are created and unregistered when they are destroyed. This process can be automated or simplified, either by creating a custom Factory system that will create/instantiate objects and register them automatically, or by inheriting from a base type that will automatically register and unregister instances. You can use the following predefined base types for this purpose.
+ ```MonitoredBehaviour : MonoBehaviour```
+ ```MonitoredSingleton<T> : MonoBehaviour where T : MonoBehaviour```
+ ```MonitoredScriptableObject : ScriptableObject```
+ ```MonitoredObject : object, IDisposable```

```c#
public class GameManager
{
	// Static member are always monitored!
	[Monitor]
	public static GameState GameState { get; }
}
```

```c#
// Monitored instance must be registered / unregistered.
public class Player : MonoBehaviour
{
    [Monitor] private int health;

    private void Awake()
    {
        MonitoringSystems.Resolve<IMonitoringManager>().RegisterTarget(target);
        // Or use this extension method:
        this.RegisterMonitor();
    }

    private void OnDestroy()
    {
        MonitoringSystems.Resolve<IMonitoringManager>().UnregisterTarget(target);
        // Or use this extension method:
        this.UnregisterMonitor();
    }
}
```

```c#
// Simplified by inheriting from MonitoredBehaviour.
public class Player : MonitoredBehaviour
{
    [Monitor] private int health;
	
	// Just Remember to call base.Awake and base.OnDestroy if you override these methods.
	protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
```


&nbsp;
## Monitoring Events

You can not only monitor the value of a field, property or method, but also the state of an event. Use the MonitorEvent Attribute to customize how the state of the monitored event is displayed. 

 Property                            | Description |     
:-                                        |:-                    |      
`ShowSubscriberCount` | When enabled, the subscriber count of the event handler delegate is displayed. |
`ShowInvokeCounter`     | When enabled, the amount the monitored event has been invoked will be displayed.  |
`ShowSubscriberInfo`   | When enabled, every subscribed delegate will be displayed.       |

```c#
[Monitor]
public event Action OnGameStarted;

[Monitor]
public event Action<T> OnValueChanged;

[Monitor]
public event GameStateDelegate OnGameStateChanged;
public delegate void GameStateDelegate(GameState gameState)


// If you want to monitor how ofter an event is invoked without displaying every subscriber.
[MonitorEvent(ShowInvokeCounter = true, ShowSubscriberInfo = false)]
public event Action<Player> OnPlayerSpawn;
```

![example](https://johnbaracuda.com/media/img/monitoring/Example_09.png)




&nbsp;
## Monitoring Methods

The return value of a method can be monitored like a field or property but with the additional feature that out parameters are monitored and displayed too. You can also set default parameter values by passing them to the constructor of the MonitorMethod attribute.
```c#
[MonitorMethod(3)]
public Player GetPlayerByIndex(int playerIndex)
{
	// Method will be called with an playerIndex of 3.
	//...
}

[MonitorMethod]
public bool TryGetPlayer(int playerIndex, out Player player)
{
	// Method will be called and both the return value and the out parameter player are monitored.
	//...
}
```




&nbsp;
## Attributes

Use Attributes to customize the monitoring process & display of your member. The attributes provided are divided into three broad categories, first the "Monitoring Attributes" to determine which C# member to monitor, second the "Meta Attributes" to customize how a member is monitored and third other attributes used for various purposes.

### Monitoring Attributes
 Attribute          | Base Type             | Description|     
:-                  |:-                     |:-     |      
`[Monitor]`         | Attribute             | Monitor a field, property, event or method|
`[MonitorValue]`    | MonitorAttribute      | Monitor a field or property|
`[MonitorProperty]` | MonitorValueAttribute | Monitor a property      |
`[MonitorField]`    | MonitorValueAttribute | Monitor a field      |
`[MonitorEvent]`    | MonitorAttribute      | Monitor an event      |
`[MonitorMethod]`   | MonitorAttribute      | Monitor a method      |

### Meta Attributes
 Attribute          | Description |     
:-                  |:-     |      
`[MFormatOptions]`  | Set optional formatting options (e.g. font size)|
`[MTag]`            | Set optional tags used for filtering |
`[MUpdateEvent]`    | Set an event that will trigger an refresh/update ([more](#update-event)) |
`[MValueProcessor]` | Set a method that will process the value before it is displayed as a string ([more](#value-processor)) |
`[MStyle]`          | UIToolkit only. Provide optional style names |
`[MTextColor]`      | Set the text color for the target |
`[MBackgroundColor]`| Set the background color for the target |
`[MGroupColor]`     | Set the background color for the targets group |
`[MShowIf]`         | Set custom validation logic  ([more](#conditional-display))  |
`[MFont]`           | Set a custom font style/asset |
`[MRichText]`       | Enable/disable RichText for the instance (to debug) |
`[MOrder]`          | Set the relative UI order of the target |

### Other Attributes
 Attribute               | Description |     
:-                       |:-           |      
`[GlobalValueProcessor]` | Declare a method as a global value processor for a specific type ([more](#value-processor)) |
`[DisableMonitoring]`    | Disable monitoring for the target class or assembly |




&nbsp;
## Value Processor

You can add the MValueProcessorAttribute to a monitored field or porperty to gain more controll of its string representation. Use the attibute to pass the name of a method that will be used to parse the current value to a string. The value processor method must accept a value of the monitored members type, can be both static and non static (when monitoring a non non static member) and must return a string.

```c#
[MValueProcessor(nameof(IsAliveProcessor))]
[Monitor] 
private bool isAlive;

private string IsAliveProcessor(bool isAliveValue)
{
    return isAliveValue ? "Player is Alive" : "Player is Dead!";
}
```
```c#
[MValueProcessor(nameof(IListProcessor))] 
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

Static processor methods can have certain overloads for objects that impliment generic collection interfaces, which allow you to process the value of individual elements of the collection instead of the whole collection all at once. 

```c#
//IList<T> ValueProcessor
[MValueProcessor(nameof(IListProcessor))] 
[Monitor] private IList<string> names = new string[] {"Gordon", "Alyx", "Barney"};

private static string IListProcessor(string element)
{
    return $"The name is {element}";
}

[MValueProcessor(nameof(IListProcessorWithIndex))] 
[Monitor] private IList<string> Names => names;

private static string IListProcessorWithIndex(string element, int index)
{
    return $"The name at index {index} is {element}";
}
```
```c#
//IDictionary<TKey, TValue> ValueProcessor
[MValueProcessor(nameof(IDictionaryProcessor))]
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
[MValueProcessor(nameof(IEnumerableValueProcessor))]
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

![value processor example](https://johnbaracuda.com/media/img/monitoring/Example_01.png)




&nbsp;
## Global Value Processor

You can declare a static method as a global value processor that is then used to process the value for every monitored member of the given type (instanced value processors will still be prefered). The value processor mehtod must have a valid signature, meaning that is has to accept the monitored type as a fist or second argument, can optionally accept an IFormatData object as a first argument and must return a string. 

```c#

// Custom type
public struct Version
{
    public readonly short Major;
    public readonly short Minor;
}

[GlobalValueProcessor]
private static string GlobalValueProcessorVersion(Version version)
{
    return $"Version: {version.Major}.{version.Minor}";
}

[GlobalValueProcessor]
private static string GlobalValueProcessorVersion(IFormatData ctx, Version version)
{
    return $"{ctx.Label}: {version.Major}.{version.Minor}";
}

```



&nbsp;
## Conditional Display

You can use the MShowIfAttribute to define conditions that controll if a monitored value is displayed or not. Note that the value will stil be monitored but not drawn, meaning that fields, properties, events & methods will still be accessed. There are three different ways to validate if the targeted member is displayed or not.

### Validated by Condition

Set a condition for the monitored value that when met will display the monitored value. This is useful to display important debugging information only when it is necessary.

```C#

// Queue will only be displayed if there are errors. (Queue is not empty)
[Monitor]
[MShowIf(Condition.CollectionNotEmpty)]
private Queue<string> errorCache;

// Property will only be displayed if Network is not available. (false)
[Monitor]
[MShowIf(Condition.False)]
private bool NetworkAvailable { get; }

// Property will only be displayed if MainCamera is not available or set. (null)
[Monitor]
[MShowIf(Condition.Null)]
private Camera MainCamera { get; }

```

### Validated by Comparison

Very similar to [Validated by Condition](#validated-by-condition) but more dynamic. Pass in another object value and determine a comparisson type that is then used on the current value of the monitored member and the value passed as an argument. If the comparrison evaluates to be true, the member will be displayed.

```c#

// Will only be displayed if the errorCode is 404
[Monitor]
[MShowIf(Comparison.Equals, 404)]
private int errorCode;

// Will only be displayed more than one player is active.
[Monitor]
[MShowIf(Comparison.Greater, 1)]
private int ActivePlayerCount { get; }

```

### Validated by Member

Very dynamic way of determining if a member is displayed or not. Pass in the name of a field, property, method or event.

+ Passed fields, properties and methods must return a boolean that determines if the member is displayed or not.
+ Passed methods can also accept the current value of the moitored member and use its for a more dynamic evaluation.
+ Passed events must be Action```<bool>``` and can be used to toggle the display of the member

```c#

// Will only be displayed if the field 'monitor' is true.
[Monitor]
[MShowIf(monitor)]
private bool IsAlive {get;}

[SerializeField] private bool monitor = true;


// Will only be displayed if the property 'IsDebug' is true.
[Monitor]
[MShowIf(monitor)]
private bool IsAlive {get;}

pubic static bool IsDebug { get; }


// Will only be displayed if the method 'Validate' returns true.
[Monitor]
[MShowIf(Validate)]
private Entity ActiveTarget {get;}

private bool Validate(Entity target)
{
    return target != null && this.IsActive;
}

```




&nbsp;

## Update Event

When monitoring a field or a property (Value units) you can provide an 'OnValueChanged' event that will tell the monitored unit that the state of the member has changed.

This event can either be an ```Action``` or an ```Action<T>```, with T being the type of the monitored field or property. Note that once a valid update event was provided the unit will not be evaluated during an update cycle anymore, unless  ```UpdateOptions``` are explicitly set to ```UpdateOptions.Auto``` or ```UpdateOptions.FrameUpdate```. 

Passing an event will slightly reduce performance overhead for values or member that you know will update rarely. It is however not required.

```c#
private int healthPoints;
public event Action<int> OnHealthChanged;

[Monitor]
[MUpdateEvent(nameof(OnHealthChanged))]
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
[Monitor]
[MUpdateEvent(nameof(OnGameStateChanged))]
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
## Public API

You can get interfaces for public API by calling ```c#MonitoringSystems.Resolve<TInterface>()```.




&nbsp;
## Runtime

+ Use the #define ```DISABLE_MONITORING``` to disable the internal logic of the tool. Public API will still compile so you don't have to wrap your API calls in a custom #if !DISABLE_MONITORING block.




&nbsp;
## Runtime Compatibility

### Scripting Backend Compatibility

The true purpose of this tool is to provide an easy way to debug and monitor build games. Both Mono and IL2CPP runtimes are supported. Mono runtime works without any limitations.

### IL2CPP
RTM is making extensive use of dynamic type and method creation during its profiling process. This means that the IL2CPP runtime has a hard time because it requires AOT compilation (Ahead of time compilation) When using IL2CPP runtime a list of types is generated shortly before a build to give the compiler the necessary information to generate everything it needs during runtime. You can manually create this list form the settings window 

![example](https://johnbaracuda.com/media/img/monitoring/Example_08.png)




&nbsp;
### Platform Compatibility

I can't test all of these platforms for compatibility. Let me know if you have tested any platform that is NA or not listed here.

Platform              | Compatible       | Note                    |        
:-                    |:-                |:-                       |             
Windows Standalone    |:heavy_check_mark:|                         |
Linux Standalone      |NA                |                         |
Mac Standalone        |NA                |                         |
IOS                   |NA                |                         |
Android               |NA                |                         |
WebGL                 |:heavy_check_mark:|Async profiling disabled |
Stadia                |NA                |                         |
XBox One              |NA                |                         |
XSX                   |NA                |                         |
PlayStation 4         |NA                |                         |
PlayStation 5         |NA                |                         |




&nbsp;
## UI Formatting

Use the MFormatOptionsAttribute to pass additional formatting options. 

```c#
// Will be displayed as "Value: 3.141"
[Monitor]
[MFormatOptions(Format = "0.000")]
private float _pi = 3.14159265359;

// Will be displayed as "Health Points: 100"
[Monitor]
[MFormatOptions(Label = "Health Points")]
private int _hp = 100;

// Will be displayed at the lower right corner of the screen.
[Monitor]
[MFormatOptions(IPosition = UIPosition.LowerRight)]
private string _version = "2.0.1";

// Will be displayed with a font size of 32.
[Monitor]
[MFormatOptions(FontSize = 32)]
private string _message = "Hello";


// Will not be displayed as part of a group.
[Monitor]
[MFormatOptions(GroupElement = false)]
private int _fps;
```




&nbsp;
## UI Controller

Use the ```IMonitoringUI``` API to toggle the visiblity or active state of the current monitoring UI overlay. ```IMonitoringUI``` is an accesspoint and the bridge between custom code and the active ```MonitoringUIController```. This is to offer a layer of abstraction that enables you to switch between multiple either prefabricated or custom UI implimentations / UI Controller.

Note! Not every existing UI controllers (UIToolkit, TextMeshPro and GUI) includes every feature. I would recommend unsing the UIToolkit UI solution if possible.

```c#
using Baracuda.Monitoring.API;
    
// Show the monitoring UI overlay.
MonitoringSystems.Resolve<IMonitoringUI>().Show();

// Hide the monitoring UI overlay.
MonitoringSystems.Resolve<IMonitoringUI>().Hide();

// Toggle the visibility of the active monitoring display.
// This method returns a bool indicating the new visibility state.
MonitoringSystems.Resolve<IMonitoringUI>().ToggleDisplay();

// Get the currently active MonitoringUIController
MonitoringSystems.Resolve<IMonitoringUI>().GetActiveUIController();
    
// Get the currently active MonitoringUIController casted to a concrete implimentation.
MonitoringSystems.Resolve<IMonitoringUI>().GetActiveUIController<T>();
    
// Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the
// Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController
// is instantiated.
MonitoringSystems.Resolve<IMonitoringUI>().CreateMonitoringUI();
    
// Filter displayed units by their name, tags etc. 
MonitoringSystems.Resolve<IMonitoringUI>().Filter();
    
// Reset active filter.
MonitoringSystems.Resolve<IMonitoringUI>().ResetFilter();
    
// Get an Action<bool> event that is invoked when the monitoring UI became visible/invisible.
MonitoringSystems.Resolve<IMonitoringUI>().VisibleStateChanged;
  
```




&nbsp;
## Custom UI Controller

You can create a custom UI controller by follwing the steps below. You can take a look at the existing UI Controller implimentations to get some reference. 

+ Create a new class and inherit from ```MonitoringDisplayController```.
+ Impliment the abstract mehtods and create custom UI logic. 
+ Add the script to a new GameObject and create a prefab of it.
+ Make sure to delete the GameObject from your scene.
+ Open the settings by navigating to (menu: Tools > Monitoring > Settings).
+ Set your prefab as the active controller in the ```Moniotoring UI Controller``` field.




&nbsp;
## Optimizations

In general RTM tries to be as optimizerd as possible by reducing allocations where ever possible and by doing a lot of the heavy work during the initial profiling. However due to the nature of some types and the creation of strings, allocations cannot be prevented. If performance is a sensitive aspect of your project, here are some tips and tricks to keep in mind.

### Reference Types & Value Types
Since there is no easy way to check whether the actual value of a reference type has changed when it is evaluated, a monitored ReferenceType is processed with ToString() each time it is evaluated. This, by default happens either every Update or Tick cycle and may generate a lot of garbage in a very short time, without much scope for automatic optimization. For this reason, it is recommended to pass an UpdateEvent for the monitored value whenever possible to reduce memory allocations. Of course, this shouldn't matter much if you're only debugging one value, but could be detrimental if you want to keep monitoring your member in a release or shipped build. The same is not true for monitored ValueTypes, as these are compared to a cached value to ensure that an update event and a string creation is only triggered when the value has actually changed.


### Collections
Because collections are ReferenceTypes the same applies here but on an even greater scale. Pass an update event when ever possible if you intend to monitor a collection over a longer period. Now because the example below requires a lot of boiler plate code I would not recommend this if you just want to quickly debug the values of a collection. I also want to mention that a better solution is planned and WIP.  

```C#
[Monitor]
[MUpdateEvent(nameof(OnNamesChanged))]
public List<string> Names = new List<string>() {"Riebeckite", "Prisoner", "Feldspar"};

private event Action OnNamesChanged;

public void AddName(string name)
{
    Names.Add(name);
    OnNamesChanged();
}

public void RemoveName(string name)
{
    Names.Remove(name);
    OnNamesChanged();
}
```


### Transform
Monitored Transforms are another type that have the potential to create a lot of garbage. A simple thing you could do to reduce overhead is to control the Transform.hasChanged flag on the Transform itself. The monitoring unit/handler will check the flag and only raise an update event if the flag is set to true. (which it is unless changed manually) Unity is not controlling Transform.hasChanged.




&nbsp;
## Troubleshooting

+ Open the settings by navigating to (menu: Tools > RuntimeMonitoring > Settings).
+ Ensure that both ```Enable Monitoring``` and ```Open Display On Load``` are set to ```true```.
+ If ```Enable Monitoring``` in the UI Controller foldout is set to ```false```, Make sure to call ```MonitoringSystems.Resolve<IMonitoringUI>().CreateMonitoringUI()``` from anywhere in you code. 




&nbsp;
## Assemblies and Modules

 Assembly                                    | Path                                 | Core             | Note  
:-                                           |:-                                    |:----------------:|:- 
Assembly-Baracuda-Monitoring                 | Baracuda/Monitoring                  |:heavy_check_mark:|
Assembly-Baracuda-Editor                     | Baracuda/Monitoring.Editor           |:heavy_check_mark:| Editor
Assembly-Baracuda-Example                    | Baracuda/Monitoring.Example          |                  | 
Assembly-Baracuda-Monitoring.GUI             | Baracuda/Monitoring.UI/UnityGUI      |:heavy_check_mark:| Default UI
Assembly-Baracuda-Monitoring.UITookit        | Baracuda/Monitoring.UI/UIToolkit     |                  | Unity 2020.1 or newer
Assembly-Baracuda-Monitoring.TextMeshPro     | Baracuda/Monitoring.UI/TextMeshPro   |                  | TMP Required
Assembly-Baracuda-Pooling                    | Baracuda/Pooling                     |:heavy_check_mark:| 
Assembly-Baracuda-Threading                  | Baracuda/Threading                   |:heavy_check_mark:| [Thread Dispatcher](https://github.com/JohnBaracuda/Thread-Dispatcher)
Assembly-Baracuda-Reflection                 | Baracuda/Reflection                  |:heavy_check_mark:| 




&nbsp;
## Planned Features

Any help is appreciated. Feel free to contact me if you have any feedback, suggestions or questions.

### General
+ Add to OpenUPM

### Core System
+ Monitoring Editor Window for edit time monitoring.
+ Monitored collection type with "dirty" flag. (Optimization)
+ Option to create "virtual" units during runtime.

### Documentation
+ Tutorial how to use.
+ Guide how to create a custom UI Controller.
+ Guide how to customize UI.




&nbsp;
## Support Me

I spend a lot of time working on this and other free assets to make sure as many people as possible can use my tools regardless of their financial status. Any kind of support I get helps me keep doing this, so consider leaving a star ⭐ making a donation or follow me on my socials to support me ❤️

+ [Donation (PayPal.me)](https://www.paypal.com/paypalme/johnbaracuda)
+ [Linktree](https://linktr.ee/JohnBaracuda)
+ [Twitter](https://twitter.com/JohnBaracuda)
+ [Itch](https://johnbaracuda.itch.io/)




&nbsp;
## Licence

[MIT](https://github.com/JohnBaracuda/Runtime-Monitoring/blob/main/LICENSE) You can use this tool for anything you want, including commercial products, as long as you're not just selling my work or using it for some other morally questionable or condemnable act.
