using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;
using UIInfoSuite2.Infrastructure;
using UIInfoSuite2.Infrastructure.Extensions;

namespace UIInfoSuite2.UIElements;

internal class ShowKrobusSprinkler : IDisposable
{
  #region Properties
  private bool _krobusSprinklerIsHere;
  private bool _krobusSprinklerIsVisited;
  private ClickableTextureComponent _krobusSprinklerIcon;

  private bool Enabled { get; set; }
  private bool HideWhenVisited { get; set; }

  private readonly IModHelper _helper;
  #endregion

  #region Lifecycle
  public ShowKrobusSprinkler(IModHelper helper)
  {
    _helper = helper;
  }

  public void Dispose()
  {
    ToggleOption(false);
  }

  public void ToggleOption(bool showKrobusSprinkler)
  {
    Enabled = showKrobusSprinkler;

    _helper.Events.Display.RenderingHud -= OnRenderingHud;
    _helper.Events.Display.RenderedHud -= OnRenderedHud;
    _helper.Events.Display.MenuChanged -= OnMenuChanged;
    _helper.Events.GameLoop.DayStarted -= OnDayStarted;

    if (showKrobusSprinkler)
    {
      UpdateKrobusStore();

      _helper.Events.Display.RenderingHud += OnRenderingHud;
      _helper.Events.Display.RenderedHud += OnRenderedHud;
      _helper.Events.Display.MenuChanged += OnMenuChanged;
      _helper.Events.GameLoop.DayStarted += OnDayStarted;
    }
  }

  public void ToggleHideWhenVisitedOption(bool hideWhenVisited)
  {
    HideWhenVisited = hideWhenVisited;
    ToggleOption(Enabled);
  }
  #endregion

  #region Event subscriptions
  private void OnDayStarted(object? sender, EventArgs e)
  {
    UpdateKrobusStore();
  }

  private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
  {
    if (e.NewMenu is ShopMenu && Game1.currentLocation.Name == "Sewer")
    {
      _krobusSprinklerIsVisited = true;
    }
  }

  private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
  {
    // Draw icon
    if (UIElementUtils.IsRenderingNormally() && ShouldDrawIcon())
    {
      Point iconPosition = IconHandler.Handler.GetNewIconPosition();
      _krobusSprinklerIcon = new ClickableTextureComponent(
        new Rectangle(iconPosition.X, iconPosition.Y, 40, 40),
        Game1.objectSpriteSheet,
        new Rectangle(16 * 21, 16 * 26, 16, 16),
        2.5f
      );
      _krobusSprinklerIcon.draw(Game1.spriteBatch);
    }
  }

  private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
  {
    // Show text on hover
    if (ShouldDrawIcon() && (_krobusSprinklerIcon?.containsPoint(Game1.getMouseX(), Game1.getMouseY()) ?? false))
    {
      string hoverText = _helper.SafeGetString(LanguageKeys.KrobusSprinkler);
      IClickableMenu.drawHoverText(Game1.spriteBatch, hoverText, Game1.dialogueFont);
    }
  }
  #endregion

  #region Logic
  private void UpdateKrobusStore()
  {
    _krobusSprinklerIsHere = ShopBuilder.GetShopStock("ShadowShop").Keys.FirstOrDefault(elem => elem.QualifiedItemId == "(O)645") != null;
    _krobusSprinklerIsVisited = false;
  }

  private bool ShouldDrawIcon()
  {
    return _krobusSprinklerIsHere && (!_krobusSprinklerIsVisited || !HideWhenVisited) && Game1.player.eventsSeen.Contains("66");
  }
  #endregion
}
