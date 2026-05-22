import { BaseReactPage } from "../../shared/base-page";

export class EditionsPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/editions');
    }
}
