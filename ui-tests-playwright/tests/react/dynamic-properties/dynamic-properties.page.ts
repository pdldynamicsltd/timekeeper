import { BaseReactPage, Visibility } from "../../shared/base-page";

export class DynamicPropertiesPage extends BaseReactPage {

    async gotoPage() {
        await this.gotoUrl('/app/admin/dynamic-property');
    }

    async waitForTableContent(tableSelector = '.ant-table', state: Visibility = 'visible') {
        await Promise.race([
            this.page.waitForSelector(`${tableSelector} .ant-table-tbody tr.ant-table-row`, { state }),
            this.page.waitForSelector(`${tableSelector} .ant-table-tbody .ant-empty`, { state }),
        ]);
        await this.page.waitForTimeout(500);
    }

    async openAddNewPropertyModal() {
        await this.page.click(`text=/.*Add new dynamic property.*/i`);
    }

    async waitForCreateOrEditModal() {
        await this.waitForModal();
    }

    async openActionsDropdown(rowNumber = 1) {
        const row = this.page.locator('.ant-table-tbody tr.ant-table-row').nth(rowNumber - 1);
        await row.locator('.ant-dropdown-trigger').click();
    }

    async waitForDropdownMenu(state: Visibility = 'visible') {
        if (state === 'visible') {
            await this.page.waitForSelector('.ant-dropdown .ant-dropdown-menu', { state: 'visible' });
        } else {
            await this.page.waitForFunction(() => {
                const menus = document.querySelectorAll('.ant-dropdown:not(.ant-dropdown-hidden) .ant-dropdown-menu');
                return menus.length === 0;
            });
        }
        await this.page.waitForTimeout(500);
    }

    async triggerEditDropdown() {
        await this.triggerDropdownAction('Edit');
    }

    async triggerDeleteDropdown() {
        await this.triggerDropdownAction('Delete');
    }
}
