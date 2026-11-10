using Microsoft.JSInterop;

namespace WaxUI.Services;

/// <summary>
/// Lazy ES-module bridge shared by WaxUI's runtime features. The module is imported on
/// first use and reused afterwards; every call degrades to <c>default</c> when JS is
/// unavailable (prerendering, a disconnected circuit or a failing module call) so
/// components never have to wrap interop in their own guards.
/// </summary>
internal sealed class WaxJsModule : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference?>> _module;
    private bool _unavailable;

    public WaxJsModule(IJSRuntime jsRuntime, string modulePath)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);

        _module = new Lazy<Task<IJSObjectReference?>>(
            () => LoadModuleAsync(jsRuntime, modulePath),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>Invokes a module export; returns <c>default</c> when the call cannot run.</summary>
    public async Task<T?> InvokeAsync<T>(string identifier, params object?[] args)
    {
        if (_unavailable)
        {
            return default;
        }

        try
        {
            var module = await _module.Value;
            return module is null ? default : await module.InvokeAsync<T>(identifier, args);
        }
        catch (JSException)
        {
            return default;
        }
        catch (JSDisconnectedException)
        {
            _unavailable = true;
            return default;
        }
        catch (ObjectDisposedException)
        {
            _unavailable = true;
            return default;
        }
        catch (InvalidOperationException)
        {
            // Prerendering or a disposed circuit: JS interop is not usable right now.
            _unavailable = true;
            return default;
        }
        catch (OperationCanceledException)
        {
            _unavailable = true;
            return default;
        }
    }

    /// <summary>Invokes a module export without caring about its result.</summary>
    public Task InvokeVoidAsync(string identifier, params object?[] args) =>
        InvokeAsync<object?>(identifier, args);

    public async ValueTask DisposeAsync()
    {
        if (!_module.IsValueCreated)
        {
            return;
        }

        try
        {
            var module = await _module.Value;
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private async Task<IJSObjectReference?> LoadModuleAsync(IJSRuntime jsRuntime, string modulePath)
    {
        try
        {
#pragma warning disable BL0016 // Import failures are relayed through the guarded public boundaries that use this helper.
            return await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
#pragma warning restore BL0016
        }
        catch (JSException)
        {
            _unavailable = true;
            return null;
        }
        catch (JSDisconnectedException)
        {
            _unavailable = true;
            return null;
        }
        catch (ObjectDisposedException)
        {
            _unavailable = true;
            return null;
        }
        catch (InvalidOperationException)
        {
            _unavailable = true;
            return null;
        }
        catch (OperationCanceledException)
        {
            _unavailable = true;
            return null;
        }
    }
}
