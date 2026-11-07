using Microsoft.AspNetCore.Components;
using WaxUI.Services;

namespace WaxUI.Dialog;

/// <summary>
/// Default <see cref="IWaxDialogService"/>: keeps a stack of open dialogs, resolves
/// their <c>ShowAsync</c> tasks through the contexts and notifies the outlet.
/// Registered scoped (one stack per circuit/user flow) by AddWaxUI.
/// </summary>
public sealed class WaxDialogService : IWaxDialogService
{
    private readonly WaxInstanceStore<WaxDialogInstance> _dialogs = new();

    public event Action? Changed
    {
        add => _dialogs.Changed += value;
        remove => _dialogs.Changed -= value;
    }

    public IReadOnlyList<WaxDialogInstance> OpenDialogs => _dialogs.Items;

    public Task<WaxDialogResult<T>> ShowAsync<T>(
        RenderFragment<WaxDialogContext<T>> content,
        WaxDialogOptions? options = null,
        object? state = null) =>
        ShowCoreAsync<T>(content, null, options, state);

    public Task<WaxDialogResult<T>> ShowAsync<T>(
        Type contentComponent,
        WaxDialogOptions? options = null,
        object? state = null)
    {
        if (!typeof(IWaxDialogContent<T>).IsAssignableFrom(contentComponent))
        {
            throw new ArgumentException(
                $"{contentComponent.Name} must implement IWaxDialogContent<{typeof(T).Name}> "
                + "to be used as service-driven dialog content.", nameof(contentComponent));
        }

        return ShowCoreAsync<T>(null, contentComponent, options, state);
    }

    public async Task<bool> ConfirmAsync(WaxConfirmRequest request)
    {
        var result = await ShowCoreAsync(
            EmptyContent<bool>(),
            null,
            new WaxDialogOptions
            {
                Title = request.Title,
                Description = request.Description,
                DescriptionContent = request.DescriptionContent,
                Class = request.Class,
                CloseOnBackdrop = request.CloseOnBackdrop,
                CloseOnEscape = request.CloseOnEscape,
                ConfirmText = request.ConfirmText ?? "Confirm",
                CancelText = request.CancelText ?? "Cancel",
                Danger = request.Danger,
            },
            null,
            true);

        return !result.Cancelled;
    }

    public Task<bool> ConfirmAsync(string title, string description) =>
        ConfirmAsync(new WaxConfirmRequest { Title = title, Description = description });

    public async Task<string?> PromptAsync(WaxPromptRequest request)
    {
        var result = await ShowCoreAsync(
            PromptContent(request),
            null,
            new WaxDialogOptions { UseShell = false, CloseOnBackdrop = false },
            null);

        return result.Cancelled ? null : result.Value;
    }

    public void CloseAll()
    {
        foreach (var dialog in _dialogs.Items)
        {
            dialog.Cancel();
        }
    }

    private static RenderFragment<WaxDialogContext<T>> EmptyContent<T>() => _ => _ => { };

    private static RenderFragment<WaxDialogContext<string?>> PromptContent(WaxPromptRequest request) =>
        context => builder =>
        {
            builder.OpenComponent(0, typeof(WaxPromptBody));
            builder.AddAttribute(1, nameof(WaxPromptBody.Context), context);
            builder.AddAttribute(2, nameof(WaxPromptBody.Request), request);
            builder.CloseComponent();
        };

    private Task<WaxDialogResult<T>> ShowCoreAsync<T>(
        RenderFragment<WaxDialogContext<T>>? content,
        Type? contentComponent,
        WaxDialogOptions? options,
        object? state,
        T confirmValue = default!)
    {
        var context = new WaxDialogContext<T> { State = state };
        var effectiveOptions = options ?? new WaxDialogOptions();

        var body = contentComponent is null
            ? BuildFragmentBody(context, content ?? EmptyContent<T>())
            : BuildComponentBody(context, contentComponent);

        Func<Task>? confirm = effectiveOptions.ConfirmText is null
            ? null
            : () =>
            {
                context.Close(confirmValue);
                return Task.CompletedTask;
            };

        var instance = new WaxDialogInstance(effectiveOptions, body, context.Cancel, confirm);

        _dialogs.Add(instance);
        _ = RemoveWhenResolvedAsync(context, instance);

        return context.Task;
    }

    private async Task RemoveWhenResolvedAsync<T>(WaxDialogContext<T> context, WaxDialogInstance instance)
    {
        await context.Task;
        _dialogs.Remove(instance);
    }

    private static RenderFragment BuildFragmentBody<T>(
        WaxDialogContext<T> context, RenderFragment<WaxDialogContext<T>> content) => builder =>
    {
        builder.OpenComponent<CascadingValue<WaxDialogContext<T>>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<WaxDialogContext<T>>.Value), context);
        builder.AddAttribute(2, nameof(CascadingValue<WaxDialogContext<T>>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<WaxDialogContext<T>>.ChildContent),
            (RenderFragment)(inner => inner.AddContent(0, content(context))));
        builder.CloseComponent();
    };

    private static RenderFragment BuildComponentBody<T>(WaxDialogContext<T> context, Type component) => builder =>
    {
        builder.OpenComponent<CascadingValue<WaxDialogContext<T>>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<WaxDialogContext<T>>.Value), context);
        builder.AddAttribute(2, nameof(CascadingValue<WaxDialogContext<T>>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<WaxDialogContext<T>>.ChildContent),
            (RenderFragment)(inner =>
            {
                inner.OpenComponent(0, component);
                inner.AddAttribute(1, "Context", context);
                inner.CloseComponent();
            }));
        builder.CloseComponent();
    };
}
