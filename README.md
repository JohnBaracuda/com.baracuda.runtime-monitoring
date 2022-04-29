Monitoring
===

Runtime Monitoring is an open source tool that provides an easy way to monitor the value or state of custom C# members. Just add the 'Monitor' attribute to a field, property, event or method and get its value or state displayed during runtime. Runtime Monitoring aims to be easy to use, lightweight and customizable. It is fully compatible with Mono & IL2CPP!

## Table of Contents

- [Getting started](#getting-started)
- [Basics of Runtime Monitoring](#basics-of-monitoring)
- [Monitor Fields](#monitor-fields)
- [Monitor Properties](#monitor-properties)
- [Monitor Events](#monitor-events)
- [Monitor Methods](#monitor-methods)
- [Monitoring API](#api)
- [Runtime](#runtime)
  - [IL2CPP Runtime](#il2cpp-runtime)
  - [Mono Runtiome](#mono-runtime)
- [Display / UI](#ui)
  - [UI Toolkit](#ui-toolkit)
  - [Unity UI](#ui-ugui)
  - [GUI](#ui-gui)
  - [Custom UI / Display Implimentation](#ui-create)
- [Licence](#licence)


### Basics
```c#
[Monitor]
private int healthPoints;

[Monitor]
public int HealthPoints { get; private set; }

[Monitor]
public event OnHealthChanged;
```

### Features