import { BaseReactPage } from "../../shared/base-page";

export class MaintenancePage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/maintenance');
    }
}
