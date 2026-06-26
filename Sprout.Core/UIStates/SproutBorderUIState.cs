using CommunityToolkit.Mvvm.ComponentModel;

namespace Sprout.Core.UIStates
{
    public partial class SproutBorderUIState : BaseUIState
    {
        public void SetUpState(string controlName)
        {
            this.Name = controlName;
        }
    }
}
