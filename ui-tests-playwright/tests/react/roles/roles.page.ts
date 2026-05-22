import { BaseReactPage } from "../../shared/base-page";

export class RolesPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/roles');
    }
}
