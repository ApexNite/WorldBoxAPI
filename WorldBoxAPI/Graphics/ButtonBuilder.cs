﻿using System;
using UnityEngine;
using UnityEngine.UI;
using WorldBoxAPI.BepInEx;
using WorldBoxAPI.Constants;
using WorldBoxAPI.ResourceTools;

namespace WorldBoxAPI.Graphics {
    public class ButtonBuilder {
        private bool ActionSet { get; set; }
        private string DescriptionKey { get; set; }
        private GodPower GodPower { get; set; }
        private Sprite Icon { get; set; }
        private string Id { get; set; }
        private ButtonRow Row { get; set; }
        private Vector2 Scale { get; set; }
        private int Section { get; set; }
        private ButtonStyle Style { get; set; }
        private TabManager Tab { get; set; }
        private string TitleKey { get; set; }
        private PowerButtonType Type { get; set; }
        private WindowManager Window { get; set; }
        private string WindowId { get; set; }

        /// <summary>
        /// Initialzes a new instance of the ButtonBuilder class using the specfied id and type.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="style"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder(string id, ButtonStyle style) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!Enum.IsDefined(typeof(ButtonStyle), style)) {
                throw new ArgumentException($"Position \"{style}\" is not defined by TabPosition.", nameof(style));
            }

