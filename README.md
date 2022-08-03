Runtime Monitoring
===

Runtime Monitoring is an easy way for you to monitor the state of your C# classes and objects during runtime. Just add the 'Monitor' attribute to a field, property, event or method and get its value or state displayed automatically in a customizable and extendable UI.

&nbsp;

# Also available on the [Asset Store!](https://u3d.as/2QxJ)

&nbsp;

[![YouTube Preview](https://johnBaracuda.com/media/img/monitoring/Thumbnail.png)](https://www.youtube.com/watch?v=Ir4KPjykYUM)

&nbsp;
## Table of Contents

- [Getting started](#getting-started)
	- [Setup](#setup)
	- [License](#license)
	- [Technical Information](#technical-information)
	- [Feature List](#features)
	- [Import & Update](#import-and-update)
- [Monitoring Member](#monitoring-member)
	- [Instanced & Static Member](#instanced-and-static-member)
	- [Monitoring Fields & Properties](#monitoring-fields-and-properties)
	- [Monitoring Events](#monitoring-events)
	- [Monitoring Methods](#monitoring-methods)
- [Attributes](#attributes)
	- [Value Processor](#value-processor)
	- [Value Processor (Global)](#global-value-processor)
	- [Conditional Display](#conditional-display)
	- [Update Event](#update-event)
- [Monitoring UI](#monitoring-ui)
	- [UI Formatting](#ui-formatting)
	- [UI Filtering](#ui-filtering)
	- [Custom UI Controller](#custom-ui-controller)
- [Systems and API](#systems-and-api)
	- [Monitoring Manager](#monitoring-manager)
	- [Monitoring UI](#monitoring-ui)
	- [Monitoring Settings](#monitoring-settings)
	- [Monitoring Utility](#monitoring-utility)
	- [Monitor Unit](#monitor-unit)
	- [Monitor Profile](#monitor-profile)
- [Compatibility](#compatibility)
	- [Runtime](#runtime-compatibility)
	- [Platform](#platform-compatibility)
- [Optimizations](#optimizations)
- [FAQ: Frequently Asked Questions](#frequently-asked-questions)
- [Support Me ❤️](#support-me)




&nbsp;
# Getting Started

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
![basic example](https://johnbaracuda.com/media/img/monitoring/Example_runtime_01.png)




&nbsp;
## Setup
Download and import Runtime Monitoring. To setup a different UI Controller (IMGUI, TMP or UIToolkit) follow these optional steps:
+ Open the settings by navigating to (menu: Tools > RuntimeMonitoring > Settings).
+ Depending on the Unity version and your preferences, import and optional UIController package.
+ Use the `Monitoring UI Controller` field in the UI Controller foldout or use the `Set Active UIController` button on a listed element to set the active UI Controller.
+ The inspector of the set UI Controller object will be inlined and can be edited from the settings window.
![basic example](https://johnbaracuda.com/media/img/monitoring/Example_06.png)





&nbsp;
## License

[MIT](https://github.com/JohnBaracuda/Runtime-Monitoring/blob/main/LICENSE) You can use this tool for anything you want, including commercial products, as long as you're not just selling my work or using it for some other morally questionable or condemnable act.




&nbsp;
## Technical Information
+ Unity Version: <b>2019.4</b> (for UIToolkit <b>2020.1</b>) <br/> 
+ Scripting Backend: <b>Mono & IL2CPP</b>
+ API Compatibility: <b>.NET Standard 2.0 or .NET 4.x</b>
+ Asset Version: <b>2.0.1</b>




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
+ Apply filter to displayed units.
+ Custom control of how monitored members are displayed.
+ Works both asynchronous and synchronous (WebGL supported).
+ Mono & IL2CPP support.
+ Clean open source code faithful to C# conventions & best practices.
+ Global value processor. (like property drawer)
+ Drag & drop example modules. (FPSMonitor, ConsoleMonitor etc.)
+ Draw conditions. (only show value if true, not null etc.)
+ Many more features. 




&nbsp;
## Import and Update

Import this asset into your project as a .unitypackage available at [Runtime-Monitoring/releases](https://github.com/JohnBaracuda/Runtime-Monitoring/releases) or clone this repository and use it directly. You can also download this asset from the [Asset Store!](https://u3d.as/2QxJ). Take a look at [Setup](#setup) instructions for more information how to import optional packages. (spoiler: via the settings window)

> ⚠️ If you are updating from an older version of this tool, I would advise to perform a clean update. Delete the Baracuda folder and import this asset again. Be aware that this may temporarily cause compile errors. You could also remove every usage of this tool before. 




&nbsp;
# Monitoring Member

Place the `[Monitor]` attribute an a field, property, event or method and get its value or state monitored in a customizable UI during runtime. When monitoring non static (instances), you have to register the monitored target, as shown in the next point. Other than that there is not much to it. Just try it out yourself. 

&nbsp;
![example](https://johnbaracuda.com/media/img/monitoring/Example_member_01.png)

> Note that the PlayerMovement class in this example is calling `this.RegisterMonitor()` in its Awake and `this.UnregisterMonitor` in its OnDestory method.

&nbsp;
## Instanced and Static Member

When monitoring non static member of a class, instances of these classes must be registered when they are created and unregistered when they are destroyed. This process can be automated or simplified, either by creating a custom Factory system that will create/instantiate objects and register them automatically, or by inheriting from a base type that will automatically register and unregister instances. You can use the following predefined base types for this purpose.
+ ```MonitoredBehaviour : MonoBehaviour```
+ ```MonitoredSingleton<T> : MonoBehaviour where T : MonoBehaviour```
+ ```MonitoredScriptableObject : ScriptableObject```
+ ```MonitoredObject : object, IDisposable```

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

```c#
public class GameManager
{
    // Static member are always monitored!
    [Monitor]
    public static GameState GameState { get; }
}
```





&nbsp;
## Monitoring Fields and Properties

Monitoring fields and properties is almost identical, differing only in their technical implementations. Just place the Monitor, MonitorField or MonitorProperty on either a field or a property and get its value displayed [automatically](#instanced-and-static-member) .  Multiple types like Booleans, Collections, Vectors etc. are also displayed in a readable way. To customize how a monitored value is displayed you can use a [Value Processor](#value-processor) and utilize a variety of additional  [formatting](#ui-formatting) attributes. 

```c#
[MonitorField] 
private int value;

[MonitorProperty] 
private int Value { get; }
```





&nbsp;
## Monitoring Events

Use the `[Monitor]` or `[MonitorEvent]` attributes to monitor the state of an event. The `[MonitorEvent]` attribute accepts additional arguments to customize how the event is displayed.

 Property                            | Description |     
:-                                        |:-                    |      
`ShowSubscriberCount` | When enabled, the subscriber count of the event is displayed. |
`ShowInvokeCounter`     | When enabled, the invoke count of the event is displayed. (Can be incorrect when async profiling is enabled!) |
`ShowSubscriberInfo`   | When enabled, every subscribed delegate is displayed.  |
`ShowEventSignature`   | When enabled, display the signature of the event.  |
`ShowEventHistory`       | When enabled, the arguments of the last x amount of invocations is displayed. (Planned feature) |

```c#
[Monitor]
public event Action OnGameStarted;

[Monitor]
public event Action<T> OnValueChanged;

[Monitor]
public event GameStateDelegate OnGameStateChanged;
public delegate void GameStateDelegate(GameState gameState);


// If you want to monitor how ofter an event is invoked without displaying every subscriber.
[MonitorEvent(ShowInvokeCounter = true, ShowSubscriberInfo = false)]
public event Action<Player> OnPlayerSpawn;
```

![example](https://johnbaracuda.com/media/img/monitoring/Example_event_03.png)





&nbsp;
## Monitoring Methods

A method can be monitored like a field or property with the additional feature that their out parameters are monitored too. Default parameter values can be set by passing them to the constructor of the `[MonitorMethod]` attribute. Even methods that return void can be monitored if they have at least one out parameter. Default and [Global Value Processor](#global-value-processor) are applied to monitored out parameters, meaning that collections, vectors etc. assigned via out parameter are displayed in a readable way and not just formatted with ToString().

```c#
[Monitor]
public string GetName()
{
	return "Hello World"
}

[MonitorMethod(3, 5)]
public int Multiply(int lhs, int rhs)
{
    // Displayed value will be 15;
    return lhs * rhs;
}

[MonitorMethod]
public bool TryGetPlayer(int playerIndex, out Player player)
{
    // Method will be called and both the return value and the out parameter player are monitored.
    //...
}

[MonitorMethod]
public void Populate(out Vector3[] verts)
{
    // verts will be displayed in a readable way.
    //...
}
```

![example](https://johnbaracuda.com/media/img/monitoring/Example_method_01.png)

> This example additionally shows that out parameters are formatted too.



&nbsp;
# Attributes

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
`[MUpdateEvent]`    | Set an event that will trigger an refresh/update ([more](#update-event)) |
`[MValueProcessor]` | Set a method that will process the value before it is displayed as a string ([more](#value-processor)) |
`[MShowIf]`         | Set custom validation logic  ([more](#conditional-display))  |
`[MEnabled]`         | Set the default enabled state of the monitored member.  |
`[MTag]`            | Set optional tags used for filtering |

### Meta Attributes ([Formatting](#ui-formatting))
 Attribute          | Description |     
:-                  |:-     |      
`[MOptions]`  | Contains (almost) all of the options below. |
`[MFormat]`  | Custom format string used to display the members value if possible.|
`[MLabel]`  | Custom label for the member (otherwise humanized name).|
`[MFontSize]`  | Set the font size for the element.|
`[MFontName]`           | Pass the name of a custom font style that will be used for the target member.|
`[MGroupName]`           | Set the group for the element. |
`[MGroupElement]`           | Whether or not the unit should be wrapped in an object or type group. |
`[MShowIndexer]`  | If the member is a collection, determine if the index of individual elements should be displayed or not. |
`[MElementIndent]`  | The indent of individual elements of a displayed collection. |
`[MPosition]`  | The preferred position of an individual UIElement on the canvas. |
`[MTextAlign]`  | Horizontal text align. |
`[MOrder]`          | Relative vertical order of the displayed element. |
`[MRichText]`          | Override local RichText settings. |
`[MTextColor]`      | Set the text color for the element. |
`[MBackgroundColor]`| Set the background color for the element. |
`[MGroupColor]`     | Set the background color for the elements group. |
`[MStyle]`          | UIToolkit only. Provide optional style names. |

### Other Attributes
 Attribute               | Description |     
:-                       |:-           |      
`[GlobalValueProcessor]` | Declare a method as a global value processor for a specific type ([more](#value-processor)) |
`[DisableMonitoring]`    | Disable monitoring for the target class or assembly |





&nbsp;
## Value Processor

You can add the MValueProcessorAttribute to a monitored field, porperty or method to gain more control of its string representation. Use the attribute to pass the name of a method that will be used to parse the current value to a string. The value processor method must accept a value of the monitored members type, can be both static and non static (when monitoring a non non static member) and must return a string.

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


&nbsp;
 >This example demonstrates how to create a simple progress bar with the help of a value processor. Note that this code is already relatively optimized.

![example](https://johnbaracuda.com/media/img/monitoring/Example_valueprocessor_01.png)


&nbsp;
### Static Value Processor

Static processor methods can have certain overloads for objects that implement generic collection interfaces, which allow you to process the value of individual elements of the collection instead of the whole collection all at once. 

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





&nbsp;
## Global Value Processor

You can declare a static method as a global value processor that is then used to process the value for every monitored member of the given type (instanced value processors will still be preferred). The value processor method must have a valid signature, meaning that is has to accept the monitored type as a fist or second argument, can optionally accept an IFormatData object as a first argument and must return a string. 

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

You can use the MShowIfAttribute to define conditions that control if a monitored value is displayed or not. Note that the value will still be monitored but not drawn, meaning that fields, properties, events & methods will still be accessed. There are three different ways to validate if the targeted member is displayed or not.

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

Very similar to [Validated by Condition](#validated-by-condition) but more dynamic. Pass in another object value and determine a comparison type that is then used on the current value of the monitored member and the value passed as an argument. If the comparison evaluates to be true, the member will be displayed.

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

+ Passed fields, properties and methods must return a Boolean that determines if the member is displayed or not.
+ Passed methods can also accept the current value of the monitored member and use its for a more dynamic evaluation.
+ Passed events must be `Action<bool>` and can be used to toggle the display of the member

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
# Monitoring UI

Use the `IMonitoringUI` API to access the active MonitoringUIController and set properties like visibility, filtering etc.

Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`void Show()`|Show the monitoring UI overlay.|
`void Hide()`|Hide the monitoring UI overlay.|
`void ToggleDisplay()`|Toggle the visibility of the active monitoring display.|
`bool IsVisible()`|True if monitoring UI is visible.|
`T GetActiveUIController<T>()`|Get the currently active MonitoringUIController casted to a concrete implementation.|
`void CreateMonitoringUI()`|Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController is instantiated. |
`void ApplyFilter(string filter)`|Filter displayed units by their name, tags etc. [more](#ui-filtering)|
`void ResetFilter()`|Reset active filter. [more](#ui-filtering)|
`event Action<bool> VisibleStateChanged()`|Event invoked when the monitoring UI became visible or invisible.|





&nbsp;
## UI Formatting

Formatting attributes can be used to apply custom styling options on how a monitored member is displayed. There are multiple ways to [reduce boiler plate code](#reducing-boiler-plate-code). 

 Attribute          | Description |     
:-                  |:-     |      
`[MOptions]`  | Contains (almost) all of the options below. |
`[MFormat]`  | Custom format string used to display the members value if possible.|
`[MLabel]`  | Custom label for the member (otherwise humanized name).|
`[MFontSize]`  | Set the font size for the element.|
`[MFontName]`           | Pass the name of a custom font style that will be used for the target member.|
`[MGroupName]`           | Set the group for the element. |
`[MGroupElement]`           | Whether or not the unit should be wrapped in an object or type group. |
`[MShowIndexer]`  | If the member is a collection, determine if the index of individual elements should be displayed or not. |
`[MElementIndent]`  | The indent of individual elements of a displayed collection. |
`[MPosition]`  | The preferred position of an individual UI element on the canvas. |
`[MTextAlign]`  | Horizontal text align. |
`[MOrder]`          | Relative vertical order of the displayed element. |
`[MRichText]`          | Override local RichText settings. |
`[MTextColor]`      | Set the text color for the element. |
`[MBackgroundColor]`| Set the background color for the element. |
`[MGroupColor]`     | Set the background color for the elements group. |
`[MStyle]`          | UIToolkit only. Provide optional style names. |

The `[MOptions]` attribute contains almost all of the other options. Individual attributes will override settings passed with the `[MOptions]` attribute but depending on you preferences you can either use multiple attributes or just the `[MOptions]` attribute.

```c#
// Will be displayed as "Value: 3.141"
[Monitor]
[MOptions(Format = "0.000", FontSize = 16)]
private float pi = 3.14159265359;

// Effectively the same as
[Monitor]
[MFormat("0.000")]
[MFontSize(16)]
private float pi = 3.14159265359;
```

### Reducing Boiler Plate Code
+ Creating a custom attribute and inheriting from `MOptionsAttribute` will let you use the constructor of the custom attribute to set multiple values. The attribute can then be used on multiple monitored members to apply its settings.
+ Any formatting or meta attribute applied to a class is also applied to every monitored member declared within it. Attributes directly applied to a member within such a class will always override the class scoped values meaning that you can still apply individual options to members within such a class. The `[MTag]` attribute is the only attribute that will is not overridden but added. This means that tags applied to a class scope will apply to every monitored member of that class even if these members have a custom tag attribute themselves.

> The three code segments below effectively all do the same but with different approaches. 

```c#
class MyClass 
{
	[Monitor]
	[MFormat("0.000")]
	[MFontSize(16)]
	[MPosition(UIPosition.UpperRight)]
	[MGroupElement(false)]
	[MTag("Gameplay")]
	pubic int value1;
	
	[Monitor]
	[MFormat("0.000")]
	[MFontSize(16)]
	[MPosition(UIPosition.UpperRight)]
	[MGroupElement(false)]
	[MTag("Gameplay")]
	pubic int value2;
	
	[Monitor]
	[MFormat("0.000")]
	[MFontSize(16)]
	[MPosition(UIPosition.UpperRight)]
	[MGroupElement(false)]
	[MTag("Gameplay")]
	pubic int value3;
}
```

```c#
public class MGameplayValues : MOptionsAttribute
{
	public MCustomFormatting()
	{
		Format = "0.00";
		FontSize = 16;
		Position = UIPosition.UpperRight;
		GroupElement = false;
		Tags = new string[] {"Gameplay"};
	}
}

class MyClass
{
	[Monitor]
	[MGameplayValues]
	pubic int value1;
	
	[Monitor]
	[MGameplayValues]
	pubic int value2;
	
	[Monitor]
	[MGameplayValues]
	pubic int value3;
}

```

```c#
[MFormat("0.000")]
[MFontSize(16)]
[MPosition(UIPosition.UpperRight)]
[MGroupElement(false)]
[MTag("Gameplay")]
class MyClass 
{
	[Monitor]
	pubic int value1;

	[Monitor]
	pubic int value2;
	
	[Monitor]
	pubic int value3;
}
```

![example](https://johnbaracuda.com/media/img/monitoring/Example_formatting_01.png)

 >Note that this is for display purposes only. There is no need to apply that many formatting options to every monitored member. Formatting is 100% optional.



&nbsp;
## UI Filtering
You can filter the currently displayed elements using the monitoring filter API from `IMonitoringUI` . Filtering will check for a variety of matches. If you want more explicit filtering you can disable most of these checks by navigating to (menu: Tools > Monitoring > Settings > Filtering). You can also use the settings to determine if filtering should be case sensitive or case insensitive. By default filtering is case insensitive! 

Optional Filter    | Description |        
:--                 |:-                                              
Filter Label   | Enable filtering using the displayed label. (case insensitive) This option is the most intuitive. |
Filter Static or Instance | Filter for static or non static member with *static* or *instance* |
Filter Type   | Enable filtering using the name of the type of the monitored member. (e.g. *bool*, *int*, *queue* etc.)|
Filter Declaring Type | Enable filtering using the name of the members declaring type. (e.g. *MonoBehaviour*, *Player*, *GameController* etc.)|
Filter Member Type | Enable filtering using the member type (*Field*, *Property*, *Event*, *Method*)|
Filter Tags | Enable filtering using tags applied by the `[MTag]` attribute. [more](#attributes)|

### Filter API
```c#
// Apply filter. (This filter will only show fields and properties)
MonitoringSystems.Resolve<IMonitorUI>().ApplyFilter("Field & Property");

// Reset filter.
MonitoringSystems.Resolve<IMonitorUI>().ResetFilter();
```

&nbsp;
### Absolute Filtering
Filter stating with a `@` are always case sensitive and only use the actual name of the monitored member.

### Tag only Filtering
Filter stating with a `$` only use tags applied with the custom `[MTag]` attribute. [more](#attributes)

### Combining Filters 
Use a  `&` symbol to combine multiple filters.

### Negation Filter
Append an `!` to the beginning of a filter to negate it. 

&nbsp;
You can change all of the symbols mentioned above in the monitoring settings by navigating to (menu: Tools > RuntimeMonitoring > Settings > Filtering).

&nbsp;
> This example shows a custom filtering setup in the example scene. I would recommend to test it yourself.






&nbsp; 
## Custom UI Controller

You can create a custom UI controller by following the steps below. You can take a look at the existing UI Controller implementations to get some reference. 

+ Create a new class and inherit from ```MonitoringDisplayController```.
+ Implement the abstract methods and create custom UI logic. 
+ Add the script to a new GameObject and create a prefab of it.
+ Make sure to delete the GameObject from your scene.
+ Open the settings by navigating to (menu: Tools > Monitoring > Settings).
+ Set your prefab as the active controller in the ```Moniotoring UI Controller``` field.

The following abstract member have to be implemented when inheriting from `MoniotoringUIController`.

Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`bool IsVisible()`|Return true if the UI is active and false if it is not active.|
`void ShowMonitoringUI()`|Activate / show the UI.|
`void HideMonitoringUI()`|Deactivate / hide the UI.|
`void OnUnitCreated(IMonitorUnit unit)`|Use to add UI elements for the passed unit.|
`void OnUnitDisposed(IMonitorUnit unit)`|Use to remove UI elements for the passed unit.|








&nbsp;
# Systems and API

Get the interface for a monitoring system by calling ```MonitoringSystems.Resolve<TInterface>()```. Interface implementations will not change during runtime meaning it is safe to cache them when necessary. 

System Interface    | Description |        
:--                 |:-                                              
`IMonitoringManager`   | Core access point for the system. Contains initialization and unit registry. [more](#monitoring-manager)|
`IMonitoringUI`       | Access the active monitoring UI. [more](#monitoring-ui) |
`IMonitoringSettigns`  | Access current configuration of the plugin. |
`IMonitoringPlugin`    | Access static information about the plugin. |
`IMonitoringUtility`   | Interface providing various uncategorized utility access points. [more](#monitoring-utility) |

```c#
//Example how to get / resolve a monitoring system interface.
IMonitoringUI monitoringUI = MonitoringSystems.Resolve<IMonitoringUI>();
```

### Type Interfaces

Important internally used types implement interfaces that should make it more easy to work with and extend this tool. These interfaces are especially used when creating a custom UI controller.

Type Interface    | Description |        
:--                 |:-                                              
`IMonitorUnit` |Internally used handle for instances of a monitored member. [more](#monitoring-manager)|
`IMonitorProfile` |Internally used profile describing a monitored member. [more](#monitoring-ui) |



&nbsp;
## Monitoring Manager

The `IMonitoringManager` interface provides core access points.

Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`bool IsInitialized { get; }`|Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.|
`event ProfilingCompletedListener ProfilingCompleted`|Event is invoked when profiling process for the current system has been completed. Subscribing to this event will instantly invoke a callback if profiling has already completed.|
`event Action<IMonitorUnit> UnitCreated`|Event is called when a new MonitorUnit was created. |
`event Action<IMonitorUnit> UnitDisposed`|Event is called when a MonitorUnit was disposed.|
`void RegisterTarget<T>(T target) where T : class`|Register an object that is monitored during runtime.|
`void UnregisterTarget<T>(T target) where T : class`|Unregister an object that is monitored during runtime.|
`ReadOnlyList<IMonitorUnit> GetStaticUnits();`|Get a list of MonitorUnits for static targets.|
`ReadOnlyList<IMonitorUnit> GetInstanceUnits();`|Get a list of MonitorUnits for instance targets.|
`ReadOnlyList<IMonitorUnit> GetAllMonitoringUnits();`|Get a list of all MonitorUnits.|



&nbsp;
## Monitoring Settings

Use the `IMonitoringSettigns` API can be used to access the current configuration set in the monitoring settings window. This API is readonly and mostly used by internal processes. So far it is available for transparency reasons only.

![example](https://johnbaracuda.com/media/img/monitoring/Example_settings_01.png)


&nbsp;
## Monitoring UI API

Use the `IMonitoringUI` API to access the active MonitoringUIController and set properties like visibility, filtering etc.   [more](#monitoring-ui)

Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`void Show()`|Show the monitoring UI overlay.|
`void Hide()`|Hide the monitoring UI overlay.|
`void ToggleDisplay()`|Toggle the visibility of the active monitoring display.|
`bool IsVisible()`|True if monitoring UI is visible.|
`T GetActiveUIController<T>()`|Get the currently active MonitoringUIController casted to a concrete implementation.|
`void CreateMonitoringUI()`|Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController is instantiated. |
`void ApplyFilter(string filter)`|Filter displayed units by their name, tags etc. [more](#ui-filtering)|
`void ResetFilter()`|Reset active filter. [more](#ui-filtering)|
`event Action<bool> VisibleStateChanged()`|Event invoked when the monitoring UI became visible or invisible.|



&nbsp;
## Monitoring Utility

The `IMonitoringUtility` interface provides a variety of uncategorized utility functions. 

Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`bool IsFontHashUsed(int fontHash)`|Returns true if the passed hash from the name of a font asset is used by a MFontAttribute and therefore required by a monitoring unit. Used to dynamically load/unload required fonts.|
`IMonitorUnit[] GetMonitorUnitsForTarget(object target)`|Get a list of IMonitorUnits registered to the passed target object.|
`IReadOnlyCollection<string> GetAllTags()`| Get a list of all custom tags, applied by `[MTag]` attributes that can be used for filtering.|



&nbsp;
## Monitor Unit

The  `IMonitorUnit` interface offers access points to the internally used handle for instances of a monitored member. 

Interface&#160;Member&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`string Name { get; }`|Readable display name.|
`object Target { get; }`|The target object of the monitored member. Null if static.|
`IMonitorProfile Profile { get; }`|Profile describing the monitored member. [more](#monitor-profile) |
`bool Enabled { get; set; }`|The active state of the unit. Only enabled units are updated and displayed.|
`int UniqueID { get; }`|Unique Id|
`event Action<bool> ActiveStateChanged`|Event is invoked when the units active state has changed.|
`event Action<string> ValueUpdated`|Event is invoked when the value of the monitored member has changed.|
`event Action Disposing`|Event is invoked when the unit is being disposed.|
`void Refresh()`|Force the monitored member to update its state.|
`string GetState()`|Get the current value or state of the monitored member as a formatted string.|



&nbsp;
## Monitor Profile

The  `IMonitorProfile` interface offers access points to the internally used profile describing a monitored member. It contains cached reflection and formatting data.  

Interface&#160;Membe&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;| Description                                                           |        
:---         |:-                                        
`MonitorAttribute Attribute { get; }`|Instance of the attribute that is used to flag the target member to be monitored.|
`MemberInfo MemberInfo { get; }`|Member info of the monitored member.|
`UnitType UnitType { get; }`|The type of the member (either field, property, event or method)|
`bool ReceiveTick { get; }`|True if the unit receives a custom tick event.|
`Type UnitTargetType { get; }`|The declaring type of the monitored member.|
`Type UnitValueType { get; }`|The type of the monitored member.|
`bool IsStatic { get; }`|Indicates if monitored member is static or non static.|
`IFormatData FormatData { get; }`|Contains information about label, font and more formatting data.|
`string[] Tags { get; }`|Additional meta data for filtering.|
` T GetMetaAttribute<T>()`|The monitoring profiler caches every additional attribute that inherits from MonitoringMetaAttribute on the profile. You can access these custom attributes during runtime using this method without using reflection.|
`bool TryGetMetaAttribute<T>(out T attribute)`|Try to get a MonitoringMetaAttribute.|






&nbsp;
# Compatibility
If you encounter any compatibility issues please create an Issue on GitHub. Runtime Monitoring aims to provide compatibility for every platform, scripting backend & Unity version.



&nbsp;
## Runtime Compatibility

Both Mono and IL2CPP runtimes are supported. RTM is making extensive use of dynamic type and method creation during its profiling process. In order to create these types, IL2CPP requires AOT compilation (Ahead of time compilation) When using IL2CPP runtime a list of types is generated shortly before a build to give the compiler the necessary information to generate everything it needs during runtime. You can manually create this list form the settings window.

![example](https://johnbaracuda.com/media/img/monitoring/Example_08.png)




&nbsp;
## Platform Compatibility

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
## Optimizations

In general RTM tries to be as optimized as possible by reducing allocations where ever possible and by doing a lot of the heavy work during the initial profiling. However due to the nature of some types and the creation of strings, allocations cannot be prevented. If performance is a sensitive aspect of your project, here are some tips and tricks to keep in mind.

### Reference Types & Value Types
Since there is no easy way to check whether the actual value of a reference type has changed when it is evaluated, a monitored Reference Type is processed with ToString() each time it is evaluated. This, by default happens either every Update or Tick cycle and may generate a lot of garbage in a very short time, without much scope for automatic optimization. For this reason, it is recommended to pass an UpdateEvent for the monitored value whenever possible to reduce memory allocations. Of course, this shouldn't matter much if you're only debugging one value, but could be detrimental if you want to keep monitoring your member in a release or shipped build. The same is not true for monitored Value Types, as these are compared to a cached value to ensure that an update event and a string creation is only triggered when the value has actually changed.


### Collections
Because collections are Reference Types the same applies here but on an even greater scale. Pass an update event when ever possible if you intend to monitor a collection over a longer period. Now because the example below requires a lot of boiler plate code I would not recommend this if you just want to quickly debug the values of a collection. I also want to mention that a better solution is planned and WIP.  

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
# Frequently Asked Questions



&nbsp;
### How can I disable the tool in a release?

Use the ```#define DISABLE_MONITORING``` to disable the internal logic of the tool. 
Public API will still compile so you don't have to wrap your API calls in a custom ```#if !DISABLE_MONITORING``` block.




&nbsp;
### How can I uninstall the tool?

You can just remove the plugin by deleting the folder Assets/Baracuda. 




&nbsp;
### Are there any planned features?

+ Add to OpenUPM
+ Monitoring Editor Window for edit time monitoring.
+ Monitored collection type with "dirty" flag. (Optimization)
+ Option to create "virtual" units during runtime.
+ Tutorial how to use.
+ Guide how to create a custom UI Controller.
+ Guide how to customize UI.



&nbsp;
### Assemblies and Modules?

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
### What is Thread Dispatcher?

Thread Dispatcher is another free & open source tool I developed to to pass the execution of a Delegate, Coroutine or Task from a background thread to the main thread, and await its completion or result on the calling thread as needed. Runtime Monitoring optionally uses a background thread for initial assembly profiling & setup processes. You can find more information about it on its [GitHub Repository](https://github.com/JohnBaracuda/Thread-Dispatcher),  its [Documentation](https://johnbaracuda.com/dispatcher.html) and its page on the [Asset Store](https://assetstore.unity.com/packages/slug/202421). I would also appreciate if you would leave a ❤️ or a ⭐on its page because it is also something I spent a lot of time working on.




&nbsp;
## Support Me

I spend a lot of time working on this and other free assets to make sure as many people as possible can use my tools regardless of their financial status. Any kind of support I get helps me keep doing this, so consider leaving a star ⭐ making a donation or follow me on my socials to support me ❤️

+ [Donation (PayPal.me)](https://www.paypal.com/paypalme/johnbaracuda)
+ [Linktree](https://linktr.ee/JohnBaracuda)
+ [Twitter](https://twitter.com/JohnBaracuda)
+ [Itch](https://johnbaracuda.itch.io/)
