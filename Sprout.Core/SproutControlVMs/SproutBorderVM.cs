using CommunityToolkit.Mvvm.ComponentModel;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutBorderVM : BaseSproutControlVM
    {
        public SproutBorderVM(string name) : base(name)
        {
            
        }

        public void SetUpState(string controlName)
        {
            this.Name = controlName;
        }
    }
}