            Next(id, style);
        }

        /// <summary>
        /// Add button to a custom tab by name.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        public ButtonBuilder AddToTab(string id) {
            AddToTab(id, Section);
            return this;
        }

        /// <summary>
        /// Add button to preexisting tab. Not implemented.
        /// </summary>
        /// <param name="tab"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public ButtonBuilder AddToTab(Tabs tab) {
            throw new NotImplementedException("Adding buttons to vanilla tabs is currently unsupported.");
        }

        /// <summary>
        /// Add button to a custom tab and section by name.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder AddToTab(string id, int section) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!TabManager.Tabs.ContainsKey(id)) {
                throw new ArgumentException($"Tab \"{id}\" was not able to be found.", nameof(id));
            }

            Tab = TabManager.Tabs[id];
            Section = section;
            return this;
        }

        /// <summary>
        /// Add button to a preexisting tab and section by name. Not implemented.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="section"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public ButtonBuilder AddToTab(Tabs tab, int section) {
            throw new NotImplementedException("Adding buttons to vanilla tabs is currently unsupported.");
        }

        public ButtonBuilder AddToWindow(string id) {
            throw new NotImplementedException("Adding buttons to windows is currently unsupported");
        }

        public ButtonBuilder AddToWindow(Windows window) {
            throw new NotImplementedException("Adding buttons to windows is currently unsupported");
        }

        /// <summary>
        /// Build the button.
        /// </summary>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public ButtonBuilder Build() {
            if (Tab is null && Window is null) {
                throw new NullReferenceException("Button cannot be built without a parent set.");
            }

            if (!ActionSet) {
                Plugin.Logger.LogWarning($"Button \"{Id}\" has no action!");
            }

            // Create GameObjects, button has special name so it doesn't error (nobody sees this)
            GameObject buttonObject = new GameObject(Type == PowerButtonType.Active ? GodPower.id : "Button WBAPI");
            GameObject iconObject = new GameObject("Icon");

            // Add components 
            Image image = buttonObject.AddComponent<Image>();
            TipButton tipButton = buttonObject.AddComponent<TipButton>();
            PowerButton powerButton = buttonObject.AddComponent<PowerButton>();
            Image icon = iconObject.AddComponent<Image>();
            buttonObject.AddComponent<Button>();

            // Set parent
            iconObject.transform.SetParent(buttonObject.transform);

            // Size button
            buttonObject.GetComponent<RectTransform>().sizeDelta = Scale;
            iconObject.GetComponent<RectTransform>().sizeDelta = Scale - new Vector2(UI.BUTTON_ICON_PADDING, UI.BUTTON_ICON_PADDING);
            buttonObject.transform.localScale = Vector3.one;
            iconObject.transform.localScale = Vector3.one;

            // Get button style
            switch (Style) {
                case ButtonStyle.Small:
                    image.sprite = Resources.Load<Sprite>("ui/button");
                    break;
                case ButtonStyle.Medium:
                    image.sprite = Resources.Load<Sprite>("ui/buttonMedium");
                    break;
                case ButtonStyle.Long:
                    image.sprite = Resources.Load<Sprite>("ui/buttonLong");
                    break;
                case ButtonStyle.SpecialRed:
                    image.sprite = AtlasTool.GetSprite("special_buttonRed", AtlasType.SpriteAtlasUI);
                    image.type = Image.Type.Sliced;
                    break;
                case ButtonStyle.SpecialRedBorder:
                    image.sprite = AtlasTool.GetSprite("special_buttonRed_insides", AtlasType.SpriteAtlasUI);
                    image.type = Image.Type.Sliced;
                    break;
            }

            // Add icon
            icon.sprite = Icon;

            // Setup tip so it uses the custom key and has the hotkey in its description
            tipButton.textOnClick = TitleKey;
            tipButton.textOnClickDescription = DescriptionKey;

            // Setup PowerButton
            powerButton.type = Type;
            powerButton.icon = icon;

            // Apply action
            switch (Type) {
                case PowerButtonType.Active:
                    powerButton.godPower = GodPower;
                    break;
                case PowerButtonType.Window:
                    powerButton.open_window_id = WindowId;
                    break;
            }

            // Add button to tab
            Tab.AddButton(buttonObject, Section, Row);

            return this;
        }

        /// <summary>
        /// Resets ButtonBuilder to create a new button. Same as creating a new ButtonBuilder object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder Next(string id, ButtonStyle style) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!Enum.IsDefined(typeof(ButtonStyle), style)) {
                throw new ArgumentException($"Position \"{style}\" is not defined by TabPosition.", nameof(style));
            }

            ActionSet = false;
            DescriptionKey = $"{id}_description";
            Icon = Resources.Load<Sprite>("WorldBoxAPI/UI/Icons/IconTemp");
            Id = id;
            Scale = StyleToScale(style);
            Type = (PowerButtonType) (-1);
            Style = style;
            TitleKey = id;
            WindowId = string.Empty;
            Section = 1;
            return this;
        }

        /// <summary>
        /// Set button GodPower from id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder SetGodPower(string id) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!AssetManager.powers.dict.ContainsKey(id)) {
                throw new ArgumentException($"GodPower with id \"{id}\" does not exist.", nameof(id));
            }

            SetGodPower(AssetManager.powers.get(id));
            return this;
        }

        /// <summary>
        /// Set button GodPower from an object.
        /// </summary>
        /// <param name="godPower"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ButtonBuilder SetGodPower(GodPower godPower) {
            _ = godPower ?? throw new ArgumentNullException(nameof(godPower));

            Type = PowerButtonType.Active;
            ActionSet = true;
            GodPower = godPower;
            return this;
        }

        /// <summary>
        /// Set button height
        /// </summary>
        /// <param name="height"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public ButtonBuilder SetHeight(float height) {
            Scale = new Vector2(Scale.x, height);
            return this;
        }

        /// <summary>
        /// Set button icon from path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ButtonBuilder SetIcon(string path) {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            Icon = Resources.Load<Sprite>(path);
            return this;
        }

        /// <summary>
        /// Set button icon from sprite.
        /// </summary>
        /// <param name="icon"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ButtonBuilder SetIcon(Sprite icon) {
            _ = icon ?? throw new ArgumentNullException(nameof(icon));
            Icon = icon;
            return this;
        }

        /// <summary>
        /// Set which row the button will appear on in a tab.
        /// </summary>
        /// <param name="row"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder SetRow(ButtonRow row) {
            if (!Enum.IsDefined(typeof(ButtonRow), row)) {
                throw new ArgumentException($"Row \"{row}\" is not defined by ButtonRow.", nameof(row));
            }

            Row = row;
            return this;
        }

        /// <summary>
        /// Set button scale.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        public ButtonBuilder SetScale(Vector2 scale) {
            Scale = scale;
            return this;
        }

        /// <summary>
        /// Set button section within tab.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder SetSection(int section) {
            if (section < 0) {
                throw new ArgumentException("Section cannot be less than 0.", nameof(section));
            }

            Section = section;
            return this;
        }

        /// <summary>
        /// Set button width.
        /// </summary>
        /// <param name="width"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        public ButtonBuilder SetWidth(float width) {
            Scale = new Vector2(width, Scale.y);
            return this;
        }

        /// <summary>
        /// Change window to open on click.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The ButtonBuilder instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ButtonBuilder SetWindowId(string id) {
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!ScrollWindow.allWindows.ContainsKey(id)) {
                ScrollWindow original = Resources.Load<ScrollWindow>($"windows/{id}");
                _ = original ?? throw new ArgumentException($"Window with id \"{id}\" does not exist.", nameof(id));
            }

            Type = PowerButtonType.Window;
            ActionSet = true;
            WindowId = id;
            return this;
        }

        private Vector2 StyleToScale(ButtonStyle style) {
            switch (style) {
                case ButtonStyle.Small:
                case ButtonStyle.SpecialRed:
                case ButtonStyle.SpecialRedBorder:
                    return UI.SMALL_BUTTON_SIZE;
                case ButtonStyle.Medium:
                    return UI.MEDIUM_BUTTON_SIZE;
                case ButtonStyle.Long:
                    return UI.LONG_BUTTON_SIZE;
                default:
                    return Vector2.zero;
            }
        }
    }
}
