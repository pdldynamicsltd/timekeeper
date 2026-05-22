import { BaseReactPage } from "../../shared/base-page";

export class OrganizationUnitPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/organization-units');
    }
}
