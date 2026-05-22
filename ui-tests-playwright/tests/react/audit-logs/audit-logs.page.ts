import { BaseReactPage } from "../../shared/base-page";

export class AuditLogsPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/audit-logs');
    }
}
