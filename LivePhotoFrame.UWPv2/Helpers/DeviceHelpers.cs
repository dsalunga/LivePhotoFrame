using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace LivePhotoFrame.UWP.Helpers
{
    public class DeviceHelpers
    {
        //...
        public static void SetBrightness(double targetBrightness)
        {
            // Create BrightnessOverride object
            BrightnessOverride bo = BrightnessOverride.GetForCurrentView();
            // BrightnessOverride bo = BrightnessOverride.GetForCurrentView();
            // BrightnessOverride bo = BrightnessOverride.GetDefaultForSystem();

            if (bo.IsSupported)
            {
                // Set override brightness to full brightness even when battery is low
                //bo.SetBrightnessScenario(DisplayBrightnessScenario.FullBrightness, DisplayBrightnessOverrideOptions.None);
                bo.SetBrightnessLevel(targetBrightness, DisplayBrightnessOverrideOptions.None);

                // Request to start the overriding process
                bo.StartOverride();
            }
        }
    }
}
