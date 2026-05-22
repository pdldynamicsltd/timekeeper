import { BaseReactPage, Visibility } from "../../shared/base-page";

export class LanguagesPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/languages');
    }

    async waitForTableContent(tableSelector = '.table-responsive table', state: Visibility = 'visible') {
        await this.page.waitForSelector(`${tableSelector} tbody tr`, { state });
        await this.page.waitForTimeout(500);
    }
}
