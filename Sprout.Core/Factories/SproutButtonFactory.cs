using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Features.ButtonActions.Actions;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.ViewModels;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.Factories
{
    public class SproutButtonFactory : BaseSproutControlFactory, ISproutButtonFactory
    {
        private readonly IClipboardService _clipboardService;
        private readonly IDataAdapterFactory _dataAdapterFactory;

        public SproutButtonFactory(IClipboardService clipboardService, IDataAdapterFactory dataAdapterFactory)
        {
            _clipboardService = clipboardService;
            _dataAdapterFactory = dataAdapterFactory;
        }

        public SproutButton Create(SproutButtonConfig sproutButtonConfig)
        {
            var sproutButton = new SproutButton
            {
                Name = sproutButtonConfig.Name,
                Config = sproutButtonConfig,
                VM = new SproutButtonVM(sproutButtonConfig.Name)
            };

            sproutButton.ButtonContent = sproutButtonConfig.Content ?? string.Empty;
            sproutButton.Icon = sproutButtonConfig.Icon ?? string.Empty;
            sproutButton.IconFont = sproutButtonConfig.IconFont ?? "Segoe MDL2 Assets";
            sproutButton.IconFontSize = sproutButtonConfig.IconFontSize ?? 14.0;
            sproutButton.TextFontSize = sproutButtonConfig.TextFontSize ?? 12.0;
            sproutButton.BrushThickness = sproutButtonConfig.BrushThickness;
            sproutButton.ForegroundColor = sproutButtonConfig.ForegroundColor;

            if (sproutButtonConfig.Height.HasValue)
                sproutButton.button.Height = sproutButtonConfig.Height.Value;

            if (sproutButtonConfig.Width.HasValue)
                sproutButton.button.Width = sproutButtonConfig.Width.Value;

            if (!string.IsNullOrWhiteSpace(sproutButtonConfig.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(sproutButtonConfig.Margin) is Thickness margin)
                    sproutButton.Margin = margin;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.HorizontalAlignment) &&
                sproutButtonConfig.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(sproutButtonConfig.HorizontalAlignment, out var hAlign))
            {
                sproutButton.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.VerticalAlignment) &&
                sproutButtonConfig.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(sproutButtonConfig.VerticalAlignment, out var vAlign))
            {
                sproutButton.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.ToolTip))
                sproutButton.ToolTip = sproutButtonConfig.ToolTip;

            if (!string.IsNullOrWhiteSpace(sproutButtonConfig.Padding))
            {
                if (new ThicknessConverter().ConvertFromString(sproutButtonConfig.Padding) is Thickness padding)
                {
                    if (string.IsNullOrEmpty(sproutButtonConfig.Icon))
                    {
                        sproutButton.textBlock.Padding = padding;
                    }
                    else
                    {
                        var leftPadding = padding.Left; //save left padding for later
                        padding.Left = 0; //don't add extra padding between icon and text
                        sproutButton.textBlock.Padding = padding;
                        padding.Left = leftPadding; //restore padding to be added to icon

                        padding.Right = 0; //dont add extra padding between icon and text
                        sproutButton.iconBlock.Padding = padding;
                    }
                }
            }

            SetUpVM(sproutButton);

            SetPositionInGrid(sproutButton, sproutButtonConfig);

            return sproutButton;
        }

        private void SetUpVM(SproutButton sproutButton)
        {
            var compositeAction = new CompositeButtonAction();

            foreach (var actionConfig in sproutButton.Config.Actions)
            {
                if (actionConfig is ExecuteUpdateActionConfig)
                {
                    compositeAction.Add(new ExecuteUpdateButtonAction(sproutButton.Name));
                }
                else if (actionConfig is RefreshDataGridActionConfig refreshConfig)
                {
                    compositeAction.Add(new RefreshDataGridButtonAction(refreshConfig.TargetDataGridName));
                }
                else if (actionConfig is ExecuteSelectActionConfig)
                {
                    compositeAction.Add(new ExecuteSelectButtonAction(sproutButton.Name));
                }
                else if (actionConfig is CopyToClipboardActionConfig copyConfig)
                {
                    compositeAction.Add(new CopyToClipboardButtonAction(copyConfig.ClipboardText, _clipboardService));
                }
            }

            sproutButton.BindButtonAction(sproutButton.button, compositeAction);
        }
    }
}
