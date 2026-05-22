import { BaseReactPage } from "../../shared/base-page";

export class TenantsPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/tenants');
    }
}
