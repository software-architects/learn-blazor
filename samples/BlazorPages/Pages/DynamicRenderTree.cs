using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace BlazorPages.Pages
{
    [Route("/dynamic-render-tree")]
    public class DynamicRenderTree : BlazorComponent
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "table");
            builder.OpenElement(1, "tbody");

            for (var row = 0; row < 3; row++)
            {
                builder.OpenElement(2, "tr");
                for (var col = 0; col < 3; col++)
                {
                    builder.OpenElement(3, "td");
                    builder.AddAttribute(4, "class", "tictactoe-cell");
                    builder.CloseElement();
                }

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }
    }
}
