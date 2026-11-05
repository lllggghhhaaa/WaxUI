using Microsoft.AspNetCore.Components;

namespace WaxUI.Accordion;

public class WaxAccordionItemData
{
    public string Title { get; set; } = string.Empty;
    public MarkupString Body { get; set; }
    public RenderFragment? Icon { get; set; }
}