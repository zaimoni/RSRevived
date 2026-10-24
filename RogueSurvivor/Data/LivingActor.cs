using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Data
{
  /// <summary>An actor whose model has an inventory (Abilities.HasInventory): the living, humans and dogs.  The undead,
  /// rats and the Sewers Thing are plain Actors.  The members that use the inventory still live in Actor, behind its
  /// virtual Inventory and InventorySlots; they move here next.</summary>
  [Serializable]
  public sealed class LivingActor : Actor, IJsonStaged<LivingActor>
  {
    private readonly Inventory m_Inventory; // hammerspace inventory
    private readonly InventorySlots m_InventorySlots; // Angband-ish inventory

    public override Inventory? Inventory => m_Inventory;
    public override InventorySlots? InventorySlots => m_InventorySlots;

    public LivingActor(ActorModel model, Faction faction, int spawnTime, string name = "") : base(model, faction, spawnTime, name, true)
    {
#if DEBUG
      if (!model.Abilities.HasInventory) throw new ArgumentException(model.Name + " has no inventory: wants Actor", nameof(model));
#endif
      m_Inventory = new Inventory(model.BaseInventoryCapacity);
      m_InventorySlots = new InventorySlots(1);   // just human-torso slot, for now
      InstallDefaultController();
    }

    // Implied when omitted: an empty inventory of the model's capacity, empty slots.
    private LivingActor(JsonElement src, JsonSerializerOptions options) : base(src, options)
    {
      m_Inventory = src.TryGetProperty("Inventory", out var relay) ? (relay.Deserialize<Inventory>(options) ?? throw new JsonException("null Inventory"))
                  : new Inventory(Model.BaseInventoryCapacity);
      m_InventorySlots = src.TryGetProperty("Slots", out relay) ? new InventorySlots(relay, options) : new InventorySlots(1);
      FinishLoad(src, options);
    }

    public static LivingActor fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    protected override void toJsonInventory(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WritePropertyName("Inventory");
      JsonSerializer.Serialize(writer, m_Inventory, options);
      if (!m_InventorySlots.IsEmpty) {
        writer.WritePropertyName("Slots");
        m_InventorySlots.toJson(writer, options);
      }
    }
  }
}
