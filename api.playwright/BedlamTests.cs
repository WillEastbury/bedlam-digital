using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

namespace api.playwright;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class BedlamTests : PageTest
{
    private const string BaseUrl = "https://bedlamdigital.azurewebsites.net";

    [Test]
    public async Task PageLoadsWithCorrectTitle()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page).ToHaveTitleAsync("Bedlam");
    }

    [Test]
    public async Task NavbarRendersWithBedlamOnlineText()
    {
        await Page.GotoAsync(BaseUrl);
        var brand = Page.Locator("a.navbar-brand");
        await Expect(brand).ToBeVisibleAsync();
        await Expect(brand).ToContainTextAsync("Bedlam Online");
    }

    [Test]
    public async Task LobbiesTabIsVisibleAndActiveByDefault()
    {
        await Page.GotoAsync(BaseUrl);
        var lobbiesLink = Page.Locator("a.nav-link", new() { HasTextString = "Lobbies" });
        await Expect(lobbiesLink).ToBeVisibleAsync();
        await Expect(lobbiesLink).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("active"));

        var lobbiesPane = Page.Locator("#lobbies-tab");
        await Expect(lobbiesPane).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("show active"));
    }

    [Test]
    public async Task ThemeSelectorDropdownHasFiveOptions()
    {
        await Page.GotoAsync(BaseUrl);
        var selector = Page.Locator("#theme-selector");
        await Expect(selector).ToBeVisibleAsync();

        var options = selector.Locator("option");
        await Expect(options).ToHaveCountAsync(5);
    }

    [Test]
    public async Task CssStylesheetLoadsCorrectly()
    {
        await Page.GotoAsync(BaseUrl);
        var themeLink = Page.Locator("#theme-css");
        await Expect(themeLink).ToHaveAttributeAsync("href", new System.Text.RegularExpressions.Regex(@"/css/\w+\.min\.css"));
    }

    [Test]
    public async Task ThemeSwitchingChangesStylesheetHref()
    {
        await Page.GotoAsync(BaseUrl);
        var themeLink = Page.Locator("#theme-css");

        var initialHref = await themeLink.GetAttributeAsync("href");

        await Page.SelectOptionAsync("#theme-selector", "darkly");
        var newHref = await themeLink.GetAttributeAsync("href");

        Assert.That(newHref, Is.EqualTo("/css/darkly.min.css"));
        Assert.That(newHref, Is.Not.EqualTo(initialHref));
    }

    [Test]
    public async Task LobbiesLoadAfterApiCall()
    {
        await Page.GotoAsync(BaseUrl);
        var lobbyCards = Page.Locator("#lobbies-tab .card");
        await lobbyCards.First.WaitForAsync(new() { Timeout = 10000 });
        var count = await lobbyCards.CountAsync();
        Assert.That(count, Is.GreaterThan(0));
    }

    [Test]
    public async Task TabNavigationSwitchesContent()
    {
        await Page.GotoAsync(BaseUrl);

        // Click Play Bedlam tab
        await Page.Locator("#play-bedlam-tab").ClickAsync();
        var gameplayPane = Page.Locator("#gameplay-tab");
        await Expect(gameplayPane).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("show active"));

        // Click Game Diagnostics tab
        await Page.Locator("a.nav-link", new() { HasTextString = "Game Diagnostics" }).ClickAsync();
        var diagsPane = Page.Locator("#diags-tab");
        await Expect(diagsPane).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("show active"));

        // Click Lobbies tab to go back
        await Page.Locator("a.nav-link", new() { HasTextString = "Lobbies" }).ClickAsync();
        var lobbiesPane = Page.Locator("#lobbies-tab");
        await Expect(lobbiesPane).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("show active"));
    }

    [Test]
    public async Task StaticAssetsLoad()
    {
        var jsFiles = new[] { "/js/bootstrap.bundle.min.js", "/js/knockout-latest.min.js" };

        foreach (var jsFile in jsFiles)
        {
            var response = await Page.APIRequest.GetAsync(BaseUrl + jsFile);
            Assert.That(response.Status, Is.EqualTo(200), $"Failed to load {jsFile}");
        }
    }
}
