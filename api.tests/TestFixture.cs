using System;
using System.IO;
using Xunit;

/// <summary>
/// Sets up a Cards-PNG directory in the current working directory so Lobby can be constructed in tests.
/// Shared across all test classes via collection fixture.
/// </summary>
public class CardFixture : IDisposable
{
    public CardFixture()
    {
        var cardsDir = Path.Combine(Environment.CurrentDirectory, "Cards-PNG");
        if (!Directory.Exists(cardsDir))
        {
            Directory.CreateDirectory(cardsDir);

            for (int i = 1; i <= 20; i++)
                File.WriteAllBytes(Path.Combine(cardsDir, $"Core-Black-{i}.png"), new byte[] { 0 });

            for (int i = 1; i <= 100; i++)
                File.WriteAllBytes(Path.Combine(cardsDir, $"Core-White-{i}.png"), new byte[] { 0 });
        }
    }

    public void Dispose()
    {
        var cardsDir = Path.Combine(Environment.CurrentDirectory, "Cards-PNG");
        try { Directory.Delete(cardsDir, true); } catch { }
    }
}

[CollectionDefinition("CardTests")]
public class CardTestCollection : ICollectionFixture<CardFixture> { }
