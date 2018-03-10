+++
title = "Data Binding"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## One-Way Data Binding

If you know [Razor](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor) from ASP.NET, Razor in Blazor will be quite straight forward for you. The following example shows how you can do one-way data binding in Blazor templates. Note that it is not necessary to trigger UI refresh manually because the change is triggered by a button click. Blazor recognizes changes in that case automatically.

I have been doing quite a lot of Angular work. Therefore, I added some comments about how the data binding scenarios relate to Angular constructs that you maybe know.

```cs
<!-- Use this button to trigger changes in the source values -->
<button @onclick(ChangeValues)>Change values</button>

<!-- 
    Simple interpolation
    In Angular, this would be {{ Count }}
-->
<p>Counter: @Count</p>

<!-- 
    Conditionally display content 
    In Angular, this would be *ngIf
-->
@if (ShowWarning)
{
    <p style="background-color: red; padding: 5px">Warning!</p>
}

<!--
    Style binding
    In Angular, you would do that with [style.backgroundColor]="..."
-->
<p style="background-color: @Background; color=white; padding: 5px">Notification</p>

<!--
    Add/remove class
    In Angular, you would do that with [class.highlight]="..."
-->
<p class="note @((NoteIsActive ? "highlight" : ""))">This is a note</p>

<!--
    Bind to a collection 
    In Angular, you would do that with *ngFor
-->
<ul>
    @foreach(var number in Numbers)
    {
        <li>@number</li>
    }
</ul>

@functions {
    private int Count { get; set; } = 0;
    private bool ShowWarning { get; set; } = true;
    private string Background { get; set; } = "red";
    private bool NoteIsActive { get; set; } = true;
    private List<int> Numbers { get; set; } = new List<int> { 1, 2, 3 };

    private void ChangeValues()
    {
        Count++;
        ShowWarning = !ShowWarning;
        Background = Background == "red" ? "green" : "red";
        NoteIsActive = !NoteIsActive;
        Numbers.Add(Numbers.Max() + 1);
    }
}
```

## Two-Way Data Binding

Blazor already supports two-way data binding using `@bind`. The following example demonstrates some two-way data-binding scenarios. Note that at the time of writing, Blazor only supports string and boolean types for `@bind`. If you need other types (e.g. numbers), you need to provide getter and setter from/to string.

```cs
<p>
    Enter your name: <input type="text" @bind(Name)><br />
    What is your age? <input type="number" @bind(Age)><br />
    Are you an administrator? <input type="checkbox" @bind(IsAdmin)>
</p>

<h2>Hello, @Name!</h2>

<p>You are @Age years old.</p>

@if (IsAdmin)
{
    <p>You are an administrator!</p>
}

@functions {
    private string Name { get; set; } = "Tom";
    private bool IsAdmin { get; set; } = true;

    private int age = 33;
    private string Age
    {
        get => age.ToString();
        set => Int32.TryParse(value, out age);
    }
}
```

## Manually Trigger UI Refresh

Blazor detects a necessary UI refresh automatically in many scenarios (e.g. after button click). However, there are situations in which you want to trigger a UI refresh manually. Use the `BlazorComponent.StateHasChanged` method for that as shown in the following sample. It changes the application's state using a timer.

```cs
@using System.Threading;

<h1>@Count</h1>

<button @onclick(StartCountdown)>Start Countdown</button>

@functions {
    private int Count { get; set; } = 10;

    void StartCountdown()
    {
        var timer = new Timer(new TimerCallback(_ =>
        {
            if (Count > 0)
            {
                Count--;

                // Note that the following line is necessary because otherwise
                // Blazor would not recognize the state change and not refresh the UI
                this.StateHasChanged();
            }
        }), null, 1000, 1000);
    }
}
```

## Event Binding

At the time or writing, event binding is quite limited in Blazor. Just `@onclick` and `@onchange` are supported. However, the Blazor code contains a lot of *TODO* comments regarding events, so hopefully more is to come ;-)

```cs
<!-- Note that Console.WriteLine goes to the browsers console -->

<button @onclick(Clicked)>Click me</button>
<button @onclick(() => Console.WriteLine("Hello World"))>Click me</button>

<input type="text" @onchange(newValue => Console.WriteLine(newValue)) />

@functions {
    private void Clicked()
    {
        Console.WriteLine("Hello World");
    }
}
```
