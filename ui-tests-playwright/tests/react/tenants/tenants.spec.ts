import { Login } from '../utils/login';
import { test, expect, Page } from '@playwright/test';
import { TenantsPage } from './tenants.page';

test.describe('TENANTS', () => {

    let tenantsPage: TenantsPage;
    let page: Page;

    test.beforeAll(async ({ browser }) => {
        page = await browser.newPage();
        tenantsPage = new TenantsPage(page);

        await page.setViewportSize({
            width: 1920,
            height: 1080
        });

        const login = new Login(page);
        await login.login();
    });

    test.afterAll(async () => {
        await page.close();
    });

    test.describe('CRUD', () => {
        const TENANTS_CRUD_LIST = 'tenants.crud.010-list.png';
        const TENANTS_CRUD_NEW_MODAL = 'tenants.crud.020-new-modal.png';
        const TENANTS_CRUD_VALIDATION_SHOW = 'tenants.crud.030-validation-show.png';
        const TENANTS_CRUD_VALIDATION_HIDE = 'tenants.crud.040-validation-hide.png';
        const TENANTS_CRUD_NEW_SAVE = 'tenants.crud.050-new-save.png';
        const TENANTS_CRUD_ALREADY_EXISTED = 'tenants.crud.060-already-existed.png';
        const TENANTS_CRUD_ACTIONS = 'tenants.crud.070-actions.png';
        const TENANTS_CRUD_EDIT_MODAL = 'tenants.crud.080-edit-modal.png';
        const TENANTS_CRUD_EDIT_SAVE = 'tenants.crud.090-edit-save.png';
        const TENANTS_CRUD_DELETE_WARNING = 'tenants.crud.100-delete-warning.png';
        const TENANTS_CRUD_DELETE_CANCEL = 'tenants.crud.110-delete-cancel.png';
        const TENANTS_CRUD_DELETE_CONFIRM = 'tenants.crud.120-delete-confirm.png';

        /* Step 1 */
        test('should render the initial list', async () => {
            await tenantsPage.gotoPage();
            await tenantsPage.replaceWith('.card-body form', 'FORM_REPLACED_DUE_TO_DATES');
            await tenantsPage.waitForTableContent();
            await tenantsPage.replaceLastColoumnOfTable();
            await tenantsPage.wait(2000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_LIST);
        });

        /* Step 2 */
        test('should display modal on click to "New" button', async () => {
            await tenantsPage.clickByTextExact('Create new tenant');
            await tenantsPage.waitForModal();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_NEW_MODAL);
        });

        /* Step 3 */
        test('should keep "Save" button disabled when required inputs are not filled', async () => {
            await tenantsPage.wait(1000);

            const saveButton = page.locator('.ant-modal:not(.ant-modal-confirm) .btn.btn-primary.fw-bold');
            await expect(saveButton).toBeDisabled();

            await expect(page).toHaveScreenshot(TENANTS_CRUD_VALIDATION_SHOW);
        });

        /* Step 4 */
        test('should enable "Save" button when form is properly filled', async () => {
            await fillInputsWithValidValues();
            await tenantsPage.wait(1000);

            const saveButton = page.locator('.ant-modal:not(.ant-modal-confirm) .btn.btn-primary.fw-bold');
            await expect(saveButton).toBeEnabled();

            await expect(page).toHaveScreenshot(TENANTS_CRUD_VALIDATION_HIDE);
        });

        /* Step 5 */
        test('should save record when "Save" button is clicked', async () => {
            await tenantsPage.saveModal();
            await tenantsPage.waitForResponse();
            await tenantsPage.replaceLastColoumnOfTable();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_NEW_SAVE);
        });

        /* Step 6 */
        test('should give an error when trying to create an existing item', async () => {
            await tenantsPage.clickByTextExact('Create new tenant');
            await tenantsPage.waitForModal();
            await fillInputsWithValidValues();
            await tenantsPage.saveModal();
            await tenantsPage.waitForResponse();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_ALREADY_EXISTED);
        });

        /* Step 7 */
        test('should display actions on click to "Actions" button', async () => {
            await page.click('.swal2-actions .swal2-confirm');
            await tenantsPage.clickButtonByText('Cancel');
            await tenantsPage.openActionsDropdown(2);
            await tenantsPage.waitForDropdownMenu();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_ACTIONS);
        });

        /* Step 8 */
        test('should display modal on click to "Edit" button', async () => {
            await tenantsPage.triggerDropdownAction('Edit');
            await tenantsPage.waitForResponse();
            await tenantsPage.waitForModal();
            await tenantsPage.replaceLastColoumnOfTable();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_EDIT_MODAL);
        });

        /* Step 9 */
        test('should save changes to record when "Save" button is clicked', async () => {
            await tenantsPage.fillInputs({ 'input[name="name"]': 'changed_name' });
            await tenantsPage.saveModal();
            await tenantsPage.waitForResponse();
            await tenantsPage.replaceLastColoumnOfTable();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_EDIT_SAVE);
        });

        /* Step 10 */
        test('should display warning on click to "Delete" button', async () => {
            await tenantsPage.openActionsDropdown(2);
            await tenantsPage.waitForDropdownMenu();
            await tenantsPage.triggerDropdownAction('Delete');
            await tenantsPage.waitForConfirmationDialog();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_DELETE_WARNING);
        });

        /* Step 11 */
        test('should not delete record on click to "Cancel" button', async () => {
            await tenantsPage.cancelConfirmation();

            await expect(page).toHaveScreenshot(TENANTS_CRUD_DELETE_CANCEL);
        });

        /* Step 12 */
        test('should delete record on click to "Yes" button', async () => {
            await tenantsPage.openActionsDropdown(2);
            await tenantsPage.waitForDropdownMenu();
            await tenantsPage.triggerDropdownAction('Delete');
            await tenantsPage.waitForConfirmationDialog();
            await tenantsPage.confirmConfirmation();
            await tenantsPage.waitForResponse();
            await tenantsPage.replaceLastColoumnOfTable();
            await tenantsPage.wait(1000);

            await expect(page).toHaveScreenshot(TENANTS_CRUD_DELETE_CONFIRM);
        });

        function fillInputsWithValidValues() {
            return tenantsPage.fillInputs({
                'input[name="tenancyName"]': 'test',
                'input[name="name"]': 'test',
                'input[name="adminEmailAddress"]': 'test@test.com',
                'input[name="adminName"]': 'admin',
                'input[name="adminSurname"]': 'admin',
            });
        }
    });
});
