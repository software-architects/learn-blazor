+++
title = "Dynamic Content"
weight = 30
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Dynamic Component

Sometimes you want create HTML using an algorithm instead of a template. Think of a chess board. It would be boring to create the HTML table by hand. In Blazor, you can ignore the template and create the component fully in C#. The following code sample demonstrates that. It creates a tic-tac-toe board with nine cells and some CSS classes for formatting.

The class that is used for dynamically generating content is `Microsoft.AspNetCore.Blazor.RenderTree.RenderTreeBuilder` ([source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/RenderTree/RenderTreeBuilder.cs)). It contains methods to open elements, add attributes, add content, add components, etc. Take a look at the source code or use IntelliSense in Visual Studio to see all available render methods.

Note that you can put this class in your project's *Pages* folder where the rest of your Blazor templates are. Blazor's router will work just fine for your code-only component.

```cs
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace BlazorPages.Pages
{
    public class DynamicRenderTree : BlazorComponent
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var seq = 0;
            builder.OpenElement(seq, "table");
            builder.OpenElement(++seq, "tbody");

            for (var row = 0; row < 3; row++)
            {
                builder.OpenElement(++seq, "tr");
                for (var col = 0; col < 3; col++)
                {
                    builder.OpenElement(++seq, "td");
                    builder.AddAttribute(++seq, "class", "tictactoe-cell");
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }
    }
}
```

## Render Fragments

It is not necessary to build the entire component in C#. You can also dynamically generate fragments as shown in the following example. Note that the method creating the render fragment (delegate in `DynamicFragment`) is called whenever a rendering occurs, not just during component load.

```cs
<h1>Welcome</h1>

<p>Lorem ipsum...</p>

<button type="button" @onclick(() => ShouldRender())>
    Trigger rendering (i.e. makes text longer)
</button>

@DynamicFragment

<p>Lorem ipsum...</p>

@functions {
    private string dynamicContent = "This is a long text...";

    protected override void OnInit()
    {
        DynamicFragment = builder =>
        {
            // Make the text longer every time this delegate is called
            dynamicContent = dynamicContent.Replace("long", "long long");

            builder.OpenElement(1, "p");
            builder.AddContent(2, dynamicContent);
            builder.CloseElement();
        };
    }

    private Microsoft.AspNetCore.Blazor.RenderFragment DynamicFragment;
}
```
