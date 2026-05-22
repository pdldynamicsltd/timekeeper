import { BaseReactPage } from "../../shared/base-page";

export class VisualSettingsPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/ui-customization');
    }
}
