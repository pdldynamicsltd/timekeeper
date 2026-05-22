import { BaseReactPage } from "../../shared/base-page";

export class LoginPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/account/login');
    }
}
