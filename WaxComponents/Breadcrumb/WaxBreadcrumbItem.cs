namespace WaxComponents.Breadcrumb;

public record WaxBreadcrumbItem(
    string Label,
    string? Href = null,
    string? Icon = null
);
