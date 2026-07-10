using CommunityToolkit.Mvvm.ComponentModel;

namespace Sprout.Core.UIStates
{
    public partial class SproutBorderUIState : BaseSproutControlVM
    {
        public void SetUpState(string controlName)
        {
            this.Name = controlName;
        }
    }
}
