using System.Reflection;
using Blazored.Toast.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blazored.Toast.Tests.ComponentTests;

public class BlazoredToastsTests : TestContext
{
    private readonly ToastService _toastService = new();

    public BlazoredToastsTests()
    {
        Services.AddSingleton<IToastService>(_toastService);
    }

    private IRenderedComponent<BlazoredToasts> RenderToasts(int maxToastCount = int.MaxValue, bool removeOnNavigation = false)
        => RenderComponent<BlazoredToasts>(p => p
            .Add(x => x.DisableTimeout, true)
            .Add(x => x.MaxToastCount, maxToastCount)
            .Add(x => x.RemoveToastsOnNavigation, removeOnNavigation));

    private static IEnumerable<string> SubscribedEvents(ToastService service)
        => typeof(ToastService)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(f => f.FieldType.IsSubclassOf(typeof(Delegate)) && f.GetValue(service) is not null)
            .Select(f => f.Name);

    private static string[] Messages(IRenderedComponent<BlazoredToasts> cut)
        => cut.FindAll(".blazored-toast-message").Select(x => x.TextContent).ToArray();

    [Fact]
    public void Dispose_UnsubscribesFromAllToastServiceEvents()
    {
        var cut = RenderToasts();
        Assert.NotEmpty(SubscribedEvents(_toastService));

        DisposeComponents();

        Assert.Empty(SubscribedEvents(_toastService));
        Assert.Null(Record.Exception(() => _toastService.ShowInfo("after dispose")));
        _ = cut;
    }

    [Fact]
    public void FailedInitialization_DoesNotLeaveToastServiceSubscriptions()
    {
        Assert.ThrowsAny<Exception>(() => RenderComponent<BlazoredToasts>(p => p
            .Add(x => x.IconType, Configuration.IconType.Custom)));

        Assert.Empty(SubscribedEvents(_toastService));
    }

    [Fact]
    public void DuplicateClose_DoesNotExceedMaxToastCount()
    {
        var cut = RenderToasts(maxToastCount: 1);
        _toastService.ShowInfo("a");
        _toastService.ShowInfo("b");
        _toastService.ShowInfo("c");
        var first = cut.FindComponent<BlazoredToast>().Instance;

        // e.g. the user clicks the close button just as the timeout elapses
        cut.InvokeAsync(() => { first.Close(); first.Close(); });

        Assert.Equal(new[] { "b" }, Messages(cut));
    }

    [Fact]
    public void NavigationClear_FillsAllFreeSlotsFromQueue()
    {
        var cut = RenderToasts(maxToastCount: 2, removeOnNavigation: true);
        foreach (var m in new[] { "a", "b", "c", "d", "e" })
        {
            _toastService.ShowInfo(m);
        }

        Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/next");

        Assert.Equal(new[] { "c", "d" }, Messages(cut));
    }

    [Fact]
    public void ClearToastsByLevel_PromotesQueuedToasts()
    {
        var cut = RenderToasts(maxToastCount: 1);
        _toastService.ShowInfo("info");
        _toastService.ShowError("error");

        _toastService.ClearInfoToasts();

        Assert.Equal(new[] { "error" }, Messages(cut));
    }

    [Fact]
    public void ClearCustomToasts_PromotesQueuedToasts()
    {
        var cut = RenderToasts(maxToastCount: 1);
        _toastService.ShowToast<ToastServiceTests.MyTestComponent>();
        _toastService.ShowInfo("queued");
        Assert.Empty(Messages(cut));

        _toastService.ClearCustomToasts();

        Assert.Equal(new[] { "queued" }, Messages(cut));
    }
}
