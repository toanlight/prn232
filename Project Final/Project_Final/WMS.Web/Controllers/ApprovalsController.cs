using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

[Authorize]
public class ApprovalsController : Controller
{
    private readonly IWmsApiClient _apiClient;

    public ApprovalsController(IWmsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Inbox()
    {
        var response = await _apiClient.GetAsync<ApiResponseModel<List<ApprovalItemViewModel>>>("api/approvals/pending");
        var approvals = response?.Data ?? new List<ApprovalItemViewModel>();
        return View(approvals);
    }
}
