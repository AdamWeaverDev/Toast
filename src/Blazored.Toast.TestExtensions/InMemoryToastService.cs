using Blazored.Toast.Configuration;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;

namespace Blazored.Toast.TestExtensions;

public class InMemoryToastService : IToastService
{
    private readonly List<InMemoryToast> _toasts = new();
    public IReadOnlyList<InMemoryToast> Toasts => _toasts;
    
    public event Action<Type, ToastParameters?, Action<ToastSettings>?>? OnShowComponent;
    public event Action<ToastLevel, RenderFragment, Action<ToastSettings>?>? OnShow;
    public event Action? OnClearAll;
    public event Action<ToastLevel>? OnClearToasts;
    public event Action? OnClearCustomToasts;
    public event Action? OnClearQueue;
    public event Action<ToastLevel>? OnClearQueueToasts;

    public void ShowToast<TComponent>() where TComponent : IComponent
    {
        _toasts.Add(new InMemoryToast(typeof(TComponent)));
        OnShowComponent?.Invoke(typeof(TComponent), new ToastParameters(), null);
    }

    public void ShowToast<TComponent>(ToastParameters parameters) where TComponent : IComponent
    {
        _toasts.Add(new InMemoryToast(typeof(TComponent), parameters));
        OnShowComponent?.Invoke(typeof(TComponent), parameters, null);
    }

    public void ShowToast<TComponent>(Action<ToastSettings>? settings) where TComponent : IComponent
    {
        _toasts.Add(new InMemoryToast(typeof(TComponent)));
        OnShowComponent?.Invoke(typeof(TComponent), null, settings);
    }

    public void ShowToast<TComponent>(ToastParameters parameters, Action<ToastSettings>? settings) where TComponent : IComponent
    {
        _toasts.Add(new InMemoryToast(typeof(TComponent), parameters));
        OnShowComponent?.Invoke(typeof(TComponent), parameters, settings);
    }

    public void ShowError(string message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Error, message, settings);

    public void ShowError(RenderFragment message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Error, message, settings);

    public void ShowInfo(string message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Info, message, settings);

    public void ShowInfo(RenderFragment message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Info, message, settings);

    public void ShowSuccess(string message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Success, message, settings);

    public void ShowSuccess(RenderFragment message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Success, message, settings);

    public void ShowToast(ToastLevel level, string message, Action<ToastSettings>? settings = null)
        => ShowToast(level, builder => builder.AddContent(0, message), settings);

    public void ShowToast(ToastLevel level, RenderFragment message, Action<ToastSettings>? settings = null)
    {
        _toasts.Add(new InMemoryToast(typeof(ToastInstance), level, message));
        OnShow?.Invoke(level, message, settings);
    }

    public void ShowWarning(string message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Warning, message, settings);

    public void ShowWarning(RenderFragment message, Action<ToastSettings>? settings = null)
        => ShowToast(ToastLevel.Warning, message,  settings);

    public void ClearAll()
    {
        _toasts.Clear();
        OnClearAll?.Invoke();
    }

    public void ClearToasts(ToastLevel toastLevel)
    {
        _toasts.RemoveAll(x => x.ToastType == typeof(ToastInstance) && x.ToastLevel == toastLevel);
        OnClearToasts?.Invoke(toastLevel);
    }

    public void ClearWarningToasts()
        => ClearToasts(ToastLevel.Warning);

    public void ClearInfoToasts()
        => ClearToasts(ToastLevel.Info);

    public void ClearSuccessToasts()
        => ClearToasts(ToastLevel.Success);

    public void ClearErrorToasts()
        => ClearToasts(ToastLevel.Error);

    public void ClearCustomToasts()
    {
        _toasts.RemoveAll(x => x.ToastType != typeof(ToastInstance));
        OnClearCustomToasts?.Invoke();
    }

    // The fake has no waiting queue (every toast is recorded immediately), so there is
    // nothing to remove; subscribers are still notified, as with the real ToastService.
    public void ClearQueue()
        => OnClearQueue?.Invoke();

    public void ClearQueueToasts(ToastLevel toastLevel)
        => OnClearQueueToasts?.Invoke(toastLevel);

    public void ClearQueueWarningToasts()
        => ClearQueueToasts(ToastLevel.Warning);

    public void ClearQueueInfoToasts()
        => ClearQueueToasts(ToastLevel.Info);

    public void ClearQueueSuccessToasts()
        => ClearQueueToasts(ToastLevel.Success);

    public void ClearQueueErrorToasts()
        => ClearQueueToasts(ToastLevel.Error);
}
