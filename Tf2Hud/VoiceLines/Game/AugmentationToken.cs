using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CriticalCommonLib.Models;

namespace Tf2Hud.VoiceLines.Game;

public static class AugmentationToken
{
    private const uint MoonshineBrine = 38386u;
    private const uint MoonshineTwine = 37387u;
    private const uint MoonshineShine = 37388u;

    private const uint AbyssosMythosTwo = 38382u;
    private const uint AbyssosMythosThree = 38383u;

    private const uint AglaiaCoin = 36820u;
    private const uint EuphrosyneCoin = 38950u;

    private static readonly ImmutableHashSet<uint> ExchangeableForTokens =
        new[] { AbyssosMythosTwo, AbyssosMythosThree, AglaiaCoin, EuphrosyneCoin }.ToImmutableHashSet();


    private static readonly ImmutableDictionary<uint, ImmutableHashSet<Recipe>>
        RecipesForTokens = new[]
        {
            // Moonshine Brine
            new KeyValuePair<uint, ImmutableHashSet<Recipe>>(MoonshineBrine, new[]
            {
                // With Books
                new Recipe(new Component(AbyssosMythosThree, 4))
            }.ToImmutableHashSet()),
            // Moonshine Twine
            new KeyValuePair<uint, ImmutableHashSet<Recipe>>(MoonshineTwine, new[]
            {
                // With Books
                new Recipe(new Component(AbyssosMythosThree, 4)),
                // With Coins
                new Recipe(new Component(AglaiaCoin, 1), new Component(EuphrosyneCoin, 1))
            }.ToImmutableHashSet()),
            // Moonshine Shine
            new KeyValuePair<uint, ImmutableHashSet<Recipe>>(MoonshineShine, new[]
            {
                // With Books
                new Recipe(new Component(AbyssosMythosTwo, 4)),
                // With Coins
                new Recipe(new Component(AglaiaCoin, 1), new Component(EuphrosyneCoin, 1))
            }.ToImmutableHashSet())
        }.ToImmutableDictionary();

    public static bool IsToken(uint itemId)
    {
        return RecipesForTokens.ContainsKey(itemId);
    }

    public static bool IsExchangeableForToken(uint itemId)
    {
        return ExchangeableForTokens.Contains(itemId);
    }

    public static bool HasEnoughForToken(
        Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories)
    {
        var temp = inventories[CriticalCommonLib.Service.ClientState.LocalContentId]
                   .SelectMany(i => i.Value)
                   .Select(i => new KeyValuePair<uint, uint>(i.ItemId, i.Quantity));
        var idsAndQuantities = new Dictionary<uint, uint>();
        foreach (var (itemId, quantity) in temp)
        {
            idsAndQuantities.TryGetValue(itemId, out var savedQuantity);
            idsAndQuantities[itemId] = savedQuantity + quantity;
        }

        // For each token
        foreach (var recipes in RecipesForTokens.Values)
            // For each recipe/combination
        foreach (var recipe in recipes)
            if (IsRecipeFulfillable(recipe, idsAndQuantities))
                return true;
        return false;
    }

    private static bool IsRecipeFulfillable(Recipe recipe, IReadOnlyDictionary<uint, uint> idsAndQuantities)
    {
        // For each component in the recipe
        foreach (var (itemId, quantityNeeded) in recipe)
            // If the user does not have the item or the quantity needed
            if (!idsAndQuantities.TryGetValue(itemId, out var quantityHad) || quantityHad < quantityNeeded)
            {
                // Then that recipe can't be fulfilled
                return false;
            }

        // If we didn't return false ever, then it is fulfillable
        return true;
    }

    public class Component
    {
        public readonly uint ItemId;
        public readonly uint Quantity;

        public Component(uint itemId, uint quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }

        public void Deconstruct(out uint itemId, out uint quantityNeeded)
        {
            itemId = ItemId;
            quantityNeeded = Quantity;
        }
    }

    public class Recipe
    {
        public readonly ImmutableHashSet<Component> Components;

        public Recipe(params Component[] components)
        {
            Components = components.ToImmutableHashSet();
        }

        public IEnumerator<Component> GetEnumerator()
        {
            return Components.GetEnumerator();
        }
    }
}
