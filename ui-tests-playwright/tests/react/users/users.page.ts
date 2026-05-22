import { BaseReactPage } from "../../shared/base-page";

export class UsersPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/users');
    }
}
