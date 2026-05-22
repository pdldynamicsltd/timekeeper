import { BaseReactPage } from "../../shared/base-page";

export class SettingsPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/settings/host');
    }
}
