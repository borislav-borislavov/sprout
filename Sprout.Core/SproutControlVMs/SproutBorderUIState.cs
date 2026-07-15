using CommunityToolkit.Mvvm.ComponentModel;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutBorderUIState : BaseSproutControlVM
    {
        public SproutBorderUIState(string name) : base(name)
        {
            
        }

        public void SetUpState(string controlName)
        {
            this.Name = controlName;
        }
    }
}
