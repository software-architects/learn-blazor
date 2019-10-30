+++
title = "Data Binding"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-10-02
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## One-Way Data Binding

If you know [Razor](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor) from ASP.NET, Razor in Blazor will be quite straight forward for you. The following example shows how you can do one-way data binding in Blazor templates. Note that it is not necessary to trigger UI refresh manually because the change is triggered by a button click. Blazor recognizes changes in that case automatically.

I have been doing quite a lot of Angular work. Therefore, I added some comments about how the data binding scenarios relate to Angular constructs that you maybe know.

```cs
@page "/one-way-data-binding"

<!-- Use this button to trigger changes in the source values -->
<button onclick="@ChangeValues">Change values</button>
<button onclick="@(() => ChangeValues())">Change values</button>

<!--
    Simple interpolation.
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
    Conditionally display content with a boolean property. Note that
    the hidden attribute will be removed if ShowWarning is false.
    In Angular, this would be [hidden]
-->
<p hidden="@ShowWarning">Everything is fine :-)</p>

<!--
    Style binding
    In Angular, you would do that with [style.backgroundColor]="..."
-->
<p style="background-color: @Background; color=white; padding: 5px">Notification</p>

<!--
    Add/remove class. Note that the second variant removes the class name in
    HighlightClass if the property is null.
    In Angular, you would do that with [class.highlight]="..."
-->
<p class="note @((NoteIsActive ? "highlight" : ""))">This is a note</p>
<p class="note @HighlightClass">This is a note</p>

<!--
    Bind to a collection
    In Angular, you would do that with *ngFor
-->
<ul>
    @foreach (var number in Numbers)
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
    private string HighlightClass { get; set; } = "highlight";

    private void ChangeValues()
    {
        Count++;
        ShowWarning = !ShowWarning;
        HighlightClass = HighlightClass == null ? "highlight" : null;
        Background = Background == "red" ? "green" : "red";
        NoteIsActive = !NoteIsActive;
        Numbers.Add(Numbers.Max() + 1);
    }
}
```

## Two-Way Data Binding

Blazor already supports two-way data binding using `bind=...`. The following example demonstrates some two-way data-binding scenarios.The comments in the code describe details about the used binding mechanisms:

```cs
@page "/two-way-data-binding"

<p>
    @* You can bind using @Property or @Field *@
    Enter your name: <input type="text" @bind="Name" /><br />

    @* Alternatively also using "Property" or "Field" *@
    What is your age? <input type="number" @bind="Age" /><br />

    @* Note how to pass a format for DateTime *@
    What is your birthday (culture-invariant default format)? <input type="text" @bind="Birthday" /><br />
    What is your birthday (German date format)? <input type="text" @bind="Birthday" format-value="dd.MM.yyyy" /><br />

    @* Data binding for checkboxes with boolean properties *@
    Are you an administrator? <input type="checkbox" @bind="IsAdmin" /><br />

    @* Data binding for selects with enums *@
    <select id="select-box" @bind="TypeOfEmployee">
        <option value=@EmployeeType.Employee>@EmployeeType.Employee.ToString()</option>
        <option value=@EmployeeType.Contractor>@EmployeeType.Contractor.ToString()</option>
        <option value=@EmployeeType.Intern>@EmployeeType.Intern.ToString()</option>
    </select>

    @*
        The following line would not work because decimal is not supported
        What is your salery? <input type="number" bind="Salary" /><br />
    *@
</p>

<h2>Hello, @Name!</h2>

<p>You are @Age years old. You are born on the @Birthday. You are @TypeOfEmployee.</p>

@if (IsAdmin)
{
    <p>You are an administrator!</p>
}

@functions {
    private enum EmployeeType { Employee, Contractor, Intern };
    private EmployeeType TypeOfEmployee { get; set; } = EmployeeType.Employee;

    private string Name { get; set; } = "Tom";
    private bool IsAdmin { get; set; } = true;
    private int Age { get; set; } = 33;
    public DateTime Birthday { get; set; } = DateTime.Today;

    public decimal Salary { get; set; }
}
```

## Bind Value to Child Component

You can use data binding for communication between components. Here is an example that demonstrates how to bind a value in a parent component to a child component. The child component uses to value to generate a list of value (in practice this could be e.g. due to a web api response).

*Parent:*

```cs
...
<ManualRefreshChild NumberOfElements=@CurrentValue></ManualRefreshChild>
...
@functions {
    private int CurrentValue { get; set; }
    ...
}
```

*Child:*

```cs
@using System.Collections.Generic
...
<p>You requested @NumberOfElements elements. Here they are:</p>
<ul>
    @foreach (var n in Numbers)
    {
        <li>@n</li>
    }
</ul>
...
@functions {
    ...
    [Parameter]
    public int NumberOfElements { get; set; }

    private IEnumerable<int> Numbers
    {
        get
        {
            for (var i = 0; i < NumberOfElements; i++)
            {
                yield return i;
            }
        }
    }
    ...
}
```

## Manually Trigger UI Refresh

Blazor detects a necessary UI refresh automatically in many scenarios (e.g. after button click). However, there are situations in which you want to trigger a UI refresh manually. Use the `BlazorComponent.StateHasChanged` method for that as shown in the following sample. It changes the application's state using a timer.

```cs
@page "/manual-refresh"
@using System.Threading;

<h1>@Count</h1>

<button onclick=@StartCountdown>Start Countdown</button>

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

Note that `StateHasChanged` only triggers a UI refresh for the current component. It does not automatically refresh its child or parent components.

## Event Binding

At the time or writing, event binding is quite limited in Blazor. Just `onclick` and `onchange` are supported. However, this is currently changing. You find more details in the Blazor GitHub issue [#503](https://github.com/aspnet/Blazor/issues/503).

```cs
@page "/event-binding"

<!-- Note that Console.WriteLine goes to the browsers console -->

<button onclick=@Clicked>Click me</button>
<button onclick=@(() => Console.WriteLine("Hello World"))>Click me</button>

<input type="text" onchange=@(newValue => Console.WriteLine(newValue)) />

<ChildComponent OnSomeEvent=@ChildEventClicked />

@functions {
    private void Clicked()
    {
        Console.WriteLine("Hello World");
    }

    private void ChildEventClicked()
    {
        Console.WriteLine("Child event clicked");
    }
}
```

Components can offer callbacks that parent components can use to react on events raised by their child components. Imagine the following child component:

```cs
<button onclick=@OnClick>Click me (child component)</button>

@functions {
    public Action OnSomeEvent { get; set; }

    private void OnClick()
    {
        OnSomeEvent?.Invoke();
    }
}
```

The parent component can handle on `OnSomeEvent` like this (Note that the typecast in the binding is temporary, it will not be necessary in future release of Blazor):

```cs
...
<ChildComponent OnSomeEvent=@ChildEventClicked />

@functions {
    ...
    private void ChildEventClicked()
    {
        Console.WriteLine("Child event clicked");
    }
}
```
