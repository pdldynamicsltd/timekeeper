using Abp.DynamicEntityProperties;
using Abp.UI.Inputs;
using CadentManagement.Authorization.Users;
using CadentManagement.CustomInputTypes;

namespace CadentManagement.DynamicEntityProperties;

public class AppDynamicEntityPropertyDefinitionProvider : DynamicEntityPropertyDefinitionProvider
{
    public override void SetDynamicEntityProperties(IDynamicEntityPropertyDefinitionContext context)
    {
        context.Manager.AddAllowedInputType<SingleLineStringInputType>();
        context.Manager.AddAllowedInputType<ComboboxInputType>();
        context.Manager.AddAllowedInputType<CheckboxInputType>();
        context.Manager.AddAllowedInputType<MultiSelectComboboxInputType>();

        //Add entities here 
        context.Manager.AddEntity<User, long>();
    }
}

