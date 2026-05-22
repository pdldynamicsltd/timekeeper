using System;
using System.Net;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadentManagement.Chat;
using CadentManagement.Storage;
using CadentManagement.Storage.FileValidator;

namespace CadentManagement.Web.Controllers;

[AbpMvcAuthorize]
public class ChatController : ChatControllerBase
{
    public ChatController(IBinaryObjectManager binaryObjectManager, IChatMessageManager chatMessageManager, IFileValidatorManager fileValidatorManager) :
        base(binaryObjectManager, chatMessageManager, fileValidatorManager)
    {
    }

    public async Task<ActionResult> GetUploadedObject(Guid fileId, string fileName, string contentType)
    {
        using (CurrentUnitOfWork.SetTenantId(null))
        {
            var fileObject = await BinaryObjectManager.GetOrNullAsync(fileId);
            if (fileObject == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            return File(fileObject.Bytes, contentType, fileName);
        }
    }
}

