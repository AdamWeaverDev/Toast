using Blazored.Toast.Services;
using Blazored.Toast.TestExtensions;
using Xunit;

namespace Blazored.Toast.Tests.ToastServiceTests;

public class InMemoryToastServiceEventTests
{
    private readonly InMemoryToastService _sut = new();

    [Fact]
    public void OnShow_IsRaised_WithLevelAndSettings_WhenToastShown()
    {
        ToastLevel? level = null;
        Action<Blazored.Toast.Configuration.ToastSettings>? received = null;
        Action<Blazored.Toast.Configuration.ToastSettings> settings = _ => { };
        _sut.OnShow += (l, _, s) => { level = l; received = s; };

        _sut.ShowWarning("hi", settings);

        Assert.Equal(ToastLevel.Warning, level);
        Assert.Same(settings, received);
        Assert.Single(_sut.Toasts);
    }

    [Fact]
    public void OnShowComponent_IsRaised_WhenComponentToastShown()
    {
        Type? type = null;
        ToastParameters? parameters = null;
        _sut.OnShowComponent += (t, p, _) => { type = t; parameters = p; };
        var toastParameters = new ToastParameters();

        _sut.ShowToast<MyTestComponent>(toastParameters);

        Assert.Equal(typeof(MyTestComponent), type);
        Assert.Same(toastParameters, parameters);
    }

    [Fact]
    public void ClearEvents_AreRaised()
    {
        var raised = new List<string>();
        _sut.OnClearAll += () => raised.Add("all");
        _sut.OnClearCustomToasts += () => raised.Add("custom");
        _sut.OnClearQueue += () => raised.Add("queue");
        _sut.OnClearToasts += l => raised.Add("toasts:" + l);
        _sut.OnClearQueueToasts += l => raised.Add("queueToasts:" + l);

        _sut.ClearAll();
        _sut.ClearCustomToasts();
        _sut.ClearQueue();
        _sut.ClearErrorToasts();
        _sut.ClearQueueInfoToasts();

        Assert.Equal(new[] { "all", "custom", "queue", "toasts:Error", "queueToasts:Info" }, raised);
    }

    [Fact]
    public void ClearQueueMethods_DoNotThrow()
    {
        _sut.ClearQueue();
        _sut.ClearQueueToasts(ToastLevel.Info);
        _sut.ClearQueueWarningToasts();
        _sut.ClearQueueSuccessToasts();
        _sut.ClearQueueErrorToasts();
    }
}
