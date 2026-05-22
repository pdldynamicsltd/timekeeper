import { BaseReactPage, Visibility } from "../../shared/base-page";

export class WebhookSubscriptionPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/webhook-subscriptions');
    }

    async waitForTableContent(tableSelector = '.ant-table', state: Visibility = 'visible') {
        await Promise.race([
            this.page.waitForSelector(`${tableSelector} .ant-table-tbody tr.ant-table-row`, { state }),
            this.page.waitForSelector(`${tableSelector} .ant-table-tbody .ant-empty`, { state }),
        ]);
        await this.page.waitForTimeout(500);
    }
}
